using Common.Extensions;
using Common.Middlewares;
using Common.Services;
using Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Services.Implementations;
using Services.Interfaces;
using System.Text.Json.Serialization;

namespace Options;
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var authConfig = builder.Configuration.GetSection("Auth");
        var jwtAuthority = authConfig["JwtAuthority"];
        var swaggerAuthority = authConfig["SwaggerAuthority"];
        var audience = authConfig["Audience"];

        builder.Services.AddLogging(logging => logging.AddConsole());
        builder.Services.AddTracing(builder.Configuration);

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = jwtAuthority;
                options.MetadataAddress = $"{jwtAuthority}/.well-known/openid-configuration";
                options.RequireHttpsMetadata = false;
                options.Audience = audience;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        builder.Services.AddAuthorization();
        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<IIdempotencyService, IdempotencyCacheService>();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(config =>
        {
            config.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{swaggerAuthority}/connect/authorize"),
                        TokenUrl = new Uri($"{swaggerAuthority}/connect/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            { audience, "Options API" },
                            { "openid", "OpenID" },
                            { "profile", "Profile" }
                        }
                    }
                }
            });

            config.AddSecurityDefinition("IdempotencyKey", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Idempotency-Key",
                Type = SecuritySchemeType.ApiKey,
                Description = "���������� ���� ��� ��������������� �������"
            });

            config.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "IdempotencyKey" }
                    },
                    new List<string>()
                }
            });

            config.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                    },
                    new[] { audience }
                }
            });
        });

        builder.Services.AddDbContext<OptionsDbContext>(options =>
            options.UseLazyLoadingProxies()
                   .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                              b => b.MigrationsAssembly("OptionsService")));

        builder.Services.AddScoped<IOptionsService, OptionService>();

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
            var context = scope.ServiceProvider.GetRequiredService<OptionsDbContext>();
            if (context.Database.GetPendingMigrations().Any())
                context.Database.Migrate();
        }

        app.UseMiddleware<TracingMiddleware>();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.OAuthClientId("options_service_swagger");
            options.OAuthScopes(new[] { audience, "openid", "profile" });
        });

        app.UseMiddleware<UnstableServiceMiddleware>();
        app.UseMiddleware<IdempotencyMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors("AllowAll");

        app.MapControllers();

        app.Run();
    }
}