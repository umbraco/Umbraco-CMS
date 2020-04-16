using System.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Legacy
{
    public class TypeFinderSettings : ITypeFinderSettings
    {
        public TypeFinderSettings()
        {
            AssembliesAcceptingLoadExceptions = ConfigurationManager.AppSettings[
                Constants.AppSettings.AssembliesAcceptingLoadExceptions];
        }

        public string AssembliesAcceptingLoadExceptions { get; }
    }
}
