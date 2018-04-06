using System.Collections.Generic;
using System.Linq;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install
{
    public sealed class InstallStepCollection
    {
        private readonly InstallHelper _installHelper;
        private readonly IEnumerable<InstallSetupStep> _orderedInstallerSteps;

        public InstallStepCollection(InstallHelper installHelper, IEnumerable<InstallSetupStep> orderedInstallerSteps)
        {
            _installHelper = installHelper;
            _orderedInstallerSteps = orderedInstallerSteps;
        }


        /// <summary>
        /// Get the installer steps
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// The step order returned here is how they will appear on the front-end if they have views assigned
        /// </remarks>
        public IEnumerable<InstallSetupStep> GetAllSteps()
        {
            return _orderedInstallerSteps;
        }

        /// <summary>
        /// Returns the steps that are used only for the current installation type
        /// </summary>
        /// <returns></returns>
        public IEnumerable<InstallSetupStep> GetStepsForCurrentInstallType()
        {
            return GetAllSteps().Where(x => x.InstallTypeTarget.HasFlag(_installHelper.GetInstallationType()));
        }
    }
}
