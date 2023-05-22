using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Core.Packaging;

/// <summary>
/// Represents a package migration plan.
/// </summary>
public abstract class PackageMigrationPlan : MigrationPlan, IDiscoverable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PackageMigrationPlan" /> class.
    /// </summary>
    /// <param name="packageName">The package name that the plan is for. If the package has a package.manifest these must match.</param>
    protected PackageMigrationPlan(string packageName)
        : this(packageName, packageName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PackageMigrationPlan" /> class.
    /// </summary>
    /// <param name="packageName">The package name that the plan is for. If the package has a package.manifest these must match.</param>
    /// <param name="planName">The plan name for the package. This should be the same name as the package name, if there is only one plan in the package.</param>
    protected PackageMigrationPlan(string packageName, string planName)
        : this(null!, packageName, planName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PackageMigrationPlan" /> class.
    /// </summary>
    /// <param name="packageId">The package identifier that the plan is for. If the package has a package.manifest these must match.</param>
    /// <param name="packageName">The package name that the plan is for. If the package has a package.manifest these must match.</param>
    /// <param name="planName">The plan name for the package. This should be the same name as the package name, if there is only one plan in the package.</param>
    protected PackageMigrationPlan(string packageId, string packageName, string planName)
        : base(planName)
    {
        PackageId = packageId;
        PackageName = packageName;

        // A call to From must be done first
        From(string.Empty);
        DefinePlan();
    }

    /// <summary>
    /// Inform the plan executor to ignore all saved package state and
    /// run the migration from initial state to it's end state.
    /// </summary>
    public override bool IgnoreCurrentState => true;

    /// <summary>
    /// Gets the package identifier.
    /// </summary>
    /// <value>
    /// The package identifier.
    /// </value>
    public string? PackageId { get; init; }

    /// <summary>
    /// Gets the package name.
    /// </summary>
    /// <value>
    /// The package name.
    /// </value>
    public string PackageName { get; init; }

    /// <summary>
    /// Defines the plan.
    /// </summary>
    protected abstract void DefinePlan();
}
