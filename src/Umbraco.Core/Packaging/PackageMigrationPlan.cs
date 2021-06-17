using System;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Packaging
{   

    /// <summary>
    /// Base class for package migration plans
    /// </summary>
    public abstract class PackageMigrationPlan : MigrationPlan, IDiscoverable
    {
        protected PackageMigrationPlan(string name) : base(name)
        {
            // A call to From must be done first
            From(string.Empty);

            DefinePlan();
        }

        /// <summary>
        /// Inform the plan executor to ignore all saved package state and
        /// run the migration from initial state to it's end state.
        /// </summary>
        public override bool IgnoreCurrentState => true;

        protected abstract void DefinePlan();

    }
}
