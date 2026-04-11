using Application.Dtos;
using Common.Extensions;
using Application.Profiles;
using Application.Services.Abstractions;
using Application.Services.Implementations;
using Application.Services.Interfaces;
using Application.Validators;
using Common.Contracts.AuthServiceContracts;
using Common.Enums.Common.Enums;
using Common.Middlewares;
using Common.Options;
using Common.Services;
using Domain.Entities;
using Duende.IdentityServer;
using Duende.IdentityServer.Configuration;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using Web.Options;

namespace Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var configuration = builder.Configuration;

        builder.Services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
        var rabbitOptions = configuration.GetSection("RabbitMq").Get<RabbitMqOptions>()!;

        var authConfig = configuration.GetSection("Auth");
        var authority = authConfig["InternalAuthority"] ?? "http://localhost:2280";

        var rsa = RSA.Create(2048);
        var signingKey = new RsaSecurityKey(rsa);
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256);

        builder.Services.AddControllersWithViews()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        builder.Services.AddDbContext<UserDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        builder.Services
            .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<UserDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.Configure<CookiePolicyOptions>(options =>
        {
            options.MinimumSameSitePolicy = SameSiteMode.Lax;
            options.Secure = CookieSecurePolicy.None;
        });

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.None;
        });

        builder.Services.AddIdentityServer(options =>
        {
            options.IssuerUri = authority;
            options.KeyManagement.Enabled = false;
            options.UserInteraction = new UserInteractionOptions
            {
                LoginUrl = "/Account/Login",
                LogoutUrl = "/Account/Logout",
                ErrorUrl = "/home/error"
            };
        })
            .AddAspNetIdentity<ApplicationUser>()
            .AddInMemoryClients(Config.GetClients(configuration))
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryApiResources(Config.ApiResources)
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddProfileService<ProfileService>()
            .AddSigningCredential(signingCredentials); 

        builder.Services.PostConfigure<CookieAuthenticationOptions>(
            IdentityServerConstants.DefaultCookieAuthenticationScheme,
            options =>
            {
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.None;
            });

        builder.Services.AddAuthentication()
            .AddJwtBearer("Bearer", options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateIssuer = true,
                    ValidIssuer = authority,
                    ValidateAudience = true,
                    ValidAudience = "account_api",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Authority = null;
                options.MetadataAddress = null;
               
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("EmployeeOnly", p => p.RequireRole(RoleNames.Employee));
            options.AddPolicy("CustomerOnly", p => p.RequireRole(RoleNames.Customer));
        });

        builder.Services.AddMassTransit(x =>
        {
            x.AddRequestClient<BlockUserAccountsCommand>();
            x.AddRequestClient<UnblockUserAccountsCommand>();
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitOptions.Host, rabbitOptions.VirtualHost, h =>
                {
                    h.Username(rabbitOptions.Username);
                    h.Password(rabbitOptions.Password);
                });
                cfg.ConfigureEndpoints(context);
            });
        });

        builder.Services
            .AddScoped<IUserService, UserService>()
            .AddScoped<IAuthService, AuthService>()
            .AddScoped<IValidator<UserRegisterDto>, UserRegistrationValidator>()
            .AddScoped<IValidator<UserUpdateDto>, UserUpdateValidator>()
            .AddScoped<IValidator<UserLoginDto>, UserLoginValidator>()
            .AddScoped<IValidator<UserChangePassword>, ChangePasswordValidator>()
            .AddAutoMapper(typeof(UserMapProfile));
        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<IIdempotencyService, IdempotencyCacheService>();
        builder.Services.AddTracing(configuration);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{authority}/connect/authorize"),
                        TokenUrl = new Uri($"{authority}/connect/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            { "account_api", "Account API" },
                            { "openid", "OpenId" },
                            { "profile", "Profile" }
                        }
                    }
                }
            });

            options.AddSecurityDefinition("IdempotencyKey", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Idempotency-Key",
                Type = SecuritySchemeType.ApiKey,
                Description = "Уникальный ключ для идемпотентности запроса"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "IdempotencyKey" }
                    },
                    new List<string>()
                }
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                    },
                    new[] { "account_api", "openid", "profile" }
                }
            });
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
            var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            if (context.Database.GetPendingMigrations().Any())
                context.Database.Migrate();
        }

        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            foreach (var role in new[] { RoleNames.Customer, RoleNames.Employee })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        app.UseMiddleware<TracingMiddleware>();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.OAuthClientId(authConfig["SwaggerClientId"] ?? "swagger");
            options.OAuthScopes(new[] { "account_api", "openid", "profile" });
            options.OAuth2RedirectUrl($"{authConfig["SwaggerUrl"]}/swagger/oauth2-redirect.html");
        });

        app.UseStaticFiles();
        app.UseRouting();
        app.UseCookiePolicy();
        app.UseIdentityServer();
        app.UseMiddleware<ExceptionCatchMiddleware>();
        app.UseMiddleware<IdempotencyMiddleware>();
        app.UseMiddleware<UnstableServiceMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors("AllowAll");

        app.MapDefaultControllerRoute();
        app.MapControllers();

        app.Run();
    }
}