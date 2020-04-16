using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IRequestHandlerSettings : IUmbracoConfigurationSection
    {
        bool AddTrailingSlash { get; }

        bool ConvertUrlsToAscii { get; }

        bool TryConvertUrlsToAscii { get; }

        IEnumerable<IChar> CharCollection { get; }
    }
}
