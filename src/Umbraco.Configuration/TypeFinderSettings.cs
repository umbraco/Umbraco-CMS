using System.Configuration;
using Umbraco.Abstractions;
using Umbraco.Core;

namespace Umbraco.Configuration
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
