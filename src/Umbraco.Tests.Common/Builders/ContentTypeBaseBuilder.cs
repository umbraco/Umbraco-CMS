// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders
{
    public abstract class ContentTypeBaseBuilder<TParent, TType>
        : ChildBuilderBase<TParent, TType>,
            IBuildPropertyGroups,
            IWithIdBuilder,
            IWithKeyBuilder,
            IWithAliasBuilder,
            IWithNameBuilder,
            IWithParentIdBuilder,
            IWithParentContentTypeBuilder,
            IWithPathBuilder,
            IWithLevelBuilder,
            IWithSortOrderBuilder,
            IWithCreatorIdBuilder,
            IWithCreateDateBuilder,
            IWithUpdateDateBuilder,
            IWithDescriptionBuilder,
            IWithIconBuilder,
            IWithThumbnailBuilder,
            IWithTrashedBuilder,
            IWithIsContainerBuilder
        where TParent : IBuildContentTypes
    {
        private int? _id;
        private Guid? _key;
        private string _alias;
        private string _name;
        private int? _parentId;
        private IContentTypeComposition _parent;
        private int? _level;
        private string _path;
        private int? _sortOrder;
        private int? _creatorId;
        private DateTime? _createDate;
        private DateTime? _updateDate;
        private string _description;
        private string _icon;
        private string _thumbnail;
        private bool? _trashed;
        private bool? _isContainer;

        protected IShortStringHelper ShortStringHelper => new DefaultShortStringHelper(new DefaultShortStringHelperConfig());

        public ContentTypeBaseBuilder(TParent parentBuilder)
            : base(parentBuilder)
        {
        }

        protected int GetId() => _id ?? 0;

        protected Guid GetKey() => _key ?? Guid.NewGuid();

        protected DateTime GetCreateDate() => _createDate ?? DateTime.Now;

        protected DateTime GetUpdateDate() => _updateDate ?? DateTime.Now;

        protected string GetName() => _name ?? Guid.NewGuid().ToString();

        protected string GetAlias() => _alias ?? GetName().ToCamelCase();

        protected int GetParentId() => _parentId ?? -1;

        protected IContentTypeComposition GetParent() => _parent ?? null;

        protected int GetLevel() => _level ?? 0;

        protected string GetPath() => _path ?? _path ?? $"-1,{GetId()}";

        protected int GetSortOrder() => _sortOrder ?? 0;

        protected string GetDescription() => _description ?? string.Empty;

        protected string GetIcon() => _icon ?? "icon-document";

        protected string GetThumbnail() => _thumbnail ?? "folder.png";

        protected int GetCreatorId() => _creatorId ?? 0;

        protected bool GetTrashed() => _trashed ?? false;

        protected bool GetIsContainer() => _isContainer ?? false;

        protected void BuildPropertyGroups(ContentTypeCompositionBase contentType, IEnumerable<PropertyGroup> propertyGroups)
        {
            foreach (PropertyGroup propertyGroup in propertyGroups)
            {
                contentType.PropertyGroups.Add(propertyGroup);
            }
        }

        protected void BuildPropertyTypeIds(ContentTypeCompositionBase contentType, int? propertyTypeIdsIncrementingFrom)
        {
            if (propertyTypeIdsIncrementingFrom.HasValue)
            {
                var i = propertyTypeIdsIncrementingFrom.Value;
                foreach (IPropertyType propertyType in contentType.PropertyTypes)
                {
                    propertyType.Id = ++i;
                }
            }
        }

        public static void EnsureAllIds(ContentTypeCompositionBase contentType, int seedId)
        {
            // Ensure everything has IDs (it will have if builder is used to create the object, but still useful to reset
            // and ensure there are no clashes).
            contentType.Id = seedId;
            var itemid = seedId + 1;
            foreach (PropertyGroup propertyGroup in contentType.PropertyGroups)
            {
                propertyGroup.Id = itemid++;
            }

            foreach (IPropertyType propertyType in contentType.PropertyTypes)
            {
                propertyType.Id = itemid++;
            }
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

        int? IWithParentIdBuilder.ParentId
        {
            get => _parentId;
            set
            {
                _parent = null;
                _parentId = value;
            }
        }

        IContentTypeComposition IWithParentContentTypeBuilder.Parent
        {
            get => _parent;
            set
            {
                _parentId = null;
                _parent = value;
            }
        }

        int? IWithLevelBuilder.Level
        {
            get => _level;
            set => _level = value;
        }

        string IWithPathBuilder.Path
        {
            get => _path;
            set => _path = value;
        }

        int? IWithSortOrderBuilder.SortOrder
        {
            get => _sortOrder;
            set => _sortOrder = value;
        }

        int? IWithCreatorIdBuilder.CreatorId
        {
            get => _creatorId;
            set => _creatorId = value;
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

        string IWithDescriptionBuilder.Description
        {
            get => _description;
            set => _description = value;
        }

        string IWithIconBuilder.Icon
        {
            get => _icon;
            set => _icon = value;
        }

        string IWithThumbnailBuilder.Thumbnail
        {
            get => _thumbnail;
            set => _thumbnail = value;
        }

        bool? IWithTrashedBuilder.Trashed
        {
            get => _trashed;
            set => _trashed = value;
        }

        bool? IWithIsContainerBuilder.IsContainer
        {
            get => _isContainer;
            set => _isContainer = value;
        }
    }
}
