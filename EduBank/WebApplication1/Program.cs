using Application.Dtos;
using Application.Profiles;
using Application.Services.Abstractions;
using Application.Services.Implementations;
using Application.Services.Interfaces;
using Application.Validators;
using Common.Enums.Common.Enums;
using Common.Options;
using Domain.Entities;
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

            builder.Services.Configure<IdentityPasswordOptions>(
                builder.Configuration.GetSection("Identity:Password"));

            var jwtOptions = builder.Configuration
                .GetSection("Jwt")
                .Get<JwtOptions>()!;

            var rabbitOptions = builder.Configuration
                .GetSection("RabbitMq")
                .Get<RabbitMqOptions>()!;

            var identityOptions = builder.Configuration
                .GetSection("Identity:Password")
                .Get<IdentityPasswordOptions>()!;


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

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitOptions.Host,
                             rabbitOptions.VirtualHost,
                             h =>
                             {
                                 h.Username(rabbitOptions.Username);
                                 h.Password(rabbitOptions.Password);
                             });

                    cfg.ReceiveEndpoint();

                    cfg.ConfigureEndpoints(context);
                });
            });


            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
                {
                    options.Password.RequiredLength = identityOptions.RequiredLength;
                    options.Password.RequireDigit = identityOptions.RequireDigit;
                    options.Password.RequireUppercase = identityOptions.RequireUppercase;
                    options.Password.RequireLowercase = identityOptions.RequireLowercase;
                    options.Password.RequireNonAlphanumeric = identityOptions.RequireNonAlphanumeric;
                })
                .AddEntityFrameworkStores<UserDbContext>()
                .AddDefaultTokenProviders();


            builder.Services
                .AddScoped<IUserService, UserService>()
                .AddScoped<IAuthService, AuthService>()
                .AddScoped<IValidator<UserRegisterDto>, UserRegistrationValidator>()
                .AddScoped<IValidator<UserUpdateDto>, UserUpdateValidator>()
                .AddScoped<IValidator<UserLoginDto>, UserLoginValidator>()
                .AddScoped<IValidator<UserChangePassword>, ChangePasswordValidator>()
                .AddAutoMapper(typeof(UserMapProfile));

            builder.Services.AddSingleton<IJwtService>(sp =>
            {
                var jwtOptions = sp.GetRequiredService<IOptions<JwtOptions>>().Value;

                return new JWTService(
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.Access.Secret)),
                    jwtOptions.Access.LifetimeMinutes,
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.Refresh.Secret)),
                    jwtOptions.Refresh.LifetimeDays
                );
            });


            builder.Services.AddDbContext<UserDbContext>(options =>
                options
                    .UseLazyLoadingProxies()
                    .UseNpgsql(
                        builder.Configuration.GetConnectionString("DefaultConnection"),
                        b => b.MigrationsAssembly("AuthWeb")
                    ));


            var app = builder.Build();



            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();

                if (context.Database.GetPendingMigrations().Any())
                    context.Database.Migrate();
            }

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider
                    .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

                string[] roles =
                {
                    RoleNames.Customer,
                    RoleNames.Employee,
                    RoleNames.Admin
                };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                         await roleManager.CreateAsync(
                            new IdentityRole<Guid>(role));
                    }
                }
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();


        }
    }
}