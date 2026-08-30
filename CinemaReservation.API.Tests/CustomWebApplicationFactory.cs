using CinemaReservation.Core.Interfaces;
using CinemaReservation.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.API.Tests
{
    public class CustomWebApplicationFactory  : WebApplicationFactory<Program>
    {     
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // Postgres, (NPSqL), and DbContext configuraiotn (replace with in memory)------
                var descriptorsToRemove = services.Where(d =>
                (d.ServiceType.FullName?.Contains("ApplicationDbContext") ?? false) ||
                (d.ServiceType.Name.Contains("DbContextOptions")) ||
                (d.ServiceType.FullName?.Contains("Npgsql") ?? false) ||
                (d.ImplementationType?.FullName?.Contains("Npgsql") ?? false)
            ).ToList();

                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }                       
                
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InmemoryDbForTesting");
                });

                // MongoDb configuration (remove context $ Mock repo)
                // find and remove the mongocontext singlton registration
                var mongoContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(MongoContext));

                if (mongoContextDescriptor != null)
                    services.Remove(mongoContextDescriptor);

                // find and remove the actual moviecommentrepository registraion
                var commentRepoDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IMovieCommentRepository));

                if (commentRepoDescriptor != null)
                    services.Remove(commentRepoDescriptor); 

                var mockMongoRepo =  new Mock<IMovieCommentRepository>();
                services.AddSingleton(mockMongoRepo);
                services.AddScoped<IMovieCommentRepository>(_ =>mockMongoRepo.Object);

                // fake authentication
                services.AddAuthentication(defaultScheme: "TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });

                services.Configure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = "TestScheme";
                    options.DefaultChallengeScheme = "TestScheme";
                    options.DefaultScheme = "TestShceme";
                });
            });
        }
    }
}
