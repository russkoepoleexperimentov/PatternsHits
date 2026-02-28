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
using MassTransit.JobService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Web.Options;

namespace Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<JwtOptions>(
                builder.Configuration.GetSection("Jwt"));

            builder.Services.Configure<RabbitMqOptions>(
                builder.Configuration.GetSection("RabbitMq"));

            var jwtOptions = builder.Configuration
                .GetSection("Jwt")
                .Get<JwtOptions>()!;

            var rabbitOptions = builder.Configuration
                .GetSection("RabbitMq")
                .Get<RabbitMqOptions>()!;



            builder.Services.AddLogging(logging =>
                logging.AddConsole());


            builder.Services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions
                        .Converters
                        .Add(new JsonStringEnumConverter());
                });


            var accessKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Access.Secret));

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            IssuerSigningKey = accessKey,
                        };
                });

            builder.Services.AddAuthorization();


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(config =>
            {
                config.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                config.OperationFilter<SwaggerAuthorizeFilter>();
            });


            builder.Services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();

                x.AddConsumer<ProcessExternalPaymentConsumer>();
                x.AddRequestClient<DepositFundsCommand>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitOptions.Host,
                             rabbitOptions.VirtualHost,
                             h =>
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
                options
                    .UseLazyLoadingProxies()
                    .UseNpgsql(
                        builder.Configuration.GetConnectionString("DefaultConnection"),
                        b => b.MigrationsAssembly("CreditWeb")
                    ));


            var app = builder.Build();



            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CreditDbContext>();

                if (context.Database.GetPendingMigrations().Any())
                    context.Database.Migrate();
            }

            app.UseMiddleware<ExceptionCatchMiddleware>();
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();


        }
    }
}