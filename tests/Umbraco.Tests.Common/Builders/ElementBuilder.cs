// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;
using Umbraco.Cms.Tests.Common.Extensions;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ElementBuilder
    : BuilderBase<Element>,
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
        IWithCultureInfoBuilder,
        IWithPropertyValues
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
    private GenericDictionaryBuilder<ElementBuilder, string, object> _propertyDataBuilder;
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

    public ElementBuilder WithVersionId(int versionId)
    {
        _versionId = versionId;
        return this;
    }

    public ElementBuilder WithContentType(IContentType contentType)
    {
        _contentTypeBuilder = null;
        _contentType = contentType;
        return this;
    }

    public ElementBuilder WithContentCultureInfosCollection(
        ContentCultureInfosCollection contentCultureInfosCollection)
    {
        _contentCultureInfosCollectionBuilder = null;
        _contentCultureInfosCollection = contentCultureInfosCollection;
        return this;
    }

    public ElementBuilder WithCultureName(string culture, string name = "")
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

    public ContentTypeBuilder AddContentType()
    {
        _contentType = null;
        var builder = new ContentTypeBuilder(this);
        _contentTypeBuilder = builder;
        return builder;
    }

    public GenericDictionaryBuilder<ElementBuilder, string, object> AddPropertyData()
    {
        var builder = new GenericDictionaryBuilder<ElementBuilder, string, object>(this);
        _propertyDataBuilder = builder;
        return builder;
    }

    public ContentCultureInfosCollectionBuilder AddContentCultureInfosCollection()
    {
        _contentCultureInfosCollection = null;
        var builder = new ContentCultureInfosCollectionBuilder(this);
        _contentCultureInfosCollectionBuilder = builder;
        return builder;
    }

    public override Element Build()
    {
        var id = _id ?? 0;
        var versionId = _versionId ?? 0;
        var key = _key ?? Guid.NewGuid();
        var createDate = _createDate ?? DateTime.Now;
        var updateDate = _updateDate ?? DateTime.Now;
        var name = _name ?? Guid.NewGuid().ToString();
        var creatorId = _creatorId ?? 0;
        var level = _level ?? 1;
        var path = $"-1,{id}";
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

        var element = new Element(name, contentType, culture)
        {
            Id = id, VersionId = versionId, Key = key, CreateDate = createDate,
            UpdateDate = updateDate,
            CreatorId = creatorId,
            Level = level,
            Path = path,
            SortOrder = sortOrder,
            Trashed = trashed
        };

        foreach (var cultureName in _cultureNames)
        {
            element.SetCultureName(cultureName.Value, cultureName.Key);
        }

        if (_propertyDataBuilder != null || propertyValues != null)
        {
            if (_propertyDataBuilder != null)
            {
                var propertyData = _propertyDataBuilder.Build();
                foreach (var keyValuePair in propertyData)
                {
                    element.SetValue(keyValuePair.Key, keyValuePair.Value);
                }
            }
            else
            {
                element.PropertyValues(propertyValues, propertyValuesCulture, propertyValuesSegment);
            }

            element.ResetDirtyProperties(false);
        }

        if (_contentCultureInfosCollection is not null || _contentCultureInfosCollectionBuilder is not null)
        {
            var contentCultureInfos =
                _contentCultureInfosCollection ?? _contentCultureInfosCollectionBuilder.Build();
            element.PublishCultureInfos = contentCultureInfos;
        }

        return element;
    }

    public static Element CreateBasicElement(IContentType contentType, int id = 0) =>
        new ElementBuilder()
            .WithId(id)
            .WithContentType(contentType)
            .WithName("Element")
            .Build();

    public static Element CreateSimpleElement(IContentType contentType, string name = "Element", string? culture = null,
        string? segment = null)
        => new ElementBuilder()
            .WithContentType(contentType)
            .WithName(name)
            .WithPropertyValues(
                new
                {
                    title = "This is the element title",
                    bodyText = "This is the element body text",
                    author = "Some One"
                },
                culture,
                segment)
            .Build();

}
