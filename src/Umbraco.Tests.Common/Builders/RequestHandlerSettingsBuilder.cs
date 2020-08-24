using System.Collections.Generic;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Tests.Common.Builders
{
    public class RequestHandlerSettingsBuilder : BuilderBase<RequestHandlerSettings>
    {
        private bool? _addTrailingSlash;
        private bool? _convertUrlsToAscii;
        private IEnumerable<IChar> _charCollection;

        public RequestHandlerSettingsBuilder WithAddTrailingSlash(bool addTrailingSlash)
        {
            _addTrailingSlash = addTrailingSlash;
            return this;
        }

        public RequestHandlerSettingsBuilder WithConvertUrlsToAscii(bool convertUrlsToAscii)
        {
            _convertUrlsToAscii = convertUrlsToAscii;
            return this;
        }

        public RequestHandlerSettingsBuilder WithCharCollection(IEnumerable<IChar> charCollection)
        {
            _charCollection = charCollection;
            return this;
        }
        

        public override RequestHandlerSettings Build()
        {
            var addTrailingSlash = _addTrailingSlash ?? false;
            var convertUrlsToAscii = _convertUrlsToAscii ?? false;
            var charCollection = _charCollection ?? RequestHandlerSettings.DefaultCharCollection;

            return new RequestHandlerSettings
            {
                AddTrailingSlash = addTrailingSlash,
                ConvertUrlsToAscii = convertUrlsToAscii,
                CharCollection = charCollection,
            };
        }
    }
}
