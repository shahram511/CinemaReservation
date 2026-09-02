using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.API.Tests
{
    [CollectionDefinition("SharedDatabaseCollection")]
    public class SharedDatabaseCollection : ICollectionFixture<SharedDatabaseFixture>
    {
    }
}
