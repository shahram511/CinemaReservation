using CinemaReservation.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace CenemaReservation.E2ETests
{
    public class E2EWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _postgressConnection;
        private readonly string _mongoConnection;

        public E2EWebApplicationFactory(string postgressConnection, string mongoConnection)
        {
            _postgressConnection = postgressConnection;
            _mongoConnection = mongoConnection;
        }


        // Swap the Database connection in  the DI container
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {    
            // override the  configuration
            builder.ConfigureAppConfiguration((context,config) =>
            {
                var testConfig = new Dictionary<string, string>()
                {                    
                    { "MongoDbSettings:ConnectionString", _mongoConnection }
                };
                config.AddInMemoryCollection(testConfig);
            });

            // override the EF core
            builder.ConfigureTestServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseNpgsql(_postgressConnection);
                });
            });
        }
    }
}

