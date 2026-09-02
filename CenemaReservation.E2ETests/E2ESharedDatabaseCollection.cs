using System;
using System.Collections.Generic;
using System.Text;

namespace CenemaReservation.E2ETests
{
    [CollectionDefinition("E2ESharedDatabaseCollection")]
    public class E2ESharedDatabaseCollection : ICollectionFixture<E2ESharedDatabaseFixture>
    {
    }
}
