using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;

namespace CinemaReservation.API.Tests
{
    public class SharedDatabaseFixture : IAsyncLifetime
    {
        public PostgreSqlContainer PostgresContainer { get;}
        public MongoDbContainer MongoContainer { get;}

        public SharedDatabaseFixture()
        {
            PostgresContainer= new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine") 
            .WithDatabase("CinemaTestDb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

            MongoContainer = new MongoDbBuilder()
                .WithImage("mongo:latest")
                .Build();
        }

        public async Task InitializeAsync()
        {
            await Task.WhenAll(
                        PostgresContainer.StartAsync(),
                        MongoContainer.StartAsync()
                    );
        }

        public async Task DisposeAsync()
        {
            await Task.WhenAll(
               PostgresContainer.DisposeAsync().AsTask(),
               MongoContainer.DisposeAsync().AsTask()
           );
        }       
    }
}
