using System;
using Moq;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Tests.Common.Builders
{
    public class DataValueEditorBuilder<TParent> : ChildBuilderBase<TParent, IDataValueEditor>
    {
        private string _configuration;
        private string _view;
        private bool? _hideLabel;
        private string _valueType;

        public DataValueEditorBuilder(TParent parentBuilder) : base(parentBuilder)
        {
        }

        public DataValueEditorBuilder<TParent> WithConfiguration(string configuration)
        {
            _configuration = configuration;
            return this;
        }

        public DataValueEditorBuilder<TParent> WithView(string view)
        {
            _view = view;
            return this;
        }

        public DataValueEditorBuilder<TParent> WithHideLabel(bool hideLabel)
        {
            _hideLabel = hideLabel;
            return this;
        }

        public DataValueEditorBuilder<TParent> WithValueType(string valueType)
        {
            _valueType = valueType;
            return this;
        }

        public override IDataValueEditor Build()
        {
            var configuration = _configuration ?? null;
            var view = _view ?? null;
            var hideLabel = _hideLabel ?? false;
            var valueType = _valueType ?? Guid.NewGuid().ToString();

            return new DataValueEditor(
                Mock.Of<IDataTypeService>(),
                Mock.Of<ILocalizationService>(),
                Mock.Of<ILocalizedTextService>(),
                Mock.Of<IShortStringHelper>(),
                Mock.Of<IJsonSerializer>()
            )
            {
                Configuration = configuration,
                View = view,
                HideLabel = hideLabel,
                ValueType = valueType,
            };
        }
    }
}
