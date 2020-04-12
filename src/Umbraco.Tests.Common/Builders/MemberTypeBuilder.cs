using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Strings;
using Umbraco.Tests.Common.Builders.Extensions;
using Umbraco.Tests.Common.Builders.Interfaces;

namespace Umbraco.Tests.Common.Builders
{
    public class MemberTypeBuilder
        : ChildBuilderBase<MemberBuilder, IMemberType>,
            IBuildPropertyGroups,
            IWithIdBuilder,
            IWithAliasBuilder,
            IWithNameBuilder,
            IWithParentIdBuilder,
            IWithSortOrderBuilder,
            IWithCreatorIdBuilder,
            IWithDescriptionBuilder,
            IWithIconBuilder,
            IWithThumbnailBuilder,            
            IWithTrashedBuilder
    {
        private readonly List<PropertyGroupBuilder<MemberTypeBuilder>> _propertyGroupBuilders = new List<PropertyGroupBuilder<MemberTypeBuilder>>();

        private int? _id;
        private string _alias;
        private string _name;
        private int? _parentId;
        private int? _sortOrder;
        private int? _creatorId;
        private string _description;
        private string _icon;
        private string _thumbnail;
        private bool? _trashed;

        public MemberTypeBuilder(MemberBuilder parentBuilder) : base(parentBuilder)
        {
        }

        public MemberTypeBuilder WithMembershipPropertyGroup()
        {
            var builder = new PropertyGroupBuilder<MemberTypeBuilder>(this)
                .WithId(99)
                .WithName(Constants.Conventions.Member.StandardPropertiesGroupName)
                .WithSortOrder(1)
                .AddPropertyType()
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextArea)
                    .WithValueStorageType(ValueStorageType.Ntext)
                    .WithAlias(Constants.Conventions.Member.Comments)
                    .WithName(Constants.Conventions.Member.CommentsLabel)
                    .Done()
                .AddPropertyType()
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Boolean)
                    .WithValueStorageType(ValueStorageType.Integer)
                    .WithAlias(Constants.Conventions.Member.IsApproved)
                    .WithName(Constants.Conventions.Member.IsApprovedLabel)
                    .Done()
                .AddPropertyType()
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Boolean)
                    .WithValueStorageType(ValueStorageType.Integer)
                    .WithAlias(Constants.Conventions.Member.IsLockedOut)
                    .WithName(Constants.Conventions.Member.IsLockedOutLabel)
                    .Done()
                .AddPropertyType()
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Label)
                    .WithValueStorageType(ValueStorageType.Date)
                    .WithAlias(Constants.Conventions.Member.LastLoginDate)
                    .WithName(Constants.Conventions.Member.LastLoginDateLabel)
                    .Done()
                .AddPropertyType()
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Label)
                    .WithValueStorageType(ValueStorageType.Date)
                    .WithAlias(Constants.Conventions.Member.LastPasswordChangeDate)
                    .WithName(Constants.Conventions.Member.LastPasswordChangeDateLabel)
                    .Done()
                .AddPropertyType()
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Label)
                    .WithValueStorageType(ValueStorageType.Date)
                    .WithAlias(Constants.Conventions.Member.LastLockoutDate)
                    .WithName(Constants.Conventions.Member.LastLockoutDateLabel)
                    .Done()
                .AddPropertyType()
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Label)
                    .WithValueStorageType(ValueStorageType.Integer)
                    .WithAlias(Constants.Conventions.Member.FailedPasswordAttempts)
                    .WithName(Constants.Conventions.Member.FailedPasswordAttemptsLabel)
                    .Done();
            _propertyGroupBuilders.Add(builder);
            return this;
        }

        public PropertyGroupBuilder<MemberTypeBuilder> AddPropertyGroup()
        {
            var builder = new PropertyGroupBuilder<MemberTypeBuilder>(this);
            _propertyGroupBuilders.Add(builder);
            return builder;
        }

        public override IMemberType Build()
        {
            var id = _id ?? 1;
            var name = _name ?? Guid.NewGuid().ToString();
            var alias = _alias ?? name.ToCamelCase();
            var parentId = _parentId ?? -1;
            var sortOrder = _sortOrder ?? 0;
            var description = _description ?? string.Empty;
            var icon = _icon ?? string.Empty;
            var thumbnail = _thumbnail ?? string.Empty;
            var creatorId = _creatorId ?? 0;
            var trashed = _trashed ?? false;

            var shortStringHelper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig());

            var memberType = new MemberType(shortStringHelper, parentId)
            {
                Id = id,
                Alias = alias,
                Name = name,
                SortOrder = sortOrder,
                Description = description,
                Icon = icon,
                Thumbnail = thumbnail,
                CreatorId = creatorId,
                Trashed = trashed,
            };

            foreach (var propertyGroup in _propertyGroupBuilders.Select(x => x.Build()))
            {
                memberType.PropertyGroups.Add(propertyGroup);
            }

            memberType.ResetDirtyProperties(false);

            Reset();
            return memberType;
        }

        protected override void Reset()
        {
            _id = null;
            _alias = null;
            _name = null;
            _parentId = null;
            _sortOrder = null;
            _creatorId = null;
            _description = null;
            _icon = null;
            _thumbnail = null;
            _trashed = null;
            _propertyGroupBuilders.Clear();
        }

        int? IWithIdBuilder.Id
        {
            get => _id;
            set => _id = value;
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
            set => _parentId = value;
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
    }
}
