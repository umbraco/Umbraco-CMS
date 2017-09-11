using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IInternalSearchFieldsToSearchSection : IUmbracoConfigurationSection
    {
        IEnumerable<string> ContentSearchFields { get; }
    }
}
