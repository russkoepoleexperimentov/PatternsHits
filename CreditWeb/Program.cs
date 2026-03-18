using Common.Contracts;
using Common.Middlewares;
using Common.Options;
using CreditApplication.Consumers;
using CreditApplication.Dtos;
using CreditApplication.Profiles;
using CreditApplication.Services.Interfaces;
using CreditApplication.Validators;
using CreditInfrastructure;
using CreditService.Services;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Web.Options;

namespace Web
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

            builder.Services.AddLogging(logging => logging.AddConsole());

            builder.Services
                .AddControllers()
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
                    { audience, "API" },
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
            builder.Services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumer<ProcessExternalPaymentConsumer>();
                x.AddRequestClient<DepositFundsCommand>();
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

            builder.Services.AddHostedService<InterestAccrualService>();
            builder.Services.AddScoped<IValidator<CreateTariffRequest>, TariffValidator>();
            builder.Services.AddScoped<IValidator<CreateCreditRequest>, CreateCreditRequestValidator>();
            builder.Services.AddScoped<IValidator<ApproveCreditRequest>, ApproveCreditRequestValidator>();
            builder.Services.AddScoped<IValidator<RejectCreditRequest>, RejectCreditRequestValidator>();
            builder.Services.AddScoped<IValidator<CreatePaymentRequest>, CreatePaymentRequestValidator>();
            builder.Services.AddScoped<IValidator<UpdatePaymentStatusRequest>, UpdatePaymentStatusRequestValidator>();
            builder.Services.AddScoped<ITariffService, TariffService>();
            builder.Services.AddScoped<ICreditService, CreditsService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services
                .AddAutoMapper(typeof(CreditProfile))
                .AddAutoMapper(typeof(TariffProfile))
                .AddAutoMapper(typeof(PaymentProfile));

            builder.Services.AddDbContext<CreditDbContext>(options =>
                options.UseLazyLoadingProxies()
                       .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                                  b => b.MigrationsAssembly("CreditWeb")));

            var app = builder.Build();


            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CreditDbContext>();
                if (context.Database.GetPendingMigrations().Any())
                    context.Database.Migrate();
            }

            app.UseMiddleware<ExceptionCatchMiddleware>();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.OAuthClientId("credit_service_swagger");
                options.OAuthScopes(new[] { audience, "openid", "profile" });
            });

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors("AllowFrontend");

            app.MapControllers();

            app.Run();
        }
    }
}