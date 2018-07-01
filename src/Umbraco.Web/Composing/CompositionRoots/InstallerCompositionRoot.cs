﻿using Umbraco.Core.Composing;
using Umbraco.Web.Install;
using Umbraco.Web.Install.InstallSteps;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Composing.CompositionRoots
{
    /// <summary>
    /// A composition root for dealing with the installer and installer steps
    /// </summary>
    public sealed class InstallerCompositionRoot : IRegistrationBundle
    {
        public void Compose(IContainer container)
        {
            // fixme - Implement ordered composition instead of abusing DI container?
            // this creates an unnecessary requirement for containers.
            //register the installer steps in order
            container.RegisterOrdered(typeof(InstallSetupStep),
                new[]
                {
                    typeof(NewInstallStep),
                    typeof(UpgradeStep),
                    typeof(FilePermissionsStep),
                    typeof(ConfigureMachineKey),
                    typeof(DatabaseConfigureStep),
                    typeof(DatabaseInstallStep),
                    typeof(DatabaseUpgradeStep),

                    //TODO: Add these back once we have a compatible starter kit
                    //typeof(StarterKitDownloadStep),
                    //typeof(StarterKitInstallStep),
                    //typeof(StarterKitCleanupStep),

                    typeof(SetUmbracoVersionStep)
                }, type => Lifetime.PerScope);

            container.Register<InstallStepCollection>();
            container.Register<InstallHelper>();
            
            
        }
    }
}
