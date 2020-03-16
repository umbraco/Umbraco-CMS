using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Configuration.Implementations
{
    internal class RequestHandlerSettings : ConfigurationManagerConfigBase, IRequestHandlerSettings
    {
        public bool AddTrailingSlash  => UmbracoSettingsSection.RequestHandler.AddTrailingSlash;
        public bool ConvertUrlsToAscii  => UmbracoSettingsSection.RequestHandler.UrlReplacing.ConvertUrlsToAscii.InvariantEquals("true");
        public bool TryConvertUrlsToAscii  => UmbracoSettingsSection.RequestHandler.UrlReplacing.ConvertUrlsToAscii.InvariantEquals("try");
        public IEnumerable<IChar> CharCollection => UmbracoSettingsSection.RequestHandler.UrlReplacing.CharCollection;
    }
}
