using Umbraco.Core.Configuration.Models;

namespace Umbraco.Tests.Common.Builders
{
    public class ModelsBuilderConfigBuilder : BuilderBase<ModelsBuilderSettings>
    {
        public override ModelsBuilderSettings Build()
        {
            return new ModelsBuilderSettings();
        }
    }
}
