using Microsoft.Extensions.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    internal class TypeFinderSettings : ITypeFinderSettings
    {
        private readonly IConfiguration _configuration;

        public TypeFinderSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string AssembliesAcceptingLoadExceptions =>
            _configuration.GetValue<string>("Umbraco:CMS:TypeFinder:AssembliesAcceptingLoadExceptions");
    }
}
