using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Infrastructure.DistributedLocking;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Locking;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.DistributedLocking;

[TestFixture]
public class SqliteEFCoreDistributedLockingMechanismTests
{
    private static readonly TestOptionsMonitor<GlobalSettings> DefaultGlobalSettings =
        new(new GlobalSettings());

    private static TestOptionsMonitor<ConnectionStrings> SqliteConnectionStrings() =>
        new(new ConnectionStrings
        {
            ConnectionString = "Data Source=umbraco.db",
            ProviderName = Constants.ProviderNames.SQLite,
        });

    private static TestOptionsMonitor<ConnectionStrings> NotConfiguredConnectionStrings() =>
        new(new ConnectionStrings());

    private static TestOptionsMonitor<ConnectionStrings> SqlServerConnectionStrings() =>
        new(new ConnectionStrings
        {
            ConnectionString = "Server=localhost;Database=umbraco",
            ProviderName = Constants.ProviderNames.SQLServer,
        });

    private static SqliteEFCoreDistributedLockingMechanism<UmbracoDbContext> CreateMechanism(
        TestOptionsMonitor<ConnectionStrings> connectionStrings,
        bool hasBridgedAmbientScope)
    {
        var accessor = new Mock<IEFCoreScopeAccessor<UmbracoDbContext>>();
        accessor.Setup(x => x.HasBridgedAmbientScope).Returns(hasBridgedAmbientScope);

        return new SqliteEFCoreDistributedLockingMechanism<UmbracoDbContext>(
            NullLogger<SqliteEFCoreDistributedLockingMechanism<UmbracoDbContext>>.Instance,
            new Lazy<IEFCoreScopeAccessor<UmbracoDbContext>>(() => accessor.Object),
            DefaultGlobalSettings,
            connectionStrings);
    }

    [Test]
    public void Enabled_ConnectionStringNotConfigured_ReturnsFalse()
    {
        var mechanism = CreateMechanism(NotConfiguredConnectionStrings(), hasBridgedAmbientScope: false);

        Assert.That(mechanism.Enabled, Is.False);
    }

    [Test]
    public void Enabled_SqlServerProvider_ReturnsFalse()
    {
        var mechanism = CreateMechanism(SqlServerConnectionStrings(), hasBridgedAmbientScope: false);

        Assert.That(mechanism.Enabled, Is.False);
    }

    [Test]
    public void Enabled_SqliteConfigured_NoRealEFCoreScope_ReturnsFalse()
    {
        // HasBridgedAmbientScope = true means there is no genuine EF Core scope —
        // either the stack is empty or the scope on it is a bridge scope.
        // In this state the EF Core mechanism must not be selected.
        var mechanism = CreateMechanism(SqliteConnectionStrings(), hasBridgedAmbientScope: true);

        Assert.That(mechanism.Enabled, Is.False);
    }

    [Test]
    public void Enabled_SqliteConfigured_RealEFCoreScopeActive_ReturnsTrue()
    {
        // HasBridgedAmbientScope = false means a genuine, explicitly opened EF Core scope exists.
        // The EF Core mechanism should be selected.
        var mechanism = CreateMechanism(SqliteConnectionStrings(), hasBridgedAmbientScope: false);

        Assert.That(mechanism.Enabled, Is.True);
    }

    [Test]
    public void Enabled_ReflectsCurrentScopeState_OnEachAccess()
    {
        // Enabled must be re-evaluated on every access because scope state changes at runtime.
        // This test simulates a scope becoming active between two accesses.
        var hasBridgedScope = true;
        var accessor = new Mock<IEFCoreScopeAccessor<UmbracoDbContext>>();
        accessor.Setup(x => x.HasBridgedAmbientScope).Returns(() => hasBridgedScope);

        var mechanism = new SqliteEFCoreDistributedLockingMechanism<UmbracoDbContext>(
            NullLogger<SqliteEFCoreDistributedLockingMechanism<UmbracoDbContext>>.Instance,
            new Lazy<IEFCoreScopeAccessor<UmbracoDbContext>>(() => accessor.Object),
            DefaultGlobalSettings,
            SqliteConnectionStrings());

        Assert.That(mechanism.Enabled, Is.False, "Before EF Core scope is opened");

        hasBridgedScope = false; // simulate a real EF Core scope being opened

        Assert.That(mechanism.Enabled, Is.True, "After EF Core scope is opened");
    }

    /// <summary>
    /// Integration scenario: verifies the factory selects the NPoco SQLite mechanism when only
    /// an NPoco scope is active (no real EF Core scope), even though the EF Core SQLite mechanism
    /// is registered first.
    /// </summary>
    [Test]
    public void Factory_WhenNoRealEFCoreScope_SelectsNpocoMechanismOverEFCoreMechanism()
    {
        // EF Core mechanism: SQLite configured, but no real EF Core scope (bridge/NPoco only).
        var efCoreMechanism = CreateMechanism(SqliteConnectionStrings(), hasBridgedAmbientScope: true);

        // NPoco mechanism: always enabled when SQLite is configured.
        var npocoMechanism = new AlwaysEnabledMechanism();

        // EF Core mechanism is registered first (as it is in EFCoreSqliteComposer).
        var factory = new DefaultDistributedLockingMechanismFactory(
            new TestOptionsMonitor<GlobalSettings>(new GlobalSettings()),
            [efCoreMechanism, npocoMechanism]);

        Assert.That(factory.DistributedLockingMechanism, Is.SameAs(npocoMechanism));
    }

    /// <summary>
    /// Integration scenario: verifies the factory selects the EF Core SQLite mechanism when a
    /// real EF Core scope is active.
    /// </summary>
    [Test]
    public void Factory_WhenRealEFCoreScopeActive_SelectsEFCoreMechanismOverNpoco()
    {
        // EF Core mechanism: SQLite configured with a genuine EF Core scope active.
        var efCoreMechanism = CreateMechanism(SqliteConnectionStrings(), hasBridgedAmbientScope: false);

        var npocoMechanism = new AlwaysEnabledMechanism();

        var factory = new DefaultDistributedLockingMechanismFactory(
            new TestOptionsMonitor<GlobalSettings>(new GlobalSettings()),
            [efCoreMechanism, npocoMechanism]);

        Assert.That(factory.DistributedLockingMechanism, Is.SameAs(efCoreMechanism));
    }

    private sealed class AlwaysEnabledMechanism : IDistributedLockingMechanism
    {
        public bool HasActiveRelatedScope => false;
        public bool Enabled => true;
        public IDistributedLock ReadLock(int lockId, TimeSpan? obtainLockTimeout = null) => Mock.Of<IDistributedLock>();
        public IDistributedLock WriteLock(int lockId, TimeSpan? obtainLockTimeout = null) => Mock.Of<IDistributedLock>();
    }
}
