using System;
using Moq;
using Umbraco.Core.Models;
using Umbraco.Core.Strings;
using Umbraco.Tests.Common.Builders.Extensions;
using Umbraco.Tests.Common.Builders.Interfaces;

namespace Umbraco.Tests.Common.Builders
{
    public class PropertyTypeBuilder
        : ChildBuilderBase<PropertyGroupBuilder, PropertyType>,
            IWithAliasBuilder,
            IWithNameBuilder,
            IWithSortOrderBuilder,
            IWithDescriptionBuilder
    {
        private string _propertyEditorAlias;
        private ValueStorageType? _valueStorageType;
        private string _alias;
        private string _name;
        private int? _sortOrder;
        private string _description;
        private int? _dataTypeId;

        public PropertyTypeBuilder(PropertyGroupBuilder parentBuilder) : base(parentBuilder)
        {
        }

        public PropertyTypeBuilder WithPropertyEditorAlias(string propertyEditorAlias)
        {
            _propertyEditorAlias = propertyEditorAlias;
            return this;
        }

        public PropertyTypeBuilder WithValueStorageType(ValueStorageType valueStorageType)
        {
            _valueStorageType = valueStorageType;
            return this;
        }

        public PropertyTypeBuilder WithDataTypeId(int dataTypeId)
        {
            _dataTypeId = dataTypeId;
            return this;
        }

        public override PropertyType Build()
        {
            var propertyEditorAlias = _propertyEditorAlias ?? Guid.NewGuid().ToString().ToCamelCase();
            var valueStorageType = _valueStorageType ?? ValueStorageType.Ntext;
            var name = _name ?? Guid.NewGuid().ToString();
            var alias = _alias ?? name.ToCamelCase();
            var sortOrder = _sortOrder ?? 0;
            var dataTypeId = _dataTypeId ?? 0;
            var description = _description ?? string.Empty;

            var shortStringHelper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig());

            return new PropertyType(shortStringHelper, propertyEditorAlias, valueStorageType)
            {
                Alias = alias,
                Name = name,
                SortOrder = sortOrder,
                DataTypeId = dataTypeId,
                Description = description,
            };
        }

        string IWithAliasBuilder.Alias
        {
            get => _alias;
            set => _alias = value;
        }

        string IWithNameBuilder.Name
        {
            get => _name;
            set => _name = value;
        }

        int? IWithSortOrderBuilder.SortOrder
        {
            get => _sortOrder;
            set => _sortOrder = value;
        }

        string IWithDescriptionBuilder.Description
        {
            get => _description;
            set => _description = value;
        }
    }
}
