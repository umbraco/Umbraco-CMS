using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Configuration.Implementations
{
    internal class RequestHandlerSettings : ConfigurationManagerConfigBase, IRequestHandlerSettings
    {
        public bool AddTrailingSlash  => UmbracoSettingsSection?.RequestHandler?.AddTrailingSlash ?? true;
        public bool ConvertUrlsToAscii  => UmbracoSettingsSection?.RequestHandler?.UrlReplacing?.ConvertUrlsToAscii.InvariantEquals("true") ?? false;
        public bool TryConvertUrlsToAscii  => UmbracoSettingsSection?.RequestHandler?.UrlReplacing?.ConvertUrlsToAscii.InvariantEquals("try") ?? false;
        public IEnumerable<IChar> CharCollection => UmbracoSettingsSection?.RequestHandler?.UrlReplacing?.CharCollection ?? Enumerable.Empty<IChar>();
    }
}
