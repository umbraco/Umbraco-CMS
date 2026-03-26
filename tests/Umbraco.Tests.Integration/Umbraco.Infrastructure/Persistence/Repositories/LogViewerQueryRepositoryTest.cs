// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class LogViewerQueryRepositoryTest : UmbracoIntegrationTest
{
    private LogViewerQueryRepository CreateRepository(IScopeProvider provider) =>
        new(
            (IScopeAccessor)provider,
            AppCaches.Disabled,
            LoggerFactory.CreateLogger<LogViewerQueryRepository>(),
            Mock.Of<IRepositoryCacheVersionService>(),
            Mock.Of<ICacheSyncService>());

    [Test]
    public void Can_Create_And_Get_By_Name()
    {
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);
            var query = new LogViewerQuery("Test Query", "@Level='Error'");
            repository.Save(query);

            var found = repository.GetByName("Test Query");

            Assert.IsNotNull(found);
            Assert.AreEqual(query.Id, found!.Id);
            Assert.AreEqual("Test Query", found.Name);
            Assert.AreEqual("@Level='Error'", found.Query);
        }
    }

    [Test]
    public void GetByName_Returns_Null_For_NonExisting()
    {
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var found = repository.GetByName("nonexistent");

            Assert.IsNull(found);
        }
    }

    [Test]
    public void GetByName_Returns_Deep_Clone_Not_Cached_Instance()
    {
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);
            var query = new LogViewerQuery("Clone Test", "@Level='Warning'");
            repository.Save(query);

            var first = repository.GetByName("Clone Test");
            var second = repository.GetByName("Clone Test");

            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.AreEqual(first!.Id, second!.Id);
            Assert.AreNotSame(first, second);
        }
    }

    [Test]
    public void GetByName_Mutation_Does_Not_Affect_Subsequent_Get()
    {
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);
            var query = new LogViewerQuery("Mutation Test", "@Level='Fatal'");
            repository.Save(query);

            var first = repository.GetByName("Mutation Test");
            Assert.IsNotNull(first);
            first!.Name = "MUTATED_" + Guid.NewGuid();

            var second = repository.GetByName("Mutation Test");
            Assert.IsNotNull(second);
            Assert.AreEqual("Mutation Test", second!.Name, "Mutation of a returned entity should not affect the cached copy");
        }
    }
}
