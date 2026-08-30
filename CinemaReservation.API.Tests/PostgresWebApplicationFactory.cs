using CinemaReservation.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace CinemaReservation.API.Tests
{
    public class PostgresWebApplicationFactory : WebApplicationFactory<Program> , IAsyncLifetime
    {
        // Define the docker container  configuration
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine") // Matches a lightweight version of your production DB
            .WithDatabase("CinemaConcurrencyDb")
            .WithUsername("postgres")
            .WithPassword("testpassword")
            .Build();

        //  Start the container before tests run
        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();
        }

        // Destroy the container after tests finish
        async Task IAsyncLifetime.DisposeAsync()
        {
            await _dbContainer.DisposeAsync();
        }           

        // Swap the Database connection in  the DI container
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // Find and remove the original ApplicationDbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Inject this new context using the dynamic Docker connection string
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseNpgsql(_dbContainer.GetConnectionString());
                });

                //  Fake authentication
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "TestScheme";
                    options.DefaultChallengeScheme = "TestScheme";
                    options.DefaultScheme = "TestScheme";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });                
            });
        }
    }
}
