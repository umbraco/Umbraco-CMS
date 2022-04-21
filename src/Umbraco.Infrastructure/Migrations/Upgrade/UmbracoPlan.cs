using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade
{
    /// <summary>
    /// Represents the Umbraco CMS migration plan.
    /// </summary>
    /// <seealso cref="MigrationPlan" />
    public class UmbracoPlan : MigrationPlan
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoPlan" /> class.
        /// </summary>
        public UmbracoPlan()
            : base(Constants.Conventions.Migrations.UmbracoUpgradePlanName)
            => DefinePlan();

        /// <summary>
        /// Gets the initial state.
        /// </summary>
        /// <remarks>
        /// This equals the last migration state of v9, making that as the lowest supported version to upgrade from.
        /// </remarks>
        public override string InitialState => "{DED98755-4059-41BB-ADBD-3FEAB12D1D7B}"; // TODO Change this to the final v9 migration state!

        /// <summary>
        /// Defines the plan.
        /// </summary>
        protected void DefinePlan()
        {
            // MODIFYING THE PLAN
            //
            // Please take great care when modifying the plan!
            //
            // Append the migration to the main chain using a unique GUID (wrapped in curly braces).
            //
            // If the new migration causes a merge conflict, because someone else also added another
            // new migration, you NEED to fix the conflict by providing one default path, and paths
            // out of the conflict states, eg:
            //
            // .From("state-1")
            // .To<ChangeA>("state-a")
            // .To<ChangeB>("state-b") // Some might already be in this state, without having applied ChangeA
            //
            // .From("state-1")
            // .Merge()
            //     .To<ChangeA>("state-a")
            // .With()
            //     .To<ChangeB>("state-b")
            // .As("state-2");

            From(InitialState)
                .To("{init-10.0.0}");
        }
    }
}
