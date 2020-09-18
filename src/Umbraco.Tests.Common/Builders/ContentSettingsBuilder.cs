using Umbraco.Core.Configuration.Models;

namespace Umbraco.Tests.Common.Builders
{
    public class ContentSettingsBuilder : BuilderBase<ContentSettings>
    {
        public override ContentSettings Build()
        {
            return new ContentSettings();
        }
    }
}
