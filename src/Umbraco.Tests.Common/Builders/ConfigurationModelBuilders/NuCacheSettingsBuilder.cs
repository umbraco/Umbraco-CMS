using Umbraco.Core.Configuration.Models;

namespace Umbraco.Tests.Common.Builders
{
    public class NuCacheSettingsBuilder : BuilderBase<NuCacheSettings>
    {
        public override NuCacheSettings Build()
        {
            return new NuCacheSettings();
        }
    }
}
