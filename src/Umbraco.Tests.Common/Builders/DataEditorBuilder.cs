using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Tests.Common.Builders
{
    public class DataEditorBuilder<TParent> : ChildBuilderBase<TParent, IDataEditor>
    {
        private ConfigurationEditorBuilder<DataEditorBuilder<TParent>> _explicitConfigurationEditorBuilder;
        private DataValueEditorBuilder<DataEditorBuilder<TParent>> _explicitValueEditorBuilder;
        private IDictionary<string, object> _defaultConfiguration;

        public DataEditorBuilder(TParent parentBuilder) : base(parentBuilder)
        {
            _explicitConfigurationEditorBuilder = new ConfigurationEditorBuilder<DataEditorBuilder<TParent>>(this);
            _explicitValueEditorBuilder = new DataValueEditorBuilder<DataEditorBuilder<TParent>>(this);
        }

        public DataEditorBuilder<TParent> WithDefaultConfiguration(IDictionary<string, object> defaultConfiguration)
        {
            _defaultConfiguration = defaultConfiguration;
            return this;
        }

        public ConfigurationEditorBuilder<DataEditorBuilder<TParent>> AddExplicitConfigurationEditorBuilder() =>
            _explicitConfigurationEditorBuilder;

        public DataValueEditorBuilder<DataEditorBuilder<TParent>> AddExplicitValueEditorBuilder() =>
            _explicitValueEditorBuilder;

        public override IDataEditor Build()
        {
            var defaultConfiguration = _defaultConfiguration ?? new Dictionary<string, object>();
            var explicitConfigurationEditor = _explicitConfigurationEditorBuilder.Build();
            var explicitValueEditor = _explicitValueEditorBuilder.Build();

            return new DataEditor(
                NullLoggerFactory.Instance,
                Mock.Of<IDataTypeService>(),
                Mock.Of<ILocalizationService>(),
                Mock.Of<ILocalizedTextService>(),
                Mock.Of<IShortStringHelper>()
            )
            {
                DefaultConfiguration = defaultConfiguration,
                ExplicitConfigurationEditor = explicitConfigurationEditor,
                ExplicitValueEditor = explicitValueEditor
            };
        }
    }
}
