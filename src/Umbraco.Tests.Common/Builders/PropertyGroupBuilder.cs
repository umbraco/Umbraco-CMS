using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Tests.Common.Builders.Interfaces;

namespace Umbraco.Tests.Common.Builders
{
    public class PropertyGroupBuilder : PropertyGroupBuilder<NullPropertyGroupBuilderParent>
    {
        public PropertyGroupBuilder() : base(null)
        {
        }
    }

    public class NullPropertyGroupBuilderParent : IBuildPropertyGroups
    {
    }

    public class PropertyGroupBuilder<TParent>
        : ChildBuilderBase<TParent, PropertyGroup>,
            IBuildPropertyTypes,
            IWithIdBuilder,
            IWithKeyBuilder,
            IWithCreateDateBuilder,
            IWithUpdateDateBuilder,
            IWithNameBuilder,
            IWithSortOrderBuilder where TParent: IBuildPropertyGroups
    {
        private readonly List<PropertyTypeBuilder<PropertyGroupBuilder<TParent>>> _propertyTypeBuilders = new List<PropertyTypeBuilder<PropertyGroupBuilder<TParent>>>();

        private int? _id;
        private Guid? _key;
        private DateTime? _createDate;
        private DateTime? _updateDate;
        private string _name;
        private int? _sortOrder;

        public PropertyGroupBuilder(TParent parentBuilder) : base(parentBuilder)
        {
        }

        public PropertyTypeBuilder<PropertyGroupBuilder<TParent>> AddPropertyType()
        {
            var builder = new PropertyTypeBuilder<PropertyGroupBuilder<TParent>>(this);
            _propertyTypeBuilders.Add(builder);
            return builder;
        }

        public override PropertyGroup Build()
        {
            var id = _id ?? 1;
            var key = _key ?? Guid.NewGuid();
            var createDate = _createDate ?? DateTime.Now;
            var updateDate = _updateDate ?? DateTime.Now;
            var name = _name ?? Guid.NewGuid().ToString();
            var sortOrder = _sortOrder ?? 0;

            var properties = new PropertyTypeCollection(false);
            foreach (var propertyType in _propertyTypeBuilders.Select(x => x.Build()))
            {
                properties.Add(propertyType);
            }

            Reset();
            return new PropertyGroup(properties)
            {
                Id = id,
                Key = key,
                Name = name,
                SortOrder = sortOrder,
                CreateDate = createDate,
                UpdateDate = updateDate,
            };
        }

        protected override void Reset()
        {
            _id = null;
            _key = null;
            _createDate = null;
            _updateDate = null;
            _name = null;
            _sortOrder = null;
            _propertyTypeBuilders.Clear();
        }

        int? IWithIdBuilder.Id
        {
            get => _id;
            set => _id = value;
        }

        Guid? IWithKeyBuilder.Key
        {
            get => _key;
            set => _key = value;
        }

        string IWithNameBuilder.Name
        {
            get => _name;
            set => _name = value;
        }

        DateTime? IWithCreateDateBuilder.CreateDate
        {
            get => _createDate;
            set => _createDate = value;
        }

        DateTime? IWithUpdateDateBuilder.UpdateDate
        {
            get => _updateDate;
            set => _updateDate = value;
        }

        int? IWithSortOrderBuilder.SortOrder
        {
            get => _sortOrder;
            set => _sortOrder = value;
        }
    }
}
