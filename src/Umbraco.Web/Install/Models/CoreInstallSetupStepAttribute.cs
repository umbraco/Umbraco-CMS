using System.Text.RegularExpressions;

namespace Umbraco.Web.Install.Models
{
    public sealed class CoreInstallSetupStepAttribute : InstallSetupStepAttribute
    {
        public CoreInstallSetupStepAttribute(CoreInstallationType installTypeTarget, string name, string view, int serverOrder, string description) 
            : base(name, view, serverOrder, description)
        {
            InstallTypeTarget = installTypeTarget;            
        }

        public CoreInstallSetupStepAttribute(CoreInstallationType installTypeTarget, string name, int serverOrder, string description)
            : base(name, serverOrder, description)
        {
            InstallTypeTarget = installTypeTarget;            
        }

        public CoreInstallationType InstallTypeTarget { get; private set; }
    }
}