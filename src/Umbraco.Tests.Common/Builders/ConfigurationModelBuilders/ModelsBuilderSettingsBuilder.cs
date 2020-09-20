using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;

namespace Umbraco.Tests.Common.Builders
{
    public class ModelsBuilderSettingsBuilder : BuilderBase<ModelsBuilderSettings>
    {
        private string _modelsMode;

        public ModelsBuilderSettingsBuilder WithModelsMode(string modelsMode)
        {
            _modelsMode = modelsMode;
            return this;
        }

        public override ModelsBuilderSettings Build()
        {
            var modelsMode = _modelsMode ?? ModelsMode.Nothing.ToString();

            return new ModelsBuilderSettings
            {
                ModelsMode = modelsMode,
            };
        }
    }
}
