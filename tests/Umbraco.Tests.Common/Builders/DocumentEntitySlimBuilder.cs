// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class DocumentEntitySlimBuilder
    : BuilderBase<DocumentEntitySlim>,
        IWithIdBuilder,
        IWithKeyBuilder,
        IWithCreatorIdBuilder,
        IWithCreateDateBuilder,
        IWithUpdateDateBuilder,
        IWithNameBuilder,
        IWithLevelBuilder,
        IWithPathBuilder,
        IWithSortOrderBuilder,
        IWithParentIdBuilder
{
    private GenericDictionaryBuilder<DocumentEntitySlimBuilder, string, object> _additionalDataBuilder;
    private string _contentTypeAlias;
    private string _contentTypeIcon;
    private string _contentTypeThumbnail;
    private DateTime? _createDate;
    private int? _creatorId;
    private bool? _hasChildren;

    private int? _id;
    private Guid? _key;
    private int? _level;
    private string _name;
    private int? _parentId;
    private string _path;
    private bool? _published;
    private int? _sortOrder;
    private DateTime? _updateDate;

    DateTime? IWithCreateDateBuilder.CreateDate
    {
        get => _createDate;
        set => _createDate = value;
    }

    int? IWithCreatorIdBuilder.CreatorId
    {
        get => _creatorId;
        set => _creatorId = value;
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

    int? IWithLevelBuilder.Level
    {
        get => _level;
        set => _level = value;
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

    DateTime? IWithUpdateDateBuilder.UpdateDate
    {
        get => _updateDate;
        set => _updateDate = value;
    }

    public DocumentEntitySlimBuilder WithHasChildren(bool hasChildren)
    {
        _hasChildren = hasChildren;
        return this;
    }

    public DocumentEntitySlimBuilder WithPublished(bool published)
    {
        _published = published;
        return this;
    }

    public DocumentEntitySlimBuilder WithContentTypeAlias(string contentTypeAlias)
    {
        _contentTypeAlias = contentTypeAlias;
        return this;
    }

    public DocumentEntitySlimBuilder WithContentTypeIcon(string contentTypeIcon)
    {
        _contentTypeIcon = contentTypeIcon;
        return this;
    }

    public DocumentEntitySlimBuilder WithContentTypeThumbnail(string contentTypeThumbnail)
    {
        _contentTypeThumbnail = contentTypeThumbnail;
        return this;
    }

    public GenericDictionaryBuilder<DocumentEntitySlimBuilder, string, object> AddAdditionalData()
    {
        var builder = new GenericDictionaryBuilder<DocumentEntitySlimBuilder, string, object>(this);
        _additionalDataBuilder = builder;
        return builder;
    }

    public override DocumentEntitySlim Build()
    {
        var id = _id ?? 1;
        var key = _key ?? Guid.NewGuid();
        var createDate = _createDate ?? DateTime.Now;
        var updateDate = _updateDate ?? DateTime.Now;
        var name = _name ?? Guid.NewGuid().ToString();
        var creatorId = _creatorId ?? 1;
        var level = _level ?? 1;
        var path = _path ?? $"-1,{id}";
        var sortOrder = _sortOrder ?? 0;
        var parentId = _parentId ?? -1;
        var icon = _contentTypeIcon ?? string.Empty;
        var thumbnail = _contentTypeThumbnail ?? string.Empty;
        var contentTypeAlias = _contentTypeAlias ?? string.Empty;
        var hasChildren = _hasChildren ?? false;
        var published = _published ?? false;

        var documentEntitySlim = new DocumentEntitySlim
        {
            Id = id,
            Key = key,
            CreateDate = createDate,
            UpdateDate = updateDate,
            Name = name,
            CreatorId = creatorId,
            Level = level,
            Path = path,
            SortOrder = sortOrder,
            ParentId = parentId,
            ContentTypeIcon = icon,
            ContentTypeThumbnail = thumbnail,
            ContentTypeAlias = contentTypeAlias,
            HasChildren = hasChildren,
            Published = published
        };

        if (_additionalDataBuilder != null)
        {
            var additionalData = _additionalDataBuilder.Build();
            foreach (var kvp in additionalData)
            {
                documentEntitySlim.AdditionalData.Add(kvp.Key, kvp.Value);
            }
        }

        return documentEntitySlim;
    }
}
