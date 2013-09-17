using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IProvidersSection : IUmbracoConfigurationSection
    {        
        string DefaultBackOfficeUserProvider { get; }
    }
}