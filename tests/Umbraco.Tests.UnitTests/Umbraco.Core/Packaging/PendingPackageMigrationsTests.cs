using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Packaging;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Packaging;

[TestFixture]
public class PendingPackageMigrationsTests
{
    private static readonly Guid s_step1 = Guid.NewGuid();
    private static readonly Guid s_step2 = Guid.NewGuid();
    private const string TestPackageName = "Test1";

    private class TestPackageMigrationPlan : PackageMigrationPlan
    {
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

    [Test]
    public void GivenNoRegisteredMigrations_ThenPlanIsReturned()
    {
        var pendingPackageMigrations = GetPendingPackageMigrations();
        var registeredMigrations = new Dictionary<string, string>();
        var pending = pendingPackageMigrations.GetPendingPackageMigrations(registeredMigrations);
        Assert.AreEqual(1, pending.Count);
    }

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
