using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Tests.Common.Builders
{
    public class ConfigurationEditorBuilder<TParent> : ChildBuilderBase<TParent, IConfigurationEditor>
    {
        private IDictionary<string, object> _defaultConfiguration;

        public ConfigurationEditorBuilder(TParent parentBuilder) : base(parentBuilder)
        {
        }

        public ConfigurationEditorBuilder<TParent> WithDefaultConfiguration(IDictionary<string, object> defaultConfiguration)
        {
            _defaultConfiguration = defaultConfiguration;
            return this;
        }

        public override IConfigurationEditor Build()
        {
            var defaultConfiguration  = _defaultConfiguration ?? new Dictionary<string, object>();

            Reset();
            return new ConfigurationEditor()
            {
                DefaultConfiguration = defaultConfiguration,
            };
        }

        protected override void Reset()
        {
            _defaultConfiguration = null;
        }
    }
}
