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

            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Id, Is.EqualTo(query.Id));
            Assert.That(found.Name, Is.EqualTo("Test Query"));
            Assert.That(found.Query, Is.EqualTo("@Level='Error'"));
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

            Assert.That(found, Is.Null);
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

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
            Assert.That(second!.Id, Is.EqualTo(first!.Id));
            Assert.That(second, Is.Not.SameAs(first));
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
            Assert.That(first, Is.Not.Null);
            first!.Name = "MUTATED_" + Guid.NewGuid();

            var second = repository.GetByName("Mutation Test");
            Assert.That(second, Is.Not.Null);
            Assert.That(second!.Name, Is.EqualTo("Mutation Test"), "Mutation of a returned entity should not affect the cached copy");
        }
    }
}
