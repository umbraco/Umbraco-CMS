using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Core.Packaging;

/// <summary>
///     Base class for package migration plans
/// </summary>
public abstract class PackageMigrationPlan : MigrationPlan, IDiscoverable
{
    /// <summary>
    ///     Creates a package migration plan
    /// </summary>
    /// <param name="packageName">The name of the package. If the package has a package.manifest these must match.</param>
    protected PackageMigrationPlan(string packageName)
        : this(packageName, packageName)
    {
    }

    /// <summary>
    ///     Create a plan for a Package Name
    /// </summary>
    /// <param name="packageName">
    ///     The package name that the plan is for. If the package has a package.manifest these must
    ///     match.
    /// </param>
    /// <param name="planName">
    ///     The plan name for the package. This should be the same name as the
    ///     package name if there is only one plan in the package.
    /// </param>
    protected PackageMigrationPlan(string packageName, string planName)
        : base(planName)
    {
        // A call to From must be done first
        From(string.Empty);

        DefinePlan();
        PackageName = packageName;
    }

    /// <summary>
    ///     Inform the plan executor to ignore all saved package state and
    ///     run the migration from initial state to it's end state.
    /// </summary>
    public override bool IgnoreCurrentState => true;

    /// <summary>
    ///     Returns the Package Name for this plan
    /// </summary>
    public string PackageName { get; }

    protected abstract void DefinePlan();
}
