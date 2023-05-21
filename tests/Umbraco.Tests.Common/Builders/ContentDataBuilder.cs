using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.PublishedCache.DataSource;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentDataBuilder : BuilderBase<ContentData>, IWithNameBuilder
{
    private Dictionary<string, CultureVariation> _cultureInfos;
    private string _name;
    private DateTime? _now;
    private Dictionary<string, PropertyData[]> _properties;
    private bool? _published;
    private string _segment;
    private int? _templateId;
    private int? _versionId;
    private int? _writerId;

    string IWithNameBuilder.Name
    {
        get => _name;
        set => _name = value;
    }

    public ContentDataBuilder WithVersionDate(DateTime now)
    {
        _now = now;
        return this;
    }

    public ContentDataBuilder WithUrlSegment(string segment)
    {
        _segment = segment;
        return this;
    }

    public ContentDataBuilder WithVersionId(int versionId)
    {
        _versionId = versionId;
        return this;
    }

    public ContentDataBuilder WithWriterId(int writerId)
    {
        _writerId = writerId;
        return this;
    }

    public ContentDataBuilder WithTemplateId(int templateId)
    {
        _templateId = templateId;
        return this;
    }

    public ContentDataBuilder WithPublished(bool published)
    {
        _published = published;
        return this;
    }

    public ContentDataBuilder WithProperties(Dictionary<string, PropertyData[]> properties)
    {
        _properties = properties;
        return this;
    }

    public ContentDataBuilder WithCultureInfos(Dictionary<string, CultureVariation> cultureInfos)
    {
        _cultureInfos = cultureInfos;
        return this;
    }

    /// <summary>
    ///     Build and dynamically update an existing content type
    /// </summary>
    /// <typeparam name="TContentType"></typeparam>
    /// <param name="shortStringHelper"></param>
    /// <param name="propertyDataTypes"></param>
    /// <param name="contentType"></param>
    /// <param name="contentTypeAlias">
    ///     Will configure the content type with this alias/name if supplied when it's not already set on the content type.
    /// </param>
    /// <param name="autoCreateCultureNames"></param>
    /// <returns></returns>
    public ContentData Build<TContentType>(
        IShortStringHelper shortStringHelper,
        Dictionary<string, IDataType> propertyDataTypes,
        TContentType contentType,
        string contentTypeAlias = null,
        bool autoCreateCultureNames = false) where TContentType : class, IContentTypeComposition
    {
        if (_name.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException("Cannot build without a name");
        }

        _segment ??= _name.ToLower().ReplaceNonAlphanumericChars('-');

        // create or copy the current culture infos for the content
        var contentCultureInfos = _cultureInfos == null
            ? new Dictionary<string, CultureVariation>()
            : new Dictionary<string, CultureVariation>(_cultureInfos);

        contentType.Alias ??= contentTypeAlias;
        contentType.Name ??= contentTypeAlias;
        contentType.Key = contentType.Key == default ? Guid.NewGuid() : contentType.Key;
        contentType.Id = contentType.Id == default ? Math.Abs(contentTypeAlias.GetHashCode()) : contentType.Id;

        if (_properties == null)
        {
            _properties = new Dictionary<string, PropertyData[]>();
        }

        foreach (var prop in _properties)
        {
            //var dataType = new DataType(new VoidEditor("Label", Mock.Of<IDataValueEditorFactory>()), new ConfigurationEditorJsonSerializer())
            //{
            //    Id = 4
            //};

            if (!propertyDataTypes.TryGetValue(prop.Key, out var dataType))
            {
                dataType = propertyDataTypes.First().Value;
            }

            var propertyType = new PropertyType(shortStringHelper, dataType, prop.Key);

            // check each property for culture and set variations accordingly,
            // this will also ensure that we have the correct culture name on the content
            // set for each culture too.
            foreach (var cultureValue in prop.Value.Where(x => !x.Culture.IsNullOrWhiteSpace()))
            {
                // set the property type to vary based on the values
                propertyType.Variations |= ContentVariation.Culture;

                // if there isn't already a culture, then add one with the default name
                if (autoCreateCultureNames &&
                    !contentCultureInfos.TryGetValue(cultureValue.Culture, out var cultureVariation))
                {
                    cultureVariation = new CultureVariation
                    {
                        Date = DateTime.Now,
                        IsDraft = true,
                        Name = _name,
                        UrlSegment = _segment
                    };
                    contentCultureInfos[cultureValue.Culture] = cultureVariation;
                }
            }

            // set variations for segments if there is any
            if (prop.Value.Any(x => !x.Segment.IsNullOrWhiteSpace()))
            {
                propertyType.Variations |= ContentVariation.Segment;
                contentType.Variations |= ContentVariation.Segment;
            }

            if (!contentType.PropertyTypeExists(propertyType.Alias))
            {
                contentType.AddPropertyType(propertyType);
            }
        }

        if (contentCultureInfos.Count > 0)
        {
            contentType.Variations |= ContentVariation.Culture;
            WithCultureInfos(contentCultureInfos);
        }

        var result = Build();
        return result;
    }

    public override ContentData Build()
    {
        var now = _now ?? DateTime.Now;
        var versionId = _versionId ?? 1;
        var writerId = _writerId ?? -1;
        var templateId = _templateId ?? 0;
        var published = _published ?? true;
        var properties = _properties ?? new Dictionary<string, PropertyData[]>();
        var cultureInfos = _cultureInfos ?? new Dictionary<string, CultureVariation>();
        var segment = _segment ?? _name.ToLower().ReplaceNonAlphanumericChars('-');

        var contentData = new ContentData(
            _name,
            segment,
            versionId,
            now,
            writerId,
            templateId,
            published,
            properties,
            cultureInfos);

        return contentData;
    }

    public static ContentData CreateBasic(string name, DateTime? versionDate = null)
        => new ContentDataBuilder()
            .WithName(name)
            .WithVersionDate(versionDate ?? DateTime.Now)
            .Build();

    public static ContentData CreateVariant(string name, Dictionary<string, CultureVariation> cultureInfos, DateTime? versionDate = null, bool published = true)
        => new ContentDataBuilder()
            .WithName(name)
            .WithVersionDate(versionDate ?? DateTime.Now)
            .WithCultureInfos(cultureInfos)
            .WithPublished(published)
            .Build();
}
