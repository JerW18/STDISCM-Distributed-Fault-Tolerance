using Microsoft.AspNetCore.Authentication.Cookies;
using P4___Distributed_Fault_Tolerance.Handlers;

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

builder.Services.AddTransient<TokenRefreshHandler>();

builder.Services.AddHttpClient("AuthApiClient", client =>
{
    var authApiBaseUrl = builder.Configuration["ApiSettings:AuthBaseUrl"];
    client.BaseAddress = new Uri(authApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient("CourseApiClient", client =>
{
    var courseApiBaseUrl = builder.Configuration["ApiSettings:CourseBaseUrl"]; 
    client.BaseAddress = new Uri(courseApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<TokenRefreshHandler>();

builder.Services.AddHttpClient("GradeApiClient", client => 
{
    var gradeApiBaseUrl = builder.Configuration["ApiSettings:GradeBaseUrl"];
    client.BaseAddress = new Uri(gradeApiBaseUrl); 
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<TokenRefreshHandler>();

builder.Services.AddHttpClient("RefreshClient", client =>
{
    var authApiBaseUrl = builder.Configuration["ApiSettings:AuthBaseUrl"];
    client.BaseAddress = new Uri(authApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
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