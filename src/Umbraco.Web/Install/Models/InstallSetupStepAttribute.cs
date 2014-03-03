using System;

namespace Umbraco.Web.Install.Models
{
    public sealed class InstallSetupStepAttribute : Attribute
    {
        public InstallSetupStepAttribute(string name, string view, string description)
        {
            Name = name;
            View = view;
            Description = description;
        }

        public string Name { get; private set; }
        public string View { get; private set; }
        public string Description { get; private set; }
    }
}