// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Persistence;
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

    [Test]
    public void GetAll_After_GetAll_Is_Cached()
    {
        var realCache = CreateAppCaches();

        var provider = ScopeProvider;
        var scopeAccessor = ScopeAccessor;

        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, realCache);

        var database = scopeAccessor.AmbientScope.Database;

        database.EnableSqlCount = false;

        CreateDataType(repository);

        // Clear the isolated cache so the next GetAll hits the database.
        realCache.IsolatedCaches.ClearCache<IDataType>();

        database.EnableSqlCount = true;

        // First GetAll should hit the database and populate the cache.
        var first = repository.GetMany().ToArray();
        Assert.IsNotEmpty(first);
        Assert.Greater(database.SqlCount, 0);

        // Reset counter.
        database.EnableSqlCount = false;
        database.EnableSqlCount = true;

        // Second GetAll should serve data from cache. The DefaultRepositoryCachePolicy still
        // runs a SELECT COUNT(*) validation query (GetAllCacheValidateCount is enabled by default),
        // so we expect exactly 1 SQL query for the count check.
        // This verifies that GUID-keyed cache entries (under a separate prefix) do not
        // pollute the int-keyed repository's prefix search and count validation.
        // Before the fix for #21756, GUID entries inflated the prefix-based count, causing
        // validation to fail and triggering a full re-query from the database (SqlCount >> 1).
        var second = repository.GetMany().ToArray();
        Assert.IsNotEmpty(second);
        Assert.AreEqual(1, database.SqlCount);
    }

    /// <summary>
    /// Verifies that retrieving all data types from the GUID-based repository returns all items when the cache is
    /// populated.
    /// </summary>
    /// <remarks>
    /// Verifies the fix for https://github.com/umbraco/Umbraco-CMS/issues/21756 as this test fails before
    /// the fix is applied.
    /// </remarks>
    [Test]
    public void GetMany_By_Guid_With_Warm_Cache_Returns_All()
    {
        // Saving a data type populates both the int-keyed and GUID-keyed isolated
        // caches (they share the same IAppPolicyCache keyed by typeof(IDataType)).
        // When GetMany is then called via the GUID-based IReadRepository with no
        // keys, it should return all data types regardless of cache state.

        var realCache = CreateAppCaches();
        var provider = ScopeProvider;

        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, realCache);

        // Save populates the cache for both int and GUID lookups.
        var dataType = CreateDataType(repository);

        // Cast to the GUID-based read interface (same path as DataTypeService.GetAllAsync).
        var guidRepo = (IReadRepository<Guid, IDataType>)repository;

        // GetMany should return the saved data type without throwing.
        var result = guidRepo.GetMany().ToArray();
        Assert.IsNotEmpty(result);
        Assert.That(result.Any(dt => dt.Key == dataType.Key));
    }

    [Test]
    public void Retrieval_By_Guid_After_GetMany_By_Guid_Is_Cached()
    {
        var realCache = CreateAppCaches();

        var provider = ScopeProvider;
        var scopeAccessor = ScopeAccessor;

        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, realCache);

        var database = scopeAccessor.AmbientScope.Database;

        database.EnableSqlCount = false;

        var dataType = CreateDataType(repository);

        // Clear the isolated cache so the next retrieval hits the database.
        realCache.IsolatedCaches.ClearCache<IDataType>();

        var guidRepo = (IReadRepository<Guid, IDataType>)repository;

        database.EnableSqlCount = true;

        // GetMany with specific GUIDs should hit the database and populate the GUID cache.
        var result = guidRepo.GetMany(dataType.Key).ToArray();
        Assert.IsNotEmpty(result);
        Assert.Greater(database.SqlCount, 0);

        // Reset counter.
        database.EnableSqlCount = false;
        database.EnableSqlCount = true;

        // Subsequent Get by the same GUID should be served from the GUID cache.
        var cached = guidRepo.Get(dataType.Key);
        Assert.IsNotNull(cached);
        Assert.AreEqual(dataType.Key, cached!.Key);
        Assert.AreEqual(0, database.SqlCount);
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
