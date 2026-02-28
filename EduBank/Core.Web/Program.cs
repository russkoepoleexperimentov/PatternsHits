
using System.Text.Json.Serialization;
using System.Text;
using Common.Options;
using Core.Application.Mapping;
using Core.Application.Services.Implementations;
using Core.Application.Services.Interfaces;
using Core.Application.Validity;
using Core.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Core.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.


            var jwtOptions = builder.Configuration
                .GetSection("Jwt")
                .Get<JwtOptions>()!;

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


            builder.Services
                .AddScoped<IAccountService, AccountService>()
                .AddAutoMapper(typeof(CoreMapProfile));
            //services.AddScoped<ITransactionService, TransactionService>();

            builder.Services.AddDbContext<CoreDbContext>(options =>
                options
                    .UseLazyLoadingProxies()
                    .UseNpgsql(
                        builder.Configuration.GetConnectionString("DefaultConnection"),
                        b => b.MigrationsAssembly("Core.Web")
                    ));


            var app = builder.Build();



            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CoreDbContext>();

                if (context.Database.GetPendingMigrations().Any())
                    context.Database.Migrate();
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
