using System;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Semver;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade
{
    /// <summary>
    /// Represents the Umbraco CMS migration plan.
    /// </summary>
    /// <seealso cref="MigrationPlan" />
    public class UmbracoPlan : MigrationPlan
    {
        private static readonly SemVersion MinVersion = new SemVersion(9, 5);

        private readonly IUmbracoVersion _umbracoVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoPlan" /> class.
        /// </summary>
        /// <param name="umbracoVersion">The Umbraco version.</param>
        public UmbracoPlan(IUmbracoVersion umbracoVersion)
            : base(Constants.Conventions.Migrations.UmbracoUpgradePlanName)
        {
            _umbracoVersion = umbracoVersion;
            DefinePlan();
        }

        /// <inheritdoc />
        public override void ThrowOnUnknownInitialState(string state)
        {
            var currentVersion = _umbracoVersion.SemanticVersion;
            if (currentVersion < MinVersion)
            {
                throw new InvalidOperationException($"Version {currentVersion} cannot be migrated, please upgrade to at least {MinVersion} first.");
            }

            base.ThrowOnUnknownInitialState(state);
        }

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
            // out of the conflict states (see the example of merging the empty initial state and last v9 state).

            From(string.Empty)
                .Merge()
                    // A new v10 install will have an empty initial state
                .With()
                    // Merge with the last migration state of v9 (ThrowOnUnknownInitialState ensured this migration has already executed)
                    .To("{DED98755-4059-41BB-ADBD-3FEAB12D1D7B}") // TODO Change this to the final v9 migration state!
                .As("{init-10.0.0}");
        }
    }
}
