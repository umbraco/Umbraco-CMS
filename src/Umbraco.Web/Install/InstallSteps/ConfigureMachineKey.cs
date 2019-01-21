using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Xml.Linq;
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
        public override string View => HasMachineKey() == false ? base.View : "";

        /// <summary>
        /// Don't display the view or execute if a machine key already exists
        /// </summary>
        /// <returns></returns>
        private static bool HasMachineKey()
        {
            var section = (MachineKeySection) WebConfigurationManager.GetSection("system.web/machineKey");
            return section.ElementInformation.Source != null;
        }

        /// <summary>
        /// The step execution method
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public override Task<InstallSetupResult> ExecuteAsync(bool? model)
        {
            if (model.HasValue && model.Value == false) return null;

            //install the machine key
            var fileName = IOHelper.MapPath($"{SystemDirectories.Root}/web.config");
            var xml = XDocument.Load(fileName, LoadOptions.PreserveWhitespace);

            var systemWeb = xml.Root.DescendantsAndSelf("system.web").Single();

            // Update appSetting if it exists, or else create a new appSetting for the given key and value
            var machineKey = systemWeb.Descendants("machineKey").FirstOrDefault();
            if (machineKey != null) return null;

            var generator = new MachineKeyGenerator();
            var generatedSection = generator.GenerateConfigurationBlock();
            systemWeb.Add(XElement.Parse(generatedSection));

            xml.Save(fileName, SaveOptions.DisableFormatting);

            return Task.FromResult<InstallSetupResult>(null);
        }

        public override bool RequiresExecution(bool? model)
        {
            return HasMachineKey() == false;
        }
    }
}
