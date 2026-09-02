using System;
using System.Collections.Generic;
using System.Text;

namespace CenemaReservation.E2ETests
{
    public class E2ETestBase : IAsyncLifetime
    {
        protected readonly E2EWebApplicationFactory Factory;
        protected readonly HttpClient Client;

        protected E2ETestBase(E2ESharedDatabaseFixture fixture)
        {
            Factory = fixture.Factory;
            Client = Factory.CreateClient();
        }

        public virtual Task InitializeAsync() => Task.CompletedTask;
        public virtual Task DisposeAsync() => Task.CompletedTask;
    }
}
