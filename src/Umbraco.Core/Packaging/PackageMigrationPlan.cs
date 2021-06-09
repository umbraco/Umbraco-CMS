using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Cms.Core.Migrations;

namespace Umbraco.Cms.Core.Packaging
{
    public abstract class PackageMigrationPlan : MigrationPlan
    {
        protected PackageMigrationPlan(string name) : base(name)
        {
        }
    }
}
