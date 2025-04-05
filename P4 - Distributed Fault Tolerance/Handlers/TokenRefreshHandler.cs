using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using P4___Distributed_Fault_Tolerance.Models;
using System.Net.Http.Headers;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;

namespace P4___Distributed_Fault_Tolerance.Handlers
{
    public class TokenRefreshHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;

        private const string RefreshClientName = "RefreshClient";

        public TokenRefreshHandler(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = _httpContextAccessor.HttpContext;
            var accessToken = context?.User?.FindFirst("AccessToken")?.Value;

            if (!string.IsNullOrEmpty(accessToken))
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();

                    // Read the token without validating the signature (we just want to read claims)
                    // This will throw an exception if the token string is not a valid JWT format.
                    var jwtToken = handler.ReadJwtToken(accessToken);

                    // Find the expiration claim (standard claim name is "exp")
                    var expiryClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Exp); // "exp"

                    if (expiryClaim != null && long.TryParse(expiryClaim.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long epochSeconds))
                    {
                        // Convert the Unix epoch seconds to a DateTimeOffset
                        var expiryTime = DateTimeOffset.FromUnixTimeSeconds(epochSeconds);

                        // Get the current UTC time
                        var currentTime = DateTimeOffset.UtcNow;

                        // Compare
                        bool isExpired = currentTime >= expiryTime;

                        Trace.WriteLine($"Access Token Expiry Claim ('exp'): {expiryClaim.Value} (Epoch Seconds)");
                        Trace.WriteLine($"Access Token Expires At (UTC): {expiryTime:o}"); // 'o' format is ISO 8601
                        Trace.WriteLine($"Current UTC Time: {currentTime:o}");
                        Trace.WriteLine($"Access Token Is Expired: {isExpired}");

                        // You can now use the 'isExpired' boolean or 'expiryTime' DateTimeOffset
                        // Example: Check if it expires within the next minute
                        if (!isExpired && expiryTime < currentTime.AddMinutes(1))
                        {
                            Trace.WriteLine("Access Token expires within the next minute.");
                        }
                    }
                    else
                    {
                        Trace.WriteLine("Access Token does not contain a valid 'exp' (expiration) claim.");
                    }
                }
                catch (ArgumentException ex)
                {
                    // Handle cases where the token string is malformed and cannot be read
                    Trace.WriteLine($"Error parsing access token: {ex.Message}. Token might not be a valid JWT.");
                }
                catch (Exception ex) // Catch other potential exceptions during parsing
                {
                    Trace.WriteLine($"An unexpected error occurred while reading access token expiry: {ex.Message}");
                }
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshToken = context?.User?.FindFirst("RefreshToken")?.Value;

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var newTokens = await RefreshTokensAsync(refreshToken, cancellationToken);

                    if (newTokens != null)
                    {
                        await UpdateAuthenticationCookie(context, newTokens);

                        var clonedRequest = await CloneHttpRequestMessageAsync(request); // Implement this helper

                        clonedRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newTokens.Token);

                        response = await base.SendAsync(clonedRequest, cancellationToken);
                    }
                    else
                    {
                        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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
                var refreshClient = _httpClientFactory.CreateClient(RefreshClientName);
                var refreshUrl = $"refresh";

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
                var newClaims = currentIdentity.Claims
                    .Where(c => c.Type != "AccessToken" && c.Type != "RefreshToken" && c.Type != "AccessTokenExpiry")
                    .ToList();

                newClaims.Add(new Claim("AccessToken", newTokens.Token));
                newClaims.Add(new Claim("RefreshToken", newTokens.RefreshToken));

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
                catch { } 

                var newIdentity = new ClaimsIdentity(newClaims, currentIdentity.AuthenticationType);
                var newPrincipal = new ClaimsPrincipal(newIdentity);

                var authProps = (await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme))?.Properties ?? new AuthenticationProperties();

                await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, newPrincipal, authProps);
            }
        }

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
