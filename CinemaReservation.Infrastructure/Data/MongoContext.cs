using CinemaReservation.Core.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Infrastructure.Data
{
    public class MongoContext
    {
        private readonly IMongoDatabase _database;

        public MongoContext(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
            MapClasses();
        }
        public IMongoCollection<MovieComment> MovieComments =>
            _database.GetCollection<MovieComment>("MovieComments");

        private static void MapClasses()
        {
            if(!BsonClassMap.IsClassMapRegistered(typeof(MovieComment)))
            {
                BsonClassMap.RegisterClassMap<MovieComment>(cm =>
                {
                    cm.AutoMap();

                    // 1. Map the Id as an ObjectId string
                    cm.MapIdProperty(c => c.Id)
                      .SetIdGenerator(StringObjectIdGenerator.Instance)
                      .SetSerializer(new StringSerializer(BsonType.ObjectId));

                    // 2. Map the GUIDs using the standard representation
                    cm.MapProperty(c => c.MovieId)
                      .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

                    cm.MapProperty(c => c.UserId)
                      .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                });
            }
        }
    }
}
