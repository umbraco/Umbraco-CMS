using System;
using Semver;

namespace Umbraco.Core.Deploy
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class DeploySupportAttribute : Attribute
    {
        public DeploySupportAttribute(string version)
        {
            Version = version;
        }

        public SemVersion Version { get; private set; }
    }
}
