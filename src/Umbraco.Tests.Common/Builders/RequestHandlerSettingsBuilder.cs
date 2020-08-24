using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;

namespace Umbraco.Tests.Common.Builders
{
    public class RequestHandlerSettingsBuilder : BuilderBase<RequestHandlerSettings>
    {
        private bool? _addTrailingSlash;

        public RequestHandlerSettingsBuilder WithAddTrailingSlash(bool addTrailingSlash)
        {
            _addTrailingSlash = addTrailingSlash;
            return this;
        }

        public override RequestHandlerSettings Build()
        {
            var addTrailingSlash = _addTrailingSlash ?? false;

            return new RequestHandlerSettings
            {
                AddTrailingSlash = addTrailingSlash,             
            };
        }
    }
}
