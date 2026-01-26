// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DataTypeRepositoryTest : UmbracoIntegrationTest
{
    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer =>
        GetRequiredService<IConfigurationEditorJsonSerializer>();

    private IDataValueEditorFactory DataValueEditorFactory =>
        GetRequiredService<IDataValueEditorFactory>();

    [Test]
    public void Retrieval_By_Id_After_Retrieval_By_Id_Is_Cached()
    {
        var realCache = CreateAppCaches();

        var provider = ScopeProvider;
        var scopeAccessor = ScopeAccessor;

        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, realCache);

        var database = scopeAccessor.AmbientScope.Database;

        database.EnableSqlCount = false;

        var dataType = CreateDataType(repository);

        database.EnableSqlCount = true;

        // Clear the isolated cache for IDataType so the next retrieval hits the database
        realCache.IsolatedCaches.ClearCache<IDataType>();

        // Initial request by Id should hit the database.
        repository.Get(dataType.Id);
        Assert.Greater(database.SqlCount, 0);

        // Reset counter.
        database.EnableSqlCount = false;
        database.EnableSqlCount = true;

        // Subsequent requests should use the cache.
        repository.Get(dataType.Id);
        Assert.AreEqual(0, database.SqlCount);
    }

    [Test]
    public void Retrieval_By_Key_After_Retrieval_By_Key_Is_Cached()
    {
        var realCache = CreateAppCaches();

        var provider = ScopeProvider;
        var scopeAccessor = ScopeAccessor;

        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, realCache);

        var database = scopeAccessor.AmbientScope.Database;

        database.EnableSqlCount = false;

        var dataType = CreateDataType(repository);

        database.EnableSqlCount = true;

        // Clear the isolated cache for IDataType so the next retrieval hits the database
        realCache.IsolatedCaches.ClearCache<IDataType>();

        // Initial request by key should hit the database.
        repository.Get(dataType.Key);
        Assert.Greater(database.SqlCount, 0);

        // Reset counter.
        database.EnableSqlCount = false;
        database.EnableSqlCount = true;

        // Subsequent requests should use the cache.
        repository.Get(dataType.Key);
        Assert.AreEqual(0, database.SqlCount);
    }

    [Test]
    public void Retrievals_By_Id_And_Key_After_Save_Are_Cached()
    {
        var realCache = CreateAppCaches();

        var provider = ScopeProvider;
        var scopeAccessor = ScopeAccessor;

        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, realCache);

        var database = scopeAccessor.AmbientScope.Database;

        database.EnableSqlCount = false;

        var dataType = CreateDataType(repository);

        database.EnableSqlCount = true;

        // Initial and subsequent requests should use the cache, since the cache by Id and Key was populated on save.
        repository.Get(dataType.Id);
        Assert.AreEqual(0, database.SqlCount);

        repository.Get(dataType.Id);
        Assert.AreEqual(0, database.SqlCount);

        repository.Get(dataType.Key);
        Assert.AreEqual(0, database.SqlCount);

        repository.Get(dataType.Key);
        Assert.AreEqual(0, database.SqlCount);
    }

    [Test]
    public void Retrieval_By_Key_After_Retrieval_By_Id_Is_Cached()
    {
        var realCache = CreateAppCaches();

        var provider = ScopeProvider;
        var scopeAccessor = ScopeAccessor;

        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, realCache);

        var database = scopeAccessor.AmbientScope.Database;

        database.EnableSqlCount = false;

        var dataType = CreateDataType(repository);

        database.EnableSqlCount = true;

        // Clear the isolated cache for IDataType so the next retrieval hits the database
        realCache.IsolatedCaches.ClearCache<IDataType>();

        // Initial request by ID should hit the database.
        repository.Get(dataType.Id);
        Assert.Greater(database.SqlCount, 0);

        // Reset counter.
        database.EnableSqlCount = false;
        database.EnableSqlCount = true;

        // Subsequent requests should use the cache, since the cache by Id and Key was populated on retrieval.
        repository.Get(dataType.Id);
        Assert.AreEqual(0, database.SqlCount);

        repository.Get(dataType.Key);
        Assert.AreEqual(0, database.SqlCount);
    }

    [Test]
    public void Retrieval_By_Id_After_Retrieval_By_Key_Is_Cached()
    {
        var realCache = CreateAppCaches();

        var provider = ScopeProvider;
        var scopeAccessor = ScopeAccessor;

        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, realCache);

        var database = scopeAccessor.AmbientScope.Database;

        database.EnableSqlCount = false;

        var dataType = CreateDataType(repository);

        database.EnableSqlCount = true;

        // Clear the isolated cache for IDataType so the next retrieval hits the database
        realCache.IsolatedCaches.ClearCache<IDataType>();

        // Initial request by key should hit the database.
        repository.Get(dataType.Key);
        Assert.Greater(database.SqlCount, 0);

        // Reset counter.
        database.EnableSqlCount = false;
        database.EnableSqlCount = true;

        // Subsequent requests should use the cache, since the cache by Id and Key was populated on retrieval.
        repository.Get(dataType.Key);
        Assert.AreEqual(0, database.SqlCount);

        repository.Get(dataType.Id);
        Assert.AreEqual(0, database.SqlCount);
    }

    [Test]
    public void Retrieval_By_Id_After_Deletion_Returns_Null()
    {
        var realCache = CreateAppCaches();

        var provider = ScopeProvider;

        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, realCache);

        var dataType = CreateDataType(repository);
        var retrievedDataType = repository.Get(dataType.Key);
        Assert.IsNotNull(retrievedDataType);

        repository.Delete(dataType);

        retrievedDataType = repository.Get(dataType.Key);
        Assert.IsNull(retrievedDataType);
    }

    private static AppCaches CreateAppCaches() =>
        new(
            new ObjectCacheAppCache(),
            new DictionaryAppCache(),
            new IsolatedCaches(t => new ObjectCacheAppCache()));

    private DataTypeRepository CreateRepository(IScopeAccessor scopeAccessor, AppCaches? appCaches = null)
    {
        appCaches ??= AppCaches;

        var editors = new PropertyEditorCollection(new DataEditorCollection(() => Enumerable.Empty<IDataEditor>()));
        return new DataTypeRepository(
            scopeAccessor,
            appCaches,
            editors,
            LoggerFactory.CreateLogger<DataTypeRepository>(),
            LoggerFactory,
            ConfigurationEditorJsonSerializer,
            Mock.Of<IRepositoryCacheVersionService>(),
            Mock.Of<ICacheSyncService>(),
            DataValueEditorFactory);
    }

    private static DataType CreateDataType(DataTypeRepository repository)
    {
        var dataTypeBuilder = new DataTypeBuilder();
        var dataType = dataTypeBuilder
            .WithId(0)
            .WithName("Test Data Type")
            .AddEditor()
                .WithAlias(Cms.Core.Constants.PropertyEditors.Aliases.TextBox)
                .Done()
            .Build();

        repository.Save(dataType);
        return dataType;
    }
}
