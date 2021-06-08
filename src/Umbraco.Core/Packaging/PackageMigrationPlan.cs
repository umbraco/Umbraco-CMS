using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Migrations;

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

        protected abstract void DefinePlan();
    }
}
