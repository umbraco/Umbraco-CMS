using System.Collections.Generic;
using System.Text.RegularExpressions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations.Upgrade;

[TestFixture]
public class UmbracoPlanTests
{
    [Test]
    public void GetVersionForState_InitialState_Returns_9_4_0()
    {
        UmbracoPlan plan = CreateUmbracoPlan();

        // InitialState is documented as the final migration state of 9.4.
        SemVersion? result = plan.GetVersionForState(plan.InitialState);

        Assert.AreEqual(new SemVersion(9, 4, 0), result);
    }

    [Test]
    public void GetVersionForState_Null_Returns_Null()
    {
        UmbracoPlan plan = CreateUmbracoPlan();

        Assert.IsNull(plan.GetVersionForState(null));
    }

    [Test]
    public void GetVersionForState_Empty_Returns_Null()
    {
        UmbracoPlan plan = CreateUmbracoPlan();

        Assert.IsNull(plan.GetVersionForState(string.Empty));
    }

    [Test]
    public void GetVersionForState_Unknown_Returns_Null()
    {
        UmbracoPlan plan = CreateUmbracoPlan();

        Assert.IsNull(plan.GetVersionForState("{00000000-0000-0000-0000-000000000000}"));
    }

    [Test]
    public void GetVersionForState_V17_2_0_Returns_Correct_Version()
    {
        UmbracoPlan plan = CreateUmbracoPlan();

        // Last migration state of the v17.2.0 block (AddDocumentUrlAlias).
        SemVersion? result = plan.GetVersionForState("{A7B8C9D0-E1F2-4A5B-8C7D-9E0F1A2B3C4D}");

        Assert.AreEqual(new SemVersion(17, 2, 0), result);
    }

    [Test]
    public void GetVersionForState_V17_3_0_Returns_Correct_Version()
    {
        UmbracoPlan plan = CreateUmbracoPlan();

        // A v17.3.0 migration state (PopulateSortableValueForDatePropertyData).
        SemVersion? result = plan.GetVersionForState("{6748CB56-CC16-49F0-BA91-B8ECE31BF456}");

        Assert.AreEqual(new SemVersion(17, 3, 0), result);
    }

    [Test]
    [Ignore("Current plan has no no-op migrations, enable and update state-id/version when one exists.")]
    public void GetVersionForState_NoopMigration_Carries_Forward_Previous_Version()
    {
        UmbracoPlan plan = CreateUmbracoPlan();

        // First NoopMigration in the v14.0.0 block - should carry forward the previous real version.
        // Since NoopMigration has no version namespace, the last real version (13.5.0) carries forward.
        SemVersion? result = plan.GetVersionForState("{419827A0-4FCE-464B-A8F3-247C6092AF55}");

        Assert.AreEqual(new SemVersion(13, 5, 0), result);
    }

    [Test]

    public void GetVersionForState_V17_0_0_Returns_Correct_Version()
    {
        UmbracoPlan plan = CreateUmbracoPlan();

        // First migration in v17.0.0 block (AddGuidsToAuditEntries)
        SemVersion? result = plan.GetVersionForState("{17D5F6CA-CEB8-462A-AF86-4B9C3BF91CF1}");

        Assert.AreEqual(new SemVersion(17, 0, 0), result);
    }

    [Test]
    public void GetVersionForState_FinalState_Returns_Version()
    {
        UmbracoPlan plan = CreateUmbracoPlan();

        SemVersion? result = plan.GetVersionForState(plan.FinalState);

        Assert.IsNotNull(result);
    }

    [Test]
    public void All_Migration_Types_Follow_Version_Namespace_Convention()
    {
        UmbracoPlan plan = CreateUmbracoPlan();

        var violations = new List<string>();
        foreach ((var _, MigrationPlan.Transition? transition) in plan.Transitions)
        {
            if (transition is null)
            {
                continue;
            }

            Type migrationType = transition.MigrationType;
            if (migrationType == typeof(NoopMigration))
            {
                continue;
            }

            var ns = migrationType.Namespace ?? string.Empty;
            if (UmbracoPlan.MigrationStepVersionRegex().IsMatch(ns) is false)
            {
                violations.Add($"{migrationType.FullName} (namespace: {ns})");
            }
        }

        Assert.IsEmpty(violations, $"Migration types with non-standard namespaces:\n{string.Join("\n", violations)}");
    }

    private static UmbracoPlan CreateUmbracoPlan()
    {
        var umbracoVersion = new Mock<IUmbracoVersion>();
        umbracoVersion.Setup(x => x.SemanticVersion).Returns(new SemVersion(17, 4, 0));
        return new UmbracoPlan(umbracoVersion.Object);
    }
}
