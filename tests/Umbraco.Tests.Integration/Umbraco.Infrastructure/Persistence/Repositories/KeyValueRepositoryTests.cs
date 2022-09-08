// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class KeyValueRepositoryTests : UmbracoIntegrationTest
{
    [Test]
    public void CanSetAndGet()
    {
        ICoreScopeProvider provider = ScopeProvider;

        // Insert new key/value
        using (var scope = provider.CreateCoreScope())
        {
            var keyValue = new KeyValue { Identifier = "foo", Value = "bar", UpdateDate = DateTime.Now };
            var repo = CreateRepository(provider);
            repo.Save(keyValue);
            scope.Complete();
        }

        // Retrieve key/value
        using (var scope = provider.CreateCoreScope())
        {
            var repo = CreateRepository(provider);
            var keyValue = repo.Get("foo");
            scope.Complete();

            Assert.AreEqual("bar", keyValue.Value);
        }

        // Update value
        using (var scope = provider.CreateCoreScope())
        {
            var repo = CreateRepository(provider);
            var keyValue = repo.Get("foo");
            keyValue.Value = "buzz";
            keyValue.UpdateDate = DateTime.Now;
            repo.Save(keyValue);
            scope.Complete();
        }

        // Retrieve key/value again
        using (var scope = provider.CreateCoreScope())
        {
            var repo = CreateRepository(provider);
            var keyValue = repo.Get("foo");
            scope.Complete();

            Assert.AreEqual("buzz", keyValue.Value);
        }
    }

    private IKeyValueRepository CreateRepository(ICoreScopeProvider provider) =>
        new KeyValueRepository((IScopeAccessor)provider, LoggerFactory.CreateLogger<KeyValueRepository>());
}
