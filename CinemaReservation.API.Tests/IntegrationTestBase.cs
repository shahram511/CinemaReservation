using Respawn;
using System;
using System.Collections.Generic;
using System.Data.Common;
using Npgsql;
using Microsoft.Extensions.DependencyInjection;
using CinemaReservation.Infrastructure.Data;
using MongoDB.Driver;


namespace CinemaReservation.API.Tests
{
    [Collection("SharedDatabaseCollection")]
    public class IntegrationTestBase : IAsyncLifetime
    {
        protected readonly CustomWebApplicationFactory Factory;
        protected readonly HttpClient Client;

        private readonly SharedDatabaseFixture _fixture;
        private DbConnection _dbConnection = default!;
        private Respawner _respawner = default!;

        public IntegrationTestBase(SharedDatabaseFixture ficture)
        {
            _fixture = ficture;
            Factory = new CustomWebApplicationFactory(
                    _fixture.PostgresContainer.GetConnectionString(),
                    _fixture.MongoContainer.GetConnectionString()
                );
            Client = Factory.CreateClient();

        }
        public async Task InitializeAsync()
        {
            // ========Setup Postgress & Respawn======
            // 1. ensure the database schema is created is created
            using var scope = Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();

            // 2. open a connection for Respawn
            _dbConnection = new NpgsqlConnection(_fixture.PostgresContainer.GetConnectionString());
            await _dbConnection.OpenAsync();

            // 3. initialize Respawn and ignore the EF core Migrations table
            _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions()
            {
                DbAdapter = DbAdapter.Postgres,
                TablesToIgnore = new Respawn.Graph.Table[] {"__EFMigrationsHistory" }
            });

            // 4.  wipe the database clean before the test starts
            await _respawner.ResetAsync(_dbConnection);

            //=======Setup Mongo Reset==========
            var mongoClient = new MongoClient(_fixture.MongoContainer.GetConnectionString());
            var mongoDb = mongoClient.GetDatabase("CinemaCommentsDb");

            var collections = await mongoDb.ListCollectionNamesAsync();
            var collectionNames = await collections.ToListAsync();
            foreach (var collectionName in collectionNames)
            {
                // Drop collections to ensure a clean NoSQL state
                await mongoDb.DropCollectionAsync(collectionName);
            }
        }
        public async Task DisposeAsync()
        {
            await _dbConnection.DisposeAsync();
        }
    }
}
