using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHttpContextAccessor(); 
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

builder.Services.AddAuthorization();

builder.Services.AddTransient<AuthTokenHandler>();

builder.Services.AddHttpClient("ApiClient", client =>
{
    // Configure BaseAddress if your API has one
    var apiBaseUrl = builder.Configuration["ApiSettings:AuthBaseUrl"]; // Example config key
    if (!string.IsNullOrEmpty(apiBaseUrl) && Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var baseUri))
    {
        // Set base address only if it's well-formed
        // client.BaseAddress = baseUri; // Or maybe just the host part depending on usage
    }
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<AuthTokenHandler>();


builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();


// --- DelegatingHandler Implementation (Place in a separate file, e.g., Handlers/AuthTokenHandler.cs) ---

public class AuthTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthTokenHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Try to get the access token associated with the current user's cookie session
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            // Ensure user is authenticated within the MVC app via cookie
            if (httpContext.User.Identity?.IsAuthenticated ?? false)
            {
                var accessToken = await httpContext.GetTokenAsync("access_token");

                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
            }
        }
        // Else: Handle cases where HttpContext might be null if used outside a request scope (less common for user-triggered API calls)


        // Proceed sending the request (now with the token if available)
        return await base.SendAsync(request, cancellationToken);

        // NOTE: Full refresh token logic is more complex.
        // You would catch a 401 response here, attempt refresh using GetTokenAsync("refresh_token"),
        // update stored tokens, and retry the request. This handler only adds the existing token.
    }
}