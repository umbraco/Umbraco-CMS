using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Packaging;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Packaging;

/// <summary>
/// Contains unit tests for the <see cref="PendingPackageMigrations"/> class, verifying its migration logic and behavior.
/// </summary>
[TestFixture]
public class PendingPackageMigrationsTests
{
    private static readonly Guid s_step1 = Guid.NewGuid();
    private static readonly Guid s_step2 = Guid.NewGuid();
    private const string TestPackageName = "Test1";

    private class TestPackageMigrationPlan : PackageMigrationPlan
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="TestPackageMigrationPlan"/> class.
    /// </summary>
        public TestPackageMigrationPlan()
            : base(TestPackageName)
        {
        }

        protected override void DefinePlan()
        {
            To(s_step1);
            To(s_step2);
        }
    }

    private PendingPackageMigrations GetPendingPackageMigrations()
        => new(
            Mock.Of<ILogger<PendingPackageMigrations>>(),
            new PackageMigrationPlanCollection(() => new[] { new TestPackageMigrationPlan() }));

    /// <summary>
    /// Tests that when there are no registered migrations, a migration plan is still returned.
    /// </summary>
    [Test]
    public void GivenNoRegisteredMigrations_ThenPlanIsReturned()
    {
        var pendingPackageMigrations = GetPendingPackageMigrations();
        var registeredMigrations = new Dictionary<string, string>();
        var pending = pendingPackageMigrations.GetPendingPackageMigrations(registeredMigrations);
        Assert.AreEqual(1, pending.Count);
    }

    /// <summary>
    /// Tests that when a registered migration matches the final step, no pending migrations are returned.
    /// </summary>
    [Test]
    public void GivenRegisteredMigration_WhenFinalStepMatched_ThenNoneAreReturned()
    {
        var pendingPackageMigrations = GetPendingPackageMigrations();
        var registeredMigrations = new Dictionary<string, string>
        {
            [Constants.Conventions.Migrations.KeyValuePrefix + TestPackageName] = s_step2.ToString(),
        };
        var pending = pendingPackageMigrations.GetPendingPackageMigrations(registeredMigrations);
        Assert.AreEqual(0, pending.Count);
    }

    /// <summary>
    /// Tests that when a registered migration matches a non-final step, the migration plan is correctly returned.
    /// </summary>
    [Test]
    public void GivenRegisteredMigration_WhenNonFinalStepMatched_ThenPlanIsReturned()
    {
        var pendingPackageMigrations = GetPendingPackageMigrations();
        var registeredMigrations = new Dictionary<string, string>
        {
            [Constants.Conventions.Migrations.KeyValuePrefix + TestPackageName] = s_step1.ToString(),
        };
        var pending = pendingPackageMigrations.GetPendingPackageMigrations(registeredMigrations);
        Assert.AreEqual(1, pending.Count);
    }

    /// <summary>
    /// Tests that when a migration step is registered with a different case,
    /// the pending package migrations plan is still returned correctly.
    /// </summary>
    [Test]
    public void GivenRegisteredMigration_WhenStepIsDifferentCase_ThenPlanIsReturned()
    {
        var pendingPackageMigrations = GetPendingPackageMigrations();
        var registeredMigrations = new Dictionary<string, string>
        {
            [Constants.Conventions.Migrations.KeyValuePrefix + TestPackageName] = s_step1.ToString().ToUpper(),
        };
        var pending = pendingPackageMigrations.GetPendingPackageMigrations(registeredMigrations);
        Assert.AreEqual(1, pending.Count);
    }
}
