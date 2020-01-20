using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Web.Install.InstallSteps;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install
{
    public sealed class InstallStepCollection
    {
        private readonly InstallHelper _installHelper;
        private List<InstallSetupStep> _orderedInstallerSteps;
        private InstallSetupStep _finalstep;
        public event EventHandler<InstallStepCollection> PrepareListEvent;

        public InstallStepCollection(InstallHelper installHelper, IEnumerable<InstallSetupStep> installerSteps)
        {
            _installHelper = installHelper;

            // TODO: this is ugly but I have a branch where it's nicely refactored - for now we just want to manage ordering
            var a = installerSteps.ToArray();
            _orderedInstallerSteps = new List<InstallSetupStep>
            {
                a.OfType<NewInstallStep>().First(),
                a.OfType<UpgradeStep>().First(),
                a.OfType<FilePermissionsStep>().First(),
                a.OfType<ConfigureMachineKey>().First(),
                a.OfType<DatabaseConfigureStep>().First(),
                a.OfType<DatabaseInstallStep>().First(),
                a.OfType<DatabaseUpgradeStep>().First(),

                a.OfType<StarterKitDownloadStep>().First(),
                a.OfType<StarterKitInstallStep>().First(),
                a.OfType<StarterKitCleanupStep>().First(),


            };
            _finalstep =   a.OfType<SetUmbracoVersionStep>().First();
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
            OnPreparingList(this);
            _orderedInstallerSteps.Add(_finalstep);
            return _orderedInstallerSteps;

        }

        public void OnPreparingList(InstallStepCollection stepColletion)
        {
            PrepareListEvent?.Invoke(this,stepColletion);
        }

    /// <summary>
        /// Returns the steps that are used only for the current installation type
        /// </summary>
        /// <returns></returns>
        public IEnumerable<InstallSetupStep> GetStepsForCurrentInstallType()
        {
            return GetAllSteps().Where(x => x.InstallTypeTarget.HasFlag(_installHelper.GetInstallationType()));
        }
        public void RemoveStepByType(Type type)
        {
            _orderedInstallerSteps.RemoveAll(e=>e.Removable && e.StepType ==type);
        }
        public void Add(InstallSetupStep step)
        {
            _orderedInstallerSteps.Add(step);
        }
    }
}
