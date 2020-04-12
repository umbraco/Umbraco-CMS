using System;
using Umbraco.Core.Models;
using Umbraco.Tests.Common.Builders.Interfaces;

namespace Umbraco.Tests.Common.Builders
{
    public class PropertyBuilder
        : BuilderBase<IProperty>,
            IBuildPropertyTypes,
            IWithIdBuilder,
            IWithKeyBuilder,
            IWithCreateDateBuilder,
            IWithUpdateDateBuilder
    {
        private PropertyTypeBuilder<PropertyBuilder> _propertyTypeBuilder;

        private int? _id;
        private Guid? _key;
        private DateTime? _createDate;
        private DateTime? _updateDate;

        public PropertyTypeBuilder<PropertyBuilder> AddPropertyType()
        {
            var builder = new PropertyTypeBuilder<PropertyBuilder>(this);
            _propertyTypeBuilder = builder;
            return builder;
        }

        public override IProperty Build()
        {
            var id = _id ?? 1;
            var key = _key ?? Guid.NewGuid();
            var createDate = _createDate ?? DateTime.Now;
            var updateDate = _updateDate ?? DateTime.Now;

            // Needs to be within collection to support publishing.
            var propertyTypeCollection = new PropertyTypeCollection(true, new[] { _propertyTypeBuilder.Build() });

            Reset();
            return new Property(id, propertyTypeCollection[0])
            {
                Key = key,
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
    }
}
