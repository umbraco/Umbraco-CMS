using System;

namespace Umbraco.Web.Install.Models
{
    public sealed class InstallSetupStepAttribute : Attribute
    {
        public InstallSetupStepAttribute(string name, string view, int serverOrder)
        {
            Name = name;
            View = view;
            ServerOrder = serverOrder;
            Description = description;
        }

        public InstallSetupStepAttribute(string name, int serverOrder)
        {
            Name = name;
            View = string.Empty;
            ServerOrder = serverOrder;
        }

        public string Name { get; private set; }
        public string View { get; private set; }
        public int ServerOrder { get; private set; }
        public string Description { get; private set; }
    }
}