using Common.Middlewares;
using ConcurrencyService.Services.Interfaces;
using CurrencyService.Data;
using CurrencyService.Jobs;
using CurrencyService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quartz;
using System.Text.Json.Serialization;

namespace CurrencyService;

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
                            { audience, "Currency API" },
                            { "openid", "OpenID" },
                            { "profile", "Profile" }
                        }
                    }
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

        builder.Services.AddDbContext<CurrencyDbContext>(options =>
            options.UseLazyLoadingProxies()
                .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("ConcurrencyService")));

        builder.Services.AddHttpClient<ICurrencyApiService, ExchangeRateApiService>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["CurrencyApi:BaseUrl"]);
        });

        builder.Services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();

            q.AddJob<ExchangeRateUpdateJob>(opts => opts.WithIdentity("ExchangeRateUpdateJob"))
                .AddTrigger(opts => opts
                    .ForJob("ExchangeRateUpdateJob")
                    .WithIdentity("ExchangeRateUpdateTrigger")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(builder.Configuration.GetValue<int>("CurrencyApi:UpdateIntervalMinutes", 96))
                        .RepeatForever()));
        });

        builder.Services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CurrencyDbContext>();
            if (context.Database.GetPendingMigrations().Any())
                context.Database.Migrate();
        }

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.OAuthClientId("currency_service_swagger");
            options.OAuthScopes(new[] { audience, "openid", "profile" });
        });
        app.UseMiddleware<UnstableServiceMiddleware>();
        app.UseMiddleware<ExceptionCatchMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}