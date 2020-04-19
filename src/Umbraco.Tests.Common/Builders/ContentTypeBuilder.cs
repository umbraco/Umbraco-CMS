using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Tests.Common.Builders.Interfaces;

namespace Umbraco.Tests.Common.Builders
{
    public class ContentTypeBuilder
        : ContentTypeBaseBuilder<ContentBuilder, IContentType>,
            IWithPropertyTypeIdsIncrementingFrom
    {
        private List<PropertyGroupBuilder<ContentTypeBuilder>> _propertyGroupBuilders = new List<PropertyGroupBuilder<ContentTypeBuilder>>();
        private List<TemplateBuilder> _templateBuilders = new List<TemplateBuilder>();
        private List<ContentTypeSortBuilder> _allowedContentTypeBuilders = new List<ContentTypeSortBuilder>();

        private int? _propertyTypeIdsIncrementingFrom;
        private int? _defaultTemplateId;

        public ContentTypeBuilder() : base(null)
        {
        }

        public ContentTypeBuilder(ContentBuilder parentBuilder) : base(parentBuilder)
        {
        }

        public ContentTypeBuilder WithDefaultTemplateId(int templateId)
        {
            _defaultTemplateId = templateId;
            return this;
        }

        public PropertyGroupBuilder<ContentTypeBuilder> AddPropertyGroup()
        {
            var builder = new PropertyGroupBuilder<ContentTypeBuilder>(this);
            _propertyGroupBuilders.Add(builder);
            return builder;
        }

        public TemplateBuilder AddAllowedTemplate()
        {
            var builder = new TemplateBuilder(this);
            _templateBuilders.Add(builder);
            return builder;
        }

        public ContentTypeSortBuilder AddAllowedContentType()
        {
            var builder = new ContentTypeSortBuilder(this);
            _allowedContentTypeBuilders.Add(builder);
            return builder;
        }

        public override IContentType Build()
        {
            var contentType = new ContentType(ShortStringHelper, GetParentId())
            {
                Id = GetId(),
                Key = GetKey(),
                CreateDate = GetCreateDate(),
                UpdateDate = GetUpdateDate(),
                Alias = GetAlias(),
                Name = GetName(),
                Level = GetLevel(),
                Path = GetPath(),
                SortOrder = GetSortOrder(),
                Description = GetDescription(),
                Icon = GetIcon(),
                Thumbnail = GetThumbnail(),
                CreatorId = GetCreatorId(),
                Trashed = GetTrashed(),
                IsContainer = GetIsContainer(),
            };

            BuildPropertyGroups(contentType, _propertyGroupBuilders.Select(x => x.Build()));
            BuildPropertyTypeIds(contentType, _propertyTypeIdsIncrementingFrom);

            contentType.AllowedTemplates = _templateBuilders.Select(x => x.Build());
            contentType.AllowedContentTypes = _allowedContentTypeBuilders.Select(x => x.Build());

            if (_defaultTemplateId.HasValue)
            {
                contentType.SetDefaultTemplate(contentType.AllowedTemplates
                    .SingleOrDefault(x => x.Id == _defaultTemplateId.Value));
            }

            contentType.ResetDirtyProperties(false);

            return contentType;
        }

        int? IWithPropertyTypeIdsIncrementingFrom.PropertyTypeIdsIncrementingFrom
        {
            get => _propertyTypeIdsIncrementingFrom;
            set => _propertyTypeIdsIncrementingFrom = value;
        }
    }
}
