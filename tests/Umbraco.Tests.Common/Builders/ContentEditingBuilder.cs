// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;
using Umbraco.Cms.Tests.Common.Extensions;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentEditingBuilder
    : BuilderBase<ContentCreateModel>,
        IBuildContentTypes,
        IBuildContentCultureInfosCollection,
        IWithIdBuilder,
        IWithKeyBuilder,
        IWithCreatorIdBuilder,
        IWithCreateDateBuilder,
        IWithUpdateDateBuilder,
        IWithNameBuilder,
        IWithTrashedBuilder,
        IWithLevelBuilder,
        IWithPathBuilder,
        IWithSortOrderBuilder,
        IWithCultureInfoBuilder,
        IWithPropertyValues,
        IWithParentKeyBuilder
{
    private readonly IDictionary<string, string> _cultureNames = new Dictionary<string, string>();
    private ContentCultureInfosCollection _contentCultureInfosCollection;
    private ContentCultureInfosCollectionBuilder _contentCultureInfosCollectionBuilder;
    private IContentType _contentType;
    private ContentTypeBuilder _contentTypeBuilder;
    private DateTime? _createDate;
    private int? _creatorId;
    private CultureInfo _cultureInfo;

    private int? _id;
    private Guid? _key;
    private int? _level;
    private string _name;
    private IContent _parent;
    private Guid? _parentKey;
    private string _path;
    private GenericDictionaryBuilder<ContentEditingBuilder, string, object> _propertyDataBuilder;
    private object _propertyValues;
    private string _propertyValuesCulture;
    private string _propertyValuesSegment;
    private int? _sortOrder;
    private bool? _trashed;
    private DateTime? _updateDate;
    private int? _versionId;

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

    CultureInfo IWithCultureInfoBuilder.CultureInfo
    {
        get => _cultureInfo;
        set => _cultureInfo = value;
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

    Guid? IWithParentKeyBuilder.ParentKey
    {
        get => _parentKey;
        set => _parentKey = value;
    }

    string IWithPathBuilder.Path
    {
        get => _path;
        set => _path = value;
    }

    object IWithPropertyValues.PropertyValues
    {
        get => _propertyValues;
        set => _propertyValues = value;
    }

    string IWithPropertyValues.PropertyValuesCulture
    {
        get => _propertyValuesCulture;
        set => _propertyValuesCulture = value;
    }

    string IWithPropertyValues.PropertyValuesSegment
    {
        get => _propertyValuesSegment;
        set => _propertyValuesSegment = value;
    }

    int? IWithSortOrderBuilder.SortOrder
    {
        get => _sortOrder;
        set => _sortOrder = value;
    }

    bool? IWithTrashedBuilder.Trashed
    {
        get => _trashed;
        set => _trashed = value;
    }

    DateTime? IWithUpdateDateBuilder.UpdateDate
    {
        get => _updateDate;
        set => _updateDate = value;
    }

    public ContentEditingBuilder WithVersionId(int versionId)
    {
        _versionId = versionId;
        return this;
    }

    public ContentEditingBuilder WithParent(IContent parent)
    {
        _parentKey = null;
        _parent = parent;
        return this;
    }

    public ContentEditingBuilder WithContentType(IContentType contentType)
    {
        _contentTypeBuilder = null;
        _contentType = contentType;
        return this;
    }

    public ContentEditingBuilder WithContentCultureInfosCollection(
        ContentCultureInfosCollection contentCultureInfosCollection)
    {
        _contentCultureInfosCollectionBuilder = null;
        _contentCultureInfosCollection = contentCultureInfosCollection;
        return this;
    }

    public ContentEditingBuilder WithCultureName(string culture, string name = "")
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            if (_cultureNames.TryGetValue(culture, out _))
            {
                _cultureNames.Remove(culture);
            }
        }
        else
        {
            _cultureNames[culture] = name;
        }

        return this;
    }

    public GenericDictionaryBuilder<ContentEditingBuilder, string, object> AddPropertyData()
    {
        var builder = new GenericDictionaryBuilder<ContentEditingBuilder, string, object>(this);
        _propertyDataBuilder = builder;
        return builder;
    }

    // public ContentCultureInfosCollectionBuilder AddContentCultureInfosCollection()
    // {
    //     _contentCultureInfosCollection = null;
    //     var builder = new ContentCultureInfosCollectionBuilder(this);
    //     _contentCultureInfosCollectionBuilder = builder;
    //     return builder;
    // }

    public override ContentCreateModel Build()
    {
        var id = _id ?? 0;
        var versionId = _versionId ?? 0;
        var key = _key ?? Guid.NewGuid();
        var parentKey = _parentKey;
        var parent = _parent;
        var createDate = _createDate ?? DateTime.Now;
        var updateDate = _updateDate ?? DateTime.Now;
        var name = _name ?? Guid.NewGuid().ToString();
        var creatorId = _creatorId ?? 0;
        var level = _level ?? 1;
        var path = _path ?? $"-1,{id}";
        var sortOrder = _sortOrder ?? 0;
        var trashed = _trashed ?? false;
        var culture = _cultureInfo?.Name;
        var propertyValues = _propertyValues;
        var propertyValuesCulture = _propertyValuesCulture;
        var propertyValuesSegment = _propertyValuesSegment;

        if (_contentTypeBuilder is null && _contentType is null)
        {
            throw new InvalidOperationException(
                "A content item cannot be constructed without providing a content type. Use AddContentType() or WithContentType().");
        }

        var contentType = _contentType ?? _contentTypeBuilder.Build();

        var content = new ContentCreateModel();
        content.InvariantName = name;

        // if( parent.Key != Guid.Empty)
        // {
        //     content.ParentKey = parent.Key;
        // }
        content.ContentTypeKey = contentType.Key;
        // content.Variants.First().Name = culture;

        content.Key = key;

        if (contentType.DefaultTemplate?.Key != null)
        {
            content.TemplateKey = contentType.DefaultTemplate.Key;
        }

        return content;
    }

    public static ContentCreateModel CreateBasicContent(IContentType contentType, int id = 0) =>
        new ContentEditingBuilder()
            .WithId(id)
            .WithContentType(contentType)
            .WithName("Home")
            .Build();

    public static ContentCreateModel CreateSimpleContent(IContentType contentType) =>
        new ContentEditingBuilder()
            .WithContentType(contentType)
            .WithName("Home")
            .WithPropertyValues(new
            {
                title = "Welcome to our Home page",
                bodyText = "This is the welcome message on the first page",
                author = "John Doe"
            })
            .Build();

    public static ContentCreateModel CreateSimpleContent(IContentType contentType, string name, Guid? parentKey, string? culture = null, string? segment = null) =>
        new ContentEditingBuilder()
            .WithContentType(contentType)
            .WithName(name)
            .WithParentKey(parentKey)
            .WithPropertyValues(
                new
                {
                    title = "Welcome to our Home page",
                    bodyText = "This is the welcome message on the first page",
                    author = "John Doe"
                },
                culture,
                segment)
            .Build();

    public static ContentCreateModel CreateSimpleContent(IContentType contentType, string name, IContent parent,
        string? culture = null, string? segment = null, bool setPropertyValues = true)
    {
        var builder = new ContentEditingBuilder()
            .WithContentType(contentType)
            .WithName(name)
            .WithParent(parent);

        if (!(culture is null))
        {
            builder = builder.WithCultureName(culture, name);
        }

        if (setPropertyValues)
        {
            builder = builder.WithPropertyValues(
                new { title = name + " Subpage", bodyText = "This is a subpage", author = "John Doe" },
                culture,
                segment);
        }

        var content = builder.Build();

        return content;
    }

    public static ContentCreateModel CreateTextpageContent(IContentType contentType, string name, Guid? parentKey) =>
        new ContentEditingBuilder()
            .WithId(0)
            .WithContentType(contentType)
            .WithName(name)
            .WithParentKey(parentKey)
            .WithPropertyValues(
                new
                {
                    title = name + " textpage",
                    bodyText = string.Format("This is a textpage based on the {0} ContentType", contentType.Alias),
                    keywords = "text,page,meta",
                    description = "This is the meta description for a textpage"
                })
            .Build();
}
