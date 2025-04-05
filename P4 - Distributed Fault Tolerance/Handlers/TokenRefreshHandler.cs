using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using P4___Distributed_Fault_Tolerance.Models;
using System.Net.Http.Headers;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace P4___Distributed_Fault_Tolerance.Handlers
{
    public class TokenRefreshHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;

        // Use a dedicated client name for the refresh call to avoid handler loop
        private const string RefreshClientName = "RefreshClient";

        public TokenRefreshHandler(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = configuration["ApiSettings:AuthBaseUrl"] ?? "https://localhost:5001/api/auth"; // Ensure this matches
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = _httpContextAccessor.HttpContext;
            var accessToken = context?.User?.FindFirst("AccessToken")?.Value;

            // Add current access token to outgoing request if available
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            // Send the request initially
            var response = await base.SendAsync(request, cancellationToken);

            // Check if response is Unauthorized (401)
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshToken = context?.User?.FindFirst("RefreshToken")?.Value;

                // Only attempt refresh if we have a refresh token
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    // Try to refresh the token
                    var newTokens = await RefreshTokensAsync(refreshToken, cancellationToken);

                    if (newTokens != null)
                    {
                        // Update the cookie with new tokens
                        await UpdateAuthenticationCookie(context, newTokens);

                        // Clone the original request
                        var clonedRequest = await CloneHttpRequestMessageAsync(request); // Implement this helper

                        // Add the *new* access token to the cloned request
                        clonedRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newTokens.Token);

                        // Re-send the request with the new token
                        response = await base.SendAsync(clonedRequest, cancellationToken);
                    }
                    else
                    {
                        // Refresh failed, sign out the user from the MVC app
                        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                        // Optionally redirect to login or return the original 401?
                        // Returning original 401 might be better API behavior.
                    }
                }
                else
                {
                    // No refresh token available, sign out user
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            }

            return response;
        }

        private async Task<RefreshTokenResponse?> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken)
        {
            try
            {
                var refreshClient = _httpClientFactory.CreateClient(RefreshClientName); // Use dedicated client
                var refreshUrl = $"{_apiBaseUrl}/refresh"; // Ensure this is your API's refresh endpoint

                var requestBody = new { RefreshToken = refreshToken };
                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await refreshClient.PostAsync(refreshUrl, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    var newTokens = JsonConvert.DeserializeObject<RefreshTokenResponse>(responseBody);
                    return newTokens;
                }

                return null; 
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task UpdateAuthenticationCookie(HttpContext context, RefreshTokenResponse newTokens)
        {
            var currentPrincipal = context.User;
            if (currentPrincipal?.Identity is ClaimsIdentity currentIdentity)
            {
                // Create a new list of claims, replacing token claims, keeping others
                var newClaims = currentIdentity.Claims
                    .Where(c => c.Type != "AccessToken" && c.Type != "RefreshToken" && c.Type != "AccessTokenExpiry")
                    .ToList();

                newClaims.Add(new Claim("AccessToken", newTokens.Token));
                newClaims.Add(new Claim("RefreshToken", newTokens.RefreshToken));

                // Add expiry from the new token
                try
                {
                    var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    var jwtToken = tokenHandler.ReadJwtToken(newTokens.Token);
                    var expiryClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp");
                    if (expiryClaim != null)
                    {
                        newClaims.Add(new Claim("AccessTokenExpiry", expiryClaim.Value, ClaimValueTypes.Integer64));
                    }
                }
                catch { } // Ignore if parsing fails

                var newIdentity = new ClaimsIdentity(newClaims, currentIdentity.AuthenticationType); // Preserve original scheme
                var newPrincipal = new ClaimsPrincipal(newIdentity);

                // Get existing auth properties to preserve settings like IsPersistent
                var authProps = (await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme))?.Properties ?? new AuthenticationProperties();

                // Re-issue the cookie with updated claims and tokens
                await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, newPrincipal, authProps);
            }
        }

        // Helper to clone HttpRequestMessage as it can only be sent once
        private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage req)
        {
            var clone = new HttpRequestMessage(req.Method, req.RequestUri)
            {
                Content = req.Content == null ? null : await CloneHttpContentAsync(req.Content),
                Version = req.Version
            };
            foreach (var header in req.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            foreach (var prop in req.Options)
            {
                clone.Options.Set(new HttpRequestOptionsKey<object?>(prop.Key), prop.Value);
            }

            return clone;
        }

        private static async Task<HttpContent?> CloneHttpContentAsync(HttpContent? content)
        {
            if (content == null) return null;

            var ms = new MemoryStream();
            await content.CopyToAsync(ms);
            ms.Position = 0;

            var clone = new StreamContent(ms);
            foreach (var header in content.Headers)
            {
                clone.Headers.Add(header.Key, header.Value);
            }
            return clone;
        }
    }
}
