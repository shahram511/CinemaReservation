using CinemaReservation.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;

namespace CenemaReservation.E2ETests
{
    public class E2ESharedDatabaseFixture : IAsyncLifetime
    {
        public PostgreSqlContainer postgresContainer;
        public MongoDbContainer mongoDbContainer;
        public E2EWebApplicationFactory Factory { get; private set; }

        public E2ESharedDatabaseFixture()
        {
            postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("e2eTest")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

            mongoDbContainer = new MongoDbBuilder()
                .WithImage("mongo:latest")
                .Build();
        }

        public async Task InitializeAsync()
        {
            await Task.WhenAll(
                        postgresContainer.StartAsync(),
                        mongoDbContainer.StartAsync()
                    );
            Factory = new E2EWebApplicationFactory(
            postgresContainer.GetConnectionString(),
            mongoDbContainer.GetConnectionString()
            );
        }

        public async Task DisposeAsync()
        {
            await Task.WhenAll(
               postgresContainer.DisposeAsync().AsTask(),
               mongoDbContainer.DisposeAsync().AsTask()
           );
        }
    }
}
