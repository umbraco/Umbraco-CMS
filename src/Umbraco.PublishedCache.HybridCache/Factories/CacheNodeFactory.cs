using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache.Factories;

internal sealed class CacheNodeFactory : ICacheNodeFactory
{
    private readonly IShortStringHelper _shortStringHelper;
    private readonly UrlSegmentProviderCollection _urlSegmentProviders;

    public CacheNodeFactory(IShortStringHelper shortStringHelper, UrlSegmentProviderCollection urlSegmentProviders)
    {
        _shortStringHelper = shortStringHelper;
        _urlSegmentProviders = urlSegmentProviders;
    }

    public ContentCacheNode ToContentCacheNode(IContent content, bool preview)
    {


        ContentData contentData = GetContentData(
            content,
            GetPublishedValue(content, preview),
            GetTemplateId(content, preview),
            content.PublishCultureInfos!.Values.Select(x => x.Culture).ToHashSet());
        return new ContentCacheNode
        {
            Id = content.Id,
            Key = content.Key,
            SortOrder = content.SortOrder,
            CreateDate = content.CreateDate,
            CreatorId = content.CreatorId,
            ContentTypeId = content.ContentTypeId,
            Data = contentData,
            IsDraft = preview,
        };
    }

    private static bool GetPublishedValue(IPublishableContentBase content, bool preview)
    {
        switch (content.PublishedState)
        {
            case PublishedState.Published:
                return preview is false;
            case PublishedState.Publishing:
                return preview is false || content.Published; // The type changes after this operation
            case PublishedState.Unpublished:
            case PublishedState.Unpublishing:
            default:
                return false;
        }
    }

    private static int? GetTemplateId(IContent content, bool preview)
    {
        switch (content.PublishedState)
        {
            case PublishedState.Published:
                return preview ? content.TemplateId : content.PublishTemplateId;
            case PublishedState.Publishing:
            case PublishedState.Unpublished:
            case PublishedState.Unpublishing:
                return content.TemplateId;

            default:
                return null;
        }
    }

    public ContentCacheNode ToContentCacheNode(IMedia media)
    {
        ContentData contentData = GetContentData(media, false, null, new HashSet<string>());
        return new ContentCacheNode
        {
            Id = media.Id,
            Key = media.Key,
            SortOrder = media.SortOrder,
            CreateDate = media.CreateDate,
            CreatorId = media.CreatorId,
            ContentTypeId = media.ContentTypeId,
            Data = contentData,
            IsDraft = false,
        };
    }

    public ContentCacheNode ToContentCacheNode(IElement element, bool preview)
    {
        ContentData contentData = GetContentData(
            element,
            GetPublishedValue(element, preview),
            null,
            element.PublishCultureInfos?.Values.Select(x => x.Culture).ToHashSet() ?? []);
        return new ContentCacheNode
        {
            Id = element.Id,
            Key = element.Key,
            SortOrder = element.SortOrder,
            CreateDate = element.CreateDate,
            CreatorId = element.CreatorId,
            ContentTypeId = element.ContentTypeId,
            Data = contentData,
            IsDraft = false,
        };
    }

    private ContentData GetContentData(IContentBase content, bool published, int? templateId, ISet<string> publishedCultures)
    {
        var propertyData = new Dictionary<string, PropertyData[]>();
        foreach (IProperty prop in content.Properties)
        {
            var pdatas = new List<PropertyData>();
            foreach (IPropertyValue pvalue in prop.Values.OrderBy(x => x.Culture))
            {
                // sanitize - properties should be ok but ... never knows
                if (!prop.PropertyType.SupportsVariation(pvalue.Culture, pvalue.Segment))
                {
                    continue;
                }

                // note: at service level, invariant is 'null', but here invariant becomes 'string.Empty'
                if (published && (string.IsNullOrEmpty(pvalue.Culture) is false && publishedCultures.Contains(pvalue.Culture) is false))
                {
                    continue;
                }

                var value = published
                    ? pvalue.PublishedValue
                    : pvalue.EditedValue;

                if (value != null)
                {
                    pdatas.Add(new PropertyData
                    {
                        Culture = pvalue.Culture ?? string.Empty,
                        Segment = pvalue.Segment ?? string.Empty,
                        Value = value,
                    });
                }
            }

            propertyData[prop.Alias] = pdatas.ToArray();
        }

        var cultureData = new Dictionary<string, CultureVariation>();
        string? urlSegment = null;

        // sanitize - names should be ok but ... never knows
        if (content.ContentType.VariesByCulture())
        {
            var publishableContent = content as IPublishableContentBase;
            ContentCultureInfosCollection? infos = publishableContent is not null
                ? published
                    ? publishableContent.PublishCultureInfos
                    : publishableContent.CultureInfos
                : content.CultureInfos;

            // ReSharper disable once UseDeconstruction
            if (infos is not null)
            {
                foreach (ContentCultureInfos cultureInfo in infos)
                {
                    var cultureIsDraft = !published && publishableContent is not null && publishableContent.IsCultureEdited(cultureInfo.Culture);
                    cultureData[cultureInfo.Culture] = new CultureVariation
                    {
                        Name = cultureInfo.Name,
                        UrlSegment =
                            content.GetUrlSegment(_shortStringHelper, _urlSegmentProviders, cultureInfo.Culture),
                        Date = content.GetUpdateDate(cultureInfo.Culture) ?? DateTime.MinValue,
                        IsDraft = cultureIsDraft,
                    };
                }
            }
        }
        else
        {
            urlSegment = content.GetUrlSegment(_shortStringHelper, _urlSegmentProviders);
        }

        return new ContentData(
            content.Name,
            urlSegment,
            content.VersionId,
            content.UpdateDate,
            content.CreatorId,
            templateId,
            published,
            propertyData,
            cultureData);
    }
}
