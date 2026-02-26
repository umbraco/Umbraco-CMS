// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class KeyValueRepositoryTests : UmbracoIntegrationTest
{
    [Test]
    public async Task CanSetAndGet()
    {
        IEFCoreScopeProvider<UmbracoDbContext> provider = NewScopeProvider;

        // Insert new key/value
        using (var scope = provider.CreateScope())
        {
            var keyValue = new KeyValue { Identifier = "foo", Value = "bar", UpdateDate = DateTime.UtcNow };
            var repo = CreateRepository(provider);
            await repo.SaveAsync(keyValue, CancellationToken.None);
            scope.Complete();
        }

        // Retrieve key/value
        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);
            var keyValue = await repo.GetAsync("foo", CancellationToken.None);
            scope.Complete();

            Assert.AreEqual("bar", keyValue.Value);
        }

        // Update value
        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);
            var keyValue = await repo.GetAsync("foo", CancellationToken.None);
            keyValue.Value = "buzz";
            keyValue.UpdateDate = DateTime.UtcNow;
            await repo.SaveAsync(keyValue, CancellationToken.None);
            scope.Complete();
        }

        // Retrieve key/value again
        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);
            var keyValue = await repo.GetAsync("foo", CancellationToken.None);
            scope.Complete();

            Assert.AreEqual("buzz", keyValue.Value);
        }
    }

    private IKeyValueRepository CreateRepository(IEFCoreScopeProvider<UmbracoDbContext> provider) =>
        new KeyValueRepository(GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>(), LoggerFactory.CreateLogger<KeyValueRepository>(),  Mock.Of<IRepositoryCacheVersionService>(), Mock.Of<ICacheSyncService>());
}
