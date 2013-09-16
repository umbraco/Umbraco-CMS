using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IProviders
    {        
        string DefaultBackOfficeUserProvider { get; }
    }
}