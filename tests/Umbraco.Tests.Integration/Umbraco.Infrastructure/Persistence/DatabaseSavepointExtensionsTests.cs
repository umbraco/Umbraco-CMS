// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Data.Common;
using NUnit.Framework;
using StackExchange.Profiling.Data;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;
using ICoreScope = Umbraco.Cms.Core.Scoping.ICoreScope;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DatabaseSavepointExtensionsTests : UmbracoIntegrationTest
{
    private const string KeyPrefix = "savepointTest.";

    [Test]
    public void A_Successful_Operation_Is_Kept()
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            IUmbracoDatabase database = ScopeAccessor.AmbientScope!.Database;
            var result = database.ExecuteInSavepoint(() => InsertKey(database, "inside"));
            scope.Complete();

            Assert.That(result, Is.EqualTo(1));
        }

        Assert.That(GetStoredKeys(), Is.EquivalentTo(new[] { "inside" }));
    }

    [Test]
    public void A_Database_Error_Reaches_The_Caller_And_The_Transaction_Carries_On()
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            IUmbracoDatabase database = ScopeAccessor.AmbientScope!.Database;
            InsertKey(database, "before");

            Assert.Catch<DbException>(() => database.ExecuteInSavepoint(() => InsertKey(database, "before")));

            InsertKey(database, "after");
            scope.Complete();
        }

        Assert.That(GetStoredKeys(), Is.EquivalentTo(new[] { "before", "after" }));
    }

    [Test]
    public void A_Failed_Operation_Is_Undone_And_The_Transaction_Carries_On()
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            IUmbracoDatabase database = ScopeAccessor.AmbientScope!.Database;
            AssumeSavepointsAreSupported(database);
            InsertKey(database, "before");

            Assert.Throws<InvalidOperationException>(() => database.ExecuteInSavepoint(() =>
            {
                InsertKey(database, "inside");
                throw new InvalidOperationException();
            }));

            InsertKey(database, "after");
            scope.Complete();
        }

        Assert.That(GetStoredKeys(), Is.EquivalentTo(new[] { "before", "after" }));
    }

    [Test]
    public void A_Database_Error_Undoes_The_Whole_Operation()
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            IUmbracoDatabase database = ScopeAccessor.AmbientScope!.Database;
            AssumeSavepointsAreSupported(database);
            InsertKey(database, "before");

            Assert.Catch<DbException>(() => database.ExecuteInSavepoint(() =>
            {
                InsertKey(database, "inside");
                InsertKey(database, "before");
            }));

            scope.Complete();
        }

        Assert.That(GetStoredKeys(), Is.EquivalentTo(new[] { "before" }));
    }

    [Test]
    public async Task A_Failed_Asynchronous_Operation_Is_Undone_And_The_Transaction_Carries_On()
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            IUmbracoDatabase database = ScopeAccessor.AmbientScope!.Database;
            AssumeSavepointsAreSupported(database);
            InsertKey(database, "before");

            Assert.ThrowsAsync<InvalidOperationException>(() => database.ExecuteInSavepointAsync<int>(async () =>
            {
                await database.InsertAsync(CreateKeyValue("inside"));
                throw new InvalidOperationException();
            }));

            await database.ExecuteInSavepointAsync(() => database.InsertAsync(CreateKeyValue("after")));
            scope.Complete();
        }

        Assert.That(GetStoredKeys(), Is.EquivalentTo(new[] { "before", "after" }));
    }

    [Test]
    public void Savepoints_Can_Be_Nested()
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            IUmbracoDatabase database = ScopeAccessor.AmbientScope!.Database;
            AssumeSavepointsAreSupported(database);
            database.ExecuteInSavepoint(() =>
            {
                InsertKey(database, "outer");
                Assert.Throws<InvalidOperationException>(() => database.ExecuteInSavepoint(() =>
                {
                    InsertKey(database, "inner");
                    throw new InvalidOperationException();
                }));
            });

            scope.Complete();
        }

        Assert.That(GetStoredKeys(), Is.EquivalentTo(new[] { "outer" }));
    }

    // Without savepoint support the operation runs as is, and the database must keep the transaction usable.
    private static void AssumeSavepointsAreSupported(IUmbracoDatabase database)
    {
        DbTransaction? transaction = database.Transaction;
        while (transaction is ProfiledDbTransaction profiled)
        {
            transaction = profiled.WrappedTransaction;
        }

        if (transaction?.SupportsSavepoints is not true)
        {
            Assert.Ignore("The database's transactions do not support savepoints.");
        }
    }

    private static int InsertKey(IUmbracoDatabase database, string key)
    {
        database.Insert(CreateKeyValue(key));
        return 1;
    }

    private static KeyValueDto CreateKeyValue(string key)
        => new() { Key = KeyPrefix + key, Value = key, UpdateDate = DateTime.UtcNow };

    private IEnumerable<string> GetStoredKeys()
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        IUmbracoDatabase database = ScopeAccessor.AmbientScope!.Database;
        return database.Fetch<KeyValueDto>(database.SqlContext.Sql().SelectAll().From<KeyValueDto>())
            .Where(x => x.Key.StartsWith(KeyPrefix, StringComparison.Ordinal))
            .Select(x => x.Key[KeyPrefix.Length..])
            .ToArray();
    }
}
