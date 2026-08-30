using CinemaReservation.Core.DTOs;
using CinemaReservation.Core.Interfaces;
using CinemaReservation.Core.Services;
using CinemaReservation.Core.Validators;
using CinemaReservation.Infrastructure.Data;
using CinemaReservation.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json.Serialization;


namespace CinemaReservation.API.Extension
{
    public static class DependencyInjectionSetup
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Controllers & JSON Options
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

            // 2. OpenAPI

            services.AddOpenApi();

            // 3. Database Context
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("PostgresConnection")));

            services.Configure<MongoDbSettings>(
                configuration.GetSection("MongoDbSettings"));

            services.AddSingleton<MongoContext>();

            // 4. Dependency Injections (Repositories, Services, Validators)
            services.AddScoped<IUserRepository, UserRepositoriy>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IValidator<RegisterUserDto>, RegisterUserDtoValidator>();
            services.AddScoped<IValidator<LoginUserDto>, LoginUserDtoValidator>();
            services.AddScoped<IMovieRepository, MovieRepository>();
            services.AddScoped<IMovieService, MovieService>();
            services.AddScoped<IReservartinoRepository, ReservationRepository>();
            services.AddScoped<IReservartionService, ReservationService>();
            services.AddScoped<IValidator<CreateMovieDto>, CreateMovieDtoValidator>();
            services.AddScoped<IValidator<CreateShowtimeDto>, CreateShowtimeDtoValidator>();
            services.AddScoped<IValidator<CreateReservationDto>, CreateReservationDtoValidator>();
            services.AddScoped<IMovieCommentRepository, MovieCommentRepository>();
            services.AddScoped<IMovieCommentService, MovieCommentService>();
            services.AddScoped<IValidator<CreateCommentDto>, CreateCommentDtoValidator>();
            services.AddScoped<IAnalyticService, AnalyticService>();
            services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

            // 5. JWT Authentication
            var secretKey = configuration["JwtSettings:Secret"] ?? throw new Exception("JWT Key is missing!");
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
            });

            return services;
        }
    }    
}
