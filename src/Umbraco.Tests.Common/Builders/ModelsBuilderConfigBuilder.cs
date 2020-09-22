using Umbraco.Core.Configuration.Models;

namespace Umbraco.Tests.Common.Builders
{
    public class ModelsBuilderConfigBuilder : BuilderBase<ModelsBuilderConfig>
    {
        public override ModelsBuilderConfig Build()
        {
            return new ModelsBuilderConfig();
        }
    }
}
