// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;
using Umbraco.Cms.Tests.Common.Builders.Interfaces.ContentCreateModel;
using Umbraco.Cms.Tests.Common.Extensions;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentEditingBuilder
    : BuilderBase<ContentCreateModel>,
        IWithInvariantNameBuilder,
        IWithInvariantPropertiesBuilder,
        IWithVariantsBuilder,
        IWithKeyBuilder,
        IWithContentTypeKeyBuilder,
        IWithParentKeyBuilder,
        IWithTemplateKeyBuilder
{
    private readonly IDictionary<string, string> _cultureNames = new Dictionary<string, string>();
    private IContentType _contentType;
    private ContentTypeBuilder _contentTypeBuilder;
    private DateTime? _createDate;
    private int? _creatorId;
    private CultureInfo _cultureInfo;
    private IEnumerable<PropertyValueModel> _invariantProperties;
    private IEnumerable<VariantModel> _variants;
    private Guid _contentTypeKey;
    private Guid? _parentKey;
    private Guid? _templateKey;

    private int? _id;
    private Guid? _key;
    private int? _level;
    private string _name;
    private string _invariantName;
    private IContent _parent;
    private string _path;
    private GenericDictionaryBuilder<ContentEditingBuilder, string, object> _propertyDataBuilder;
    private object _propertyValues;
    private string _propertyValuesCulture;
    private string _propertyValuesSegment;
    private int? _sortOrder;
    private bool? _trashed;
    private DateTime? _updateDate;
    private int? _versionId;
    private IWithContentTypeKeyBuilder _withContentTypeKeyBuilderImplementation;


    Guid? IWithKeyBuilder.Key
    {
        get => _key;
        set => _key = value;
    }

    string IWithInvariantNameBuilder.InvariantName
    {
        get => _invariantName;
        set => _invariantName = value;
    }

    IEnumerable<PropertyValueModel> IWithInvariantPropertiesBuilder.InvariantProperties
    {
        get => _invariantProperties;
        set => _invariantProperties = value;
    }

    IEnumerable<VariantModel> IWithVariantsBuilder.Variants
    {
        get => _variants;
        set => _variants = value;
    }

    Guid? IWithParentKeyBuilder.ParentKey
    {
        get => _parentKey;
        set => _parentKey = value;
    }

    Guid IWithContentTypeKeyBuilder.ContentTypeKey
    {
        get => _contentTypeKey;
        set => _contentTypeKey = value;
    }

    Guid? IWithTemplateKeyBuilder.TemplateKey
    {
        get => _templateKey;
        set => _templateKey = value;
    }

    public ContentEditingBuilder WithInvariantName(string name)
    {
        _name = name;
        return this;
    }

    public ContentEditingBuilder WithInvariantProperty(string alias, object value)
    {
        var property = new PropertyValueModel { Alias = alias, Value = value };

        // Ensure _invariantProperties is always treated as IEnumerable and append the new property
        _invariantProperties = (_invariantProperties ?? Enumerable.Empty<PropertyValueModel>())
            .Concat(new[] { property });

        return this;
    }

    // public ContentEditingBuilder AddVariant(string culture, string segment, string name)
    // {
    //     if (_variants is null)
    //     {
    //         _variants = new List<VariantModel>();
    //     }
    //
    //     var variant = new VariantModel { Culture = culture, Segment = segment, Name = name };
    //
    //     (_variants as List<VariantModel>).Add(variant);
    //
    //     return this;
    // }


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

    public override ContentCreateModel Build()
    {
        var key = _key ?? Guid.NewGuid();
        var parentKey = _parentKey;
        // var name = _name ?? Guid.NewGuid().ToString();
        var invariantName = _invariantName ?? Guid.NewGuid().ToString();
        // var creatorId = _creatorId ?? 0;
        // var level = _level ?? 1;
        // var sortOrder = _sortOrder ?? 0;
        // var trashed = _trashed ?? false;
        var invariantProperties = _invariantProperties;
        var variants = _variants;
        // var propertyValuesCulture = _propertyValuesCulture;
        // var propertyValuesSegment = _propertyValuesSegment;

        if (_contentTypeBuilder is null && _contentType is null)
        {
            throw new InvalidOperationException(
                "A content item cannot be constructed without providing a content type. Use AddContentType() or WithContentType().");
        }

        var contentType = _contentType ?? _contentTypeBuilder.Build();

        var content = new ContentCreateModel();
        content.InvariantName = invariantName;

        if( parentKey != Guid.Empty)
        {
            content.ParentKey = parentKey;
        }
        content.ContentTypeKey = contentType.Key;



        content.Key = key;
        // content.Key = null;

        content.InvariantProperties = invariantProperties;

        content.Variants = [];

        // if (contentType.DefaultTemplate?.Key != null)
        // {
        //     content.TemplateKey = contentType.DefaultTemplate.Key;
        // }

        return content;
    }

    public static ContentCreateModel CreateBasicContent(IContentType contentType, Guid? key) =>
        new ContentEditingBuilder()
            // .WithKey(key)
            .WithContentType(contentType)
            .WithInvariantName("Home")
            .Build();

    public static ContentCreateModel CreateSimpleContent(IContentType contentType) =>
        new ContentEditingBuilder()
            .WithContentType(contentType)
            .WithInvariantName("Home")
            .WithInvariantProperty("title", "Welcome to our Home page")
            // .WithParentKey(Constants.System.RootKey)
            .Build();

    public static ContentCreateModel CreateSimpleContent(IContentType contentType, string name, Guid? parentKey,
        string? culture = null, string? segment = null) =>
        new ContentEditingBuilder()
            .WithContentType(contentType)
            .WithInvariantName(name)
            .WithParentKey(parentKey)
            .WithInvariantProperty("title", "Welcome to our Home page")

            .Build();


//     public static ContentCreateModel CreateSimpleContent(IContentType contentType, string name, IContent parent,
//         string? culture = null, string? segment = null, bool setPropertyValues = true)
//     {
//         var builder = new ContentEditingBuilder()
//             .WithContentType(contentType)
//             .WithName(name)
//             .WithParent(parent);
//
//         if (!(culture is null))
//         {
//             builder = builder.WithCultureName(culture, name);
//         }
//
//         if (setPropertyValues)
//         {
//             builder = builder.WithPropertyValues(
//                 new { title = name + " Subpage", bodyText = "This is a subpage", author = "John Doe" },
//                 culture,
//                 segment);
//         }
//
//         var content = builder.Build();
//
//         return content;
//     }
//
//     public static ContentCreateModel CreateTextpageContent(IContentType contentType, string name, Guid? parentKey) =>
//         new ContentEditingBuilder()
//             .WithId(0)
//             .WithContentType(contentType)
//             .WithName(name)
//             .WithParentKey(parentKey)
//             .WithPropertyValues(
//                 new
//                 {
//                     title = name + " textpage",
//                     bodyText = string.Format("This is a textpage based on the {0} ContentType", contentType.Alias),
//                     keywords = "text,page,meta",
//                     description = "This is the meta description for a textpage"
//                 })
//             .Build();

 }
