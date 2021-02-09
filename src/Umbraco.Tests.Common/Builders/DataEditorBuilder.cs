// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;

namespace Umbraco.Tests.Common.Builders
{
    public class DataEditorBuilder<TParent> : ChildBuilderBase<TParent, IDataEditor>
    {
        private readonly ConfigurationEditorBuilder<DataEditorBuilder<TParent>> _explicitConfigurationEditorBuilder;
        private readonly DataValueEditorBuilder<DataEditorBuilder<TParent>> _explicitValueEditorBuilder;
        private IDictionary<string, object> _defaultConfiguration;

        public DataEditorBuilder(TParent parentBuilder)
            : base(parentBuilder)
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
            IDictionary<string, object> defaultConfiguration = _defaultConfiguration ?? new Dictionary<string, object>();
            IConfigurationEditor explicitConfigurationEditor = _explicitConfigurationEditorBuilder.Build();
            IDataValueEditor explicitValueEditor = _explicitValueEditorBuilder.Build();

            return new DataEditor(
                NullLoggerFactory.Instance,
                Mock.Of<IDataTypeService>(),
                Mock.Of<ILocalizationService>(),
                Mock.Of<ILocalizedTextService>(),
                Mock.Of<IShortStringHelper>(),
                Mock.Of<IJsonSerializer>())
            {
                DefaultConfiguration = defaultConfiguration,
                ExplicitConfigurationEditor = explicitConfigurationEditor,
                ExplicitValueEditor = explicitValueEditor
            };
        }
    }
}
