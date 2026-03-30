using Common.Contracts;
using Common.Middlewares;
using Common.Options;
using Common.Services;
using Core.Application.Consumers;
using Core.Application.Dtos;
using Core.Application.Mapping;
using Core.Application.Services.Implementations;
using Core.Application.Services.Interfaces;
using Core.Application.Validity;
using Core.Domain;
using Core.Infrastructure;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Web.Options;

namespace Core.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<RabbitMqOptions>(
                builder.Configuration.GetSection("RabbitMq"));

            var rabbitOptions = builder.Configuration
                .GetSection("RabbitMq")
                .Get<RabbitMqOptions>()!;

            var authConfig = builder.Configuration.GetSection("Auth");
            var jwtAuthority = authConfig["JwtAuthority"]; 
            var swaggerAuthority = authConfig["SwaggerAuthority"]; 
            var audience = authConfig["Audience"];

            builder.Services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            var tokenValidation = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero
            }; 

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = jwtAuthority;
                    options.MetadataAddress = $"{jwtAuthority}/.well-known/openid-configuration";
                    options.RequireHttpsMetadata = false;
                    options.Audience = audience;

                    options.TokenValidationParameters = tokenValidation;

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine($"JWT failed: {context.Exception.Message}");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Console.WriteLine("JWT validated");
                            return Task.CompletedTask;
                        }
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
                    { audience, "API" },
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
                    Description = "Уникальный ключ для идемпотентности запроса"
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

            builder.Services.AddSingleton<TransactionsWebSocketConnectionManager>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    policy =>
                    {
                        policy.SetIsOriginAllowed(origin => true)
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    });
            });
            builder.Services.AddScoped<IIdempotencyService, IdempotencyCacheService>();
            builder.Services
                .AddTransient<IAccountService, AccountService>()
                .AddTransient<ITransactionService, TransactionService>()
                .AddScoped<IValidator<CreateTransactionDto>, CreateTransactionValidator>()
                .AddAutoMapper(typeof(CoreMapProfile));

            builder.Services.AddDbContext<CoreDbContext>(options =>
                options.UseLazyLoadingProxies()
                       .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                                  b => b.MigrationsAssembly("Core.Web")));

            builder.Services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumer<DepositFundsConsumer>();
                x.AddConsumer<UserBlockConsumer>();
                x.AddConsumer<UserUnblockConsumer>();
                x.AddRequestClient<ProcessExternalPaymentCommand>();
                x.AddConsumer<ProcessTransactionConsumer>();
                x.AddRequestClient<ProcessTransactionCommand>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitOptions.Host, rabbitOptions.VirtualHost, h =>
                    {
                        h.Username(rabbitOptions.Username);
                        h.Password(rabbitOptions.Password);
                    });
                    cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                    cfg.ConfigureEndpoints(context);
                });
            });

            builder.Services.AddHttpClient<ICurrencyRateService, CurrencyRateService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["CurrencyService:BaseUrl"]);
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
                if (context.Database.GetPendingMigrations().Any())
                    context.Database.Migrate();
            }

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
                if (!context.Accounts.Any(a => a.IsMaster))
                {
                    context.Accounts.Add(new Account
                    {
                        Id = Guid.NewGuid(),
                        UserId = Guid.Empty,
                        Balance = 148800000,
                        IsMaster = true,
                        CreateDateTime = DateTime.UtcNow
                    });
                }

                await context.SaveChangesAsync();
            }

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.OAuthClientId("account_service_swagger"); 
                options.OAuthScopes(new[] { audience, "openid", "profile" });
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.UseCors("AllowFrontend");
            app.UseMiddleware<UnstableServiceMiddleware>();
            app.UseMiddleware<IdempotencyMiddleware>();
            app.UseWebSockets(); // Включаем поддержку WebSocket

            // Подключаем наш middleware
            app.UseMiddleware<TransactionsWebSocketMiddleware>(tokenValidation, jwtAuthority, audience);

            app.Run();
        }
    }
}