using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.MigrationSteps
{    
    [InstallSetupStep("Migrations", "migrations", 1, "Running Package migrations.")]
    internal class InitialMigrationStep : InstallSetupStep<object>
    {
        private readonly ApplicationContext _applicationContext;

        public InitialMigrationStep(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public override bool RequiresExecution(object model)
        {
            return true;
        }

        /// <summary>
        /// The view model used to render the view
        /// </summary>
        public override object ViewModel
        {
            get { return _applicationContext.GetPendingPackageMigrations(); }
        }

        public override InstallSetupResult Execute(object model)
        {
            return null;
        }
    }
}