using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Infrastructure.DistributedLocking;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.DistributedLocking;

[TestFixture]
public class DefaultDistributedLockingMechanismFactoryTests
{
    private TestOptionsMonitor<GlobalSettings> SettingsWithNoConfiguredMechanism()
        => new(new GlobalSettings { DistributedLockingMechanism = string.Empty });

    private TestOptionsMonitor<GlobalSettings> SettingsWithConfiguredMechanism(string typeName)
        => new(new GlobalSettings { DistributedLockingMechanism = typeName });

    [Test]
    public void DistributedLockingMechanism_NoMechanismsRegistered_Throws()
    {
        var factory = new DefaultDistributedLockingMechanismFactory(
            SettingsWithNoConfiguredMechanism(),
            []);

        Assert.Throws<InvalidOperationException>(() => _ = factory.DistributedLockingMechanism);
    }

    [Test]
    public void DistributedLockingMechanism_AllMechanismsDisabled_Throws()
    {
        var factory = new DefaultDistributedLockingMechanismFactory(
            SettingsWithNoConfiguredMechanism(),
            [new DisabledMechanism(), new DisabledMechanism()]);

        Assert.Throws<InvalidOperationException>(() => _ = factory.DistributedLockingMechanism);
    }

    [Test]
    public void DistributedLockingMechanism_FirstEnabledIsReturned()
    {
        var first = new EnabledMechanism();

        var factory = new DefaultDistributedLockingMechanismFactory(
            SettingsWithNoConfiguredMechanism(),
            [first, new EnabledMechanism()]);

        Assert.That(factory.DistributedLockingMechanism, Is.SameAs(first));
    }

    [Test]
    public void DistributedLockingMechanism_FirstDisabledSecondEnabled_ReturnsSecond()
    {
        var second = new EnabledMechanism();

        var factory = new DefaultDistributedLockingMechanismFactory(
            SettingsWithNoConfiguredMechanism(),
            [new DisabledMechanism(), second]);

        Assert.That(factory.DistributedLockingMechanism, Is.SameAs(second));
    }

    [Test]
    public void DistributedLockingMechanism_DefaultPath_ReEvaluatesEnabledOnEachAccess()
    {
        // Simulate a mechanism whose Enabled state changes between calls, e.g.
        // SqliteEFCoreDistributedLockingMechanism which checks for an active EF Core scope.
        var callCount = 0;
        var mechanism = new DynamicEnabledMechanism(() =>
        {
            callCount++;
            return callCount == 2; // disabled on first call, enabled on second
        });

        var factory = new DefaultDistributedLockingMechanismFactory(
            SettingsWithNoConfiguredMechanism(),
            [mechanism]);

        // First access — Enabled returns false, factory throws.
        Assert.Throws<InvalidOperationException>(() => _ = factory.DistributedLockingMechanism);

        // Second access — Enabled returns true, factory returns the mechanism.
        Assert.That(factory.DistributedLockingMechanism, Is.SameAs(mechanism));
    }

    [Test]
    public void DistributedLockingMechanism_ConfiguredMechanismFoundByTypeName_ReturnsMechanism()
    {
        var specific = new SpecificMechanism();

        var factory = new DefaultDistributedLockingMechanismFactory(
            SettingsWithConfiguredMechanism(nameof(SpecificMechanism)),
            [new EnabledMechanism(), specific]);

        Assert.That(factory.DistributedLockingMechanism, Is.SameAs(specific));
    }

    [Test]
    public void DistributedLockingMechanism_ConfiguredMechanismNotFound_Throws()
    {
        var factory = new DefaultDistributedLockingMechanismFactory(
            SettingsWithConfiguredMechanism("NonExistentMechanism"),
            [new EnabledMechanism()]);

        Assert.Throws<InvalidOperationException>(() => _ = factory.DistributedLockingMechanism);
    }

    [Test]
    public void DistributedLockingMechanism_ConfiguredMechanism_IsCachedAfterFirstResolution()
    {
        var specific = new SpecificMechanism();

        var factory = new DefaultDistributedLockingMechanismFactory(
            SettingsWithConfiguredMechanism(nameof(SpecificMechanism)),
            [specific]);

        var first = factory.DistributedLockingMechanism;
        var second = factory.DistributedLockingMechanism;

        Assert.That(first, Is.SameAs(second));
    }

    [Test]
    public void DistributedLockingMechanism_ConfiguredMechanism_IgnoresEnabledProperty()
    {
        // When a mechanism is explicitly configured, it is returned regardless of its Enabled state.
        var specific = new DisabledSpecificMechanism();

        var factory = new DefaultDistributedLockingMechanismFactory(
            SettingsWithConfiguredMechanism(nameof(DisabledSpecificMechanism)),
            [specific]);

        Assert.That(factory.DistributedLockingMechanism, Is.SameAs(specific));
    }

    // --- Stub mechanism implementations ---

    private sealed class EnabledMechanism : IDistributedLockingMechanism
    {
        public bool HasActiveRelatedScope => false;
        public bool Enabled => true;
        public IDistributedLock ReadLock(int lockId, TimeSpan? obtainLockTimeout = null) => Mock.Of<IDistributedLock>();
        public IDistributedLock WriteLock(int lockId, TimeSpan? obtainLockTimeout = null) => Mock.Of<IDistributedLock>();
    }

    private sealed class DisabledMechanism : IDistributedLockingMechanism
    {
        public bool HasActiveRelatedScope => false;
        public bool Enabled => false;
        public IDistributedLock ReadLock(int lockId, TimeSpan? obtainLockTimeout = null) => Mock.Of<IDistributedLock>();
        public IDistributedLock WriteLock(int lockId, TimeSpan? obtainLockTimeout = null) => Mock.Of<IDistributedLock>();
    }

    private sealed class SpecificMechanism : IDistributedLockingMechanism
    {
        public bool HasActiveRelatedScope => false;
        public bool Enabled => true;
        public IDistributedLock ReadLock(int lockId, TimeSpan? obtainLockTimeout = null) => Mock.Of<IDistributedLock>();
        public IDistributedLock WriteLock(int lockId, TimeSpan? obtainLockTimeout = null) => Mock.Of<IDistributedLock>();
    }

    private sealed class DisabledSpecificMechanism : IDistributedLockingMechanism
    {
        public bool HasActiveRelatedScope => false;
        public bool Enabled => false;
        public IDistributedLock ReadLock(int lockId, TimeSpan? obtainLockTimeout = null) => Mock.Of<IDistributedLock>();
        public IDistributedLock WriteLock(int lockId, TimeSpan? obtainLockTimeout = null) => Mock.Of<IDistributedLock>();
    }

    private sealed class DynamicEnabledMechanism : IDistributedLockingMechanism
    {
        private readonly Func<bool> _isEnabled;

        public DynamicEnabledMechanism(Func<bool> isEnabled) => _isEnabled = isEnabled;

        public bool HasActiveRelatedScope => false;
        public bool Enabled => _isEnabled();
        public IDistributedLock ReadLock(int lockId, TimeSpan? obtainLockTimeout = null) => Mock.Of<IDistributedLock>();
        public IDistributedLock WriteLock(int lockId, TimeSpan? obtainLockTimeout = null) => Mock.Of<IDistributedLock>();
    }
}
