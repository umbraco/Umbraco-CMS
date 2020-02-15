using NUnit.Framework;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Infrastructure.Persistence.Repositories;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class KeyValueRepositoryTests : TestWithDatabaseBase
    {
        [Test]
        public void CanSetAndGet()
        {
            var provider = TestObjects.GetScopeProvider(Logger);

            // Insert new key/value
            using (var scope = provider.CreateScope())
            {
                var repo = CreateRepository(provider);
                repo.SetValue("foo", "bar");
                scope.Complete();
            }

            // Retrieve key/value
            using (var scope = provider.CreateScope())
            {
                var repo = CreateRepository(provider);
                var value = repo.GetValue("foo");
                scope.Complete();

                Assert.AreEqual("bar", value);
            }

            // Update new key/value
            using (var scope = provider.CreateScope())
            {
                var repo = CreateRepository(provider);
                repo.SetValue("foo", "buzz");
                scope.Complete();
            }

            // Retrieve key/value again
            using (var scope = provider.CreateScope())
            {
                var repo = CreateRepository(provider);
                var value = repo.GetValue("foo");
                scope.Complete();

                Assert.AreEqual("buzz", value);
            }
        }

        private IKeyValueRepository CreateRepository(IScopeProvider provider)
        {
            return new KeyValueRepository((IScopeAccessor) provider, Logger);
        }        
    }
}
