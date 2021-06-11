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
        // TODO: Should this take a name from an attribute or an abstract property? 
        protected PackageMigrationPlan(string name) : base(name)
        {
            // A call to From must be done first
            From(string.Empty);

            DefinePlan();
        }

        protected abstract void DefinePlan();

    }
}
