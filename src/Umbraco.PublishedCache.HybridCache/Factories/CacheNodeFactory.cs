using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache.Factories;

internal class CacheNodeFactory : ICacheNodeFactory
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
        ContentData contentData = GetContentData(content, !preview, preview ? content.PublishTemplateId : content.TemplateId);
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

    public ContentCacheNode ToContentCacheNode(IMedia media)
    {
        ContentData contentData = GetContentData(media, false, null);
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

    private ContentData GetContentData(IContentBase content, bool published, int? templateId)
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
                var value = published ? pvalue.PublishedValue : pvalue.EditedValue;
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

        // sanitize - names should be ok but ... never knows
        if (content.ContentType.VariesByCulture())
        {
            ContentCultureInfosCollection? infos = content is IContent document
                ? published
                    ? document.PublishCultureInfos
                    : document.CultureInfos
                : content.CultureInfos;

            // ReSharper disable once UseDeconstruction
            if (infos is not null)
            {
                foreach (ContentCultureInfos cultureInfo in infos)
                {
                    var cultureIsDraft = !published && content is IContent d && d.IsCultureEdited(cultureInfo.Culture);
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

        return new ContentData(
            content.Name,
            null,
            content.VersionId,
            content.UpdateDate,
            content.CreatorId,
            templateId,
            published,
            propertyData,
            cultureData);
    }
}
