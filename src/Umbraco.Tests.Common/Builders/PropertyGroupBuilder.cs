using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Tests.Common.Builders.Interfaces;

namespace Umbraco.Tests.Common.Builders
{
    public class PropertyGroupBuilder
        : ChildBuilderBase<MemberTypeBuilder, PropertyGroup>,   // TODO: likely want to genericise this, so can use for document and media types too.
            IWithNameBuilder,
            IWithSortOrderBuilder
    {
        private readonly List<PropertyTypeBuilder> _propertyTypeBuilders = new List<PropertyTypeBuilder>();

        private string _name;
        private int? _sortOrder;

        public PropertyGroupBuilder(MemberTypeBuilder parentBuilder) : base(parentBuilder)
        {
        }

        public PropertyTypeBuilder AddPropertyType()
        {
            var builder = new PropertyTypeBuilder(this);
            _propertyTypeBuilders.Add(builder);
            return builder;
        }

        public override PropertyGroup Build()
        {
            var name = _name ?? Guid.NewGuid().ToString();
            var sortOrder = _sortOrder ?? 0;

            var properties = new PropertyTypeCollection(false);
            foreach (var propertyType in _propertyTypeBuilders.Select(x => x.Build()))
            {
                properties.Add(propertyType);
            }

            return new PropertyGroup(properties)
            {
                Name = name,
                SortOrder = sortOrder,
            };
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
    }
}
