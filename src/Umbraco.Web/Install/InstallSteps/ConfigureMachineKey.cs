using System;
using System.Configuration;
using System.Linq;
using System.Web.Configuration;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Security;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall,
        "ConfigureMachineKey", "machinekey", 2,
        "Updating some security settings...",
        PerformsAppRestart = true)]
    internal class ConfigureMachineKey : InstallSetupStep<bool?>
    {
        private readonly ApplicationContext _appContext;

        public ConfigureMachineKey(ApplicationContext appContext)
        {
            if (appContext == null) throw new ArgumentNullException("appContext");
            _appContext = appContext;
        }

        public override string View
        {
            get { return HasMachineKey() == false ? base.View : ""; }
        }

        /// <summary>
        /// Don't display the view or execute if a machine key already exists
        /// </summary>
        /// <returns></returns>
        private bool HasMachineKey()
        {
            var section = (MachineKeySection)WebConfigurationManager.GetSection("system.web/machineKey");
            return section.ElementInformation.Source != null;
        }

        /// <summary>
        /// The step execution method
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public override InstallSetupResult Execute(bool? model)
        {
            if (model.HasValue && model.Value == false) return null;

            //install the machine key
            var fileName = IOHelper.MapPath(string.Format("{0}/web.config", SystemDirectories.Root));
            var xml = XDocument.Load(fileName, LoadOptions.PreserveWhitespace);

            var systemWeb = xml.Root.DescendantsAndSelf("system.web").Single();

            // Update appSetting if it exists, or else create a new appSetting for the given key and value
            var machineKey = systemWeb.Descendants("machineKey").FirstOrDefault();
            if (machineKey != null) return null;

            var generator = new MachineKeyGenerator();
            var generatedSection = generator.GenerateConfigurationBlock();
            systemWeb.Add(XElement.Parse(generatedSection));

            xml.Save(fileName, SaveOptions.DisableFormatting);

            return null;
        }

        public override bool RequiresExecution(bool? model)
        {
            return HasMachineKey() == false;
        }
    }
}