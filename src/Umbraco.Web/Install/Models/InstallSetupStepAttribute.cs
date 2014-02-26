using System;

namespace Umbraco.Web.Install.Models
{
    public sealed class InstallSetupStepAttribute : Attribute
    {
        public InstallSetupStepAttribute(string name, string view)
        {
            Name = name;
            View = view;
        }

        public string Name { get; private set; }
        public string View { get; private set; }
    }
}