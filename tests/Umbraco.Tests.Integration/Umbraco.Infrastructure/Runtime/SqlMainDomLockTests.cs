// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Runtime;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;
using ICoreScope = Umbraco.Cms.Core.Scoping.ICoreScope;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Runtime;

[TestFixture]
[Timeout(120000)]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class SqlMainDomLockTests : UmbracoIntegrationTest
{
    [Test]
    public async Task Releasing_The_Lock_Removes_Its_Lock_Record()
    {
        SqlMainDomLock mainDomLock = CreateSqlMainDomLock();

        Assert.That(await mainDomLock.AcquireLockAsync(5000), Is.True);
        Assert.That(CountLockRecords(), Is.EqualTo(1));

        mainDomLock.Dispose();

        Assert.That(CountLockRecords(), Is.EqualTo(0));
    }

    [Test]
    public async Task A_Booting_Application_Takes_Over_The_Lock_When_The_Running_One_Releases_It()
    {
        const int timeout = 30000;
        SqlMainDomLock running = CreateSqlMainDomLock(pollingInterval: 200);
        SqlMainDomLock booting = CreateSqlMainDomLock(pollingInterval: 200);
        try
        {
            Assert.That(await running.AcquireLockAsync(5000), Is.True);
            Task listening = running.ListenAsync();

            var stopwatch = Stopwatch.StartNew();
            Task<bool> acquiring = booting.AcquireLockAsync(timeout);

            // The running application notices the booting one, stops listening and, as MainDom does, releases its lock.
            Assert.That(await Task.WhenAny(listening, Task.Delay(timeout)), Is.SameAs(listening), "The running application didn't notice the booting one.");
            running.Dispose();

            Assert.That(await acquiring, Is.True);
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(timeout), "The lock was only taken by force, after the timeout.");
            Assert.That(CountLockRecords(), Is.EqualTo(1));
        }
        finally
        {
            running.Dispose();
            booting.Dispose();
        }

        Assert.That(CountLockRecords(), Is.EqualTo(0));
    }

    private SqlMainDomLock CreateSqlMainDomLock(int pollingInterval = GlobalSettings.StaticMainDomReleaseSignalPollingInterval)
    {
        IUmbracoDatabaseFactory factory = GetRequiredService<IUmbracoDatabaseFactory>();
        var connectionStrings = new TestOptionsMonitor<ConnectionStrings>(
            new ConnectionStrings { ConnectionString = factory.ConnectionString, ProviderName = factory.ProviderName });

        return new SqlMainDomLock(
            LoggerFactory,
            Options.Create(new GlobalSettings { MainDomReleaseSignalPollingInterval = pollingInterval }),
            connectionStrings,
            GetRequiredService<IDbProviderFactoryCreator>(),
            GetRequiredService<IMainDomKeyGenerator>(),
            GetRequiredService<DatabaseSchemaCreatorFactory>(),
            GetRequiredService<NPocoMapperCollection>());
    }

    private int CountLockRecords()
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        IUmbracoDatabase database = ScopeAccessor.AmbientScope!.Database;
        return database.Fetch<KeyValueDto>(database.SqlContext.Sql().SelectAll().From<KeyValueDto>())
            .Count(x => x.Key.StartsWith("Umbraco.Core.Runtime.SqlMainDom", StringComparison.Ordinal));
    }
}
