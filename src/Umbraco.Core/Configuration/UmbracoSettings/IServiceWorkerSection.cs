using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IServiceWorkerSection
    {
        IEnumerable<string> Domains { get; }
    }
}
