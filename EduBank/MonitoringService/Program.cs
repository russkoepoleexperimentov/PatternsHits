using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using MonitoringService.Data;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var authConfig = builder.Configuration.GetSection("Auth");
var jwtAuthority = authConfig["JwtAuthority"];
var browserAuthority = authConfig["SwaggerAuthority"];

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MonitoringDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.AccessDeniedPath = "/Dashboard/AccessDenied";
})
.AddOpenIdConnect(options =>
{
    options.Authority = browserAuthority;
    options.MetadataAddress = $"{jwtAuthority}/.well-known/openid-configuration";
    options.ClientId = "monitoring_web";
    options.ClientSecret = "monitoring_secret";
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.RequireHttpsMetadata = false;
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("account_api");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        RoleClaimType = "role",
        NameClaimType = "name",
        ValidateIssuer = false
    };
    options.ClaimActions.MapUniqueJsonKey(ClaimTypes.Role, "role");
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EmployeeOnly", p => p.RequireRole("Employee"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MonitoringDbContext>();
    db.Database.EnsureCreated();
}

app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax,
    Secure = CookieSecurePolicy.None
});

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowAll");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
