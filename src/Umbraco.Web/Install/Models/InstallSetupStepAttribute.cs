using System;
using System.Text.RegularExpressions;

namespace Umbraco.Web.Install.Models
{
    public sealed class InstallSetupStepAttribute : Attribute
    {
        public InstallSetupStepAttribute(InstallationType installTypeTarget, string name, string view, int serverOrder, string description)
        {
            InstallTypeTarget = installTypeTarget;
            Name = name;
            View = view;
            ServerOrder = serverOrder;
            Description = description;

            var r = new Regex("", RegexOptions.Compiled | RegexOptions.Compiled);
        }

        public InstallSetupStepAttribute(InstallationType installTypeTarget, string name, int serverOrder, string description)
        {
            InstallTypeTarget = installTypeTarget;
            Name = name;
            View = string.Empty;
            ServerOrder = serverOrder;
            Description = description;
        }

        public InstallationType InstallTypeTarget { get; private set; }
        public string Name { get; private set; }
        public string View { get; private set; }
        public int ServerOrder { get; private set; }
        public string Description { get; private set; }
    }
}