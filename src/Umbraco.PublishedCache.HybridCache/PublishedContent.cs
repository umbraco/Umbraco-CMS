using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache;

internal class PublishedContent : PublishedElement, IPublishedContent
{
    public PublishedContent(
        ContentNode contentNode,
        bool preview,
        IElementsCache elementsCache,
        IVariationContextAccessor variationContextAccessor)
        : base(contentNode, preview, elementsCache, variationContextAccessor)
    {
    }

    [Obsolete]
    public string Path
    {
        get
        {
            IDocumentNavigationQueryService documentNavigationQueryService = StaticServiceProvider.Instance.GetRequiredService<IDocumentNavigationQueryService>();
            IIdKeyMap idKeyMap = StaticServiceProvider.Instance.GetRequiredService<IIdKeyMap>();


            if (documentNavigationQueryService.TryGetAncestorsOrSelfKeys(Key, out var ancestorsOrSelfKeys))
            {
                var sb = new StringBuilder("-1");
                foreach (Guid ancestorsOrSelfKey in ancestorsOrSelfKeys.Reverse())
                {
                    Attempt<int> idAttempt = idKeyMap.GetIdForKey(ancestorsOrSelfKey, GetObjectType());
                    if (idAttempt.Success)
                    {
                        sb.AppendFormat(",{0}", idAttempt.Result);
                    }
                }

                return sb.ToString();
            }

            return string.Empty;
        }
    }

    private UmbracoObjectTypes GetObjectType()
    {
        switch (ItemType)
        {
            case PublishedItemType.Content:
                return UmbracoObjectTypes.Document;
            case PublishedItemType.Media:
                return UmbracoObjectTypes.Media;
            case PublishedItemType.Member:
                return UmbracoObjectTypes.Member;
            default:
                return UmbracoObjectTypes.Unknown;
        }
    }

    public int? TemplateId => ContentData.TemplateId;

    [Obsolete("Use the INavigationQueryService instead. Scheduled for removal in Umbraco 18.")]
    public int Level
    {
        get
        {
            INavigationQueryService? navigationQueryService;
            switch (ContentNode.ContentType.ItemType)
            {
                case PublishedItemType.Content:
                    navigationQueryService = StaticServiceProvider.Instance.GetRequiredService<IDocumentNavigationQueryService>();
                    break;
                case PublishedItemType.Media:
                    navigationQueryService = StaticServiceProvider.Instance.GetRequiredService<IMediaNavigationQueryService>();
                    break;
                default:
                    throw new NotImplementedException("Level is not implemented for " + ContentNode.ContentType.ItemType);
            }

            // Attempt to retrieve the level, returning 0 if it fails or if level is null.
            if (navigationQueryService.TryGetLevel(Key, out var level) && level.HasValue)
            {
                return level.Value;
            }

            return 0;
        }
    }

    /// <inheritdoc />
    [Obsolete("Please use GetUrlSegment() on IDocumentUrlService instead. Scheduled for removal in V16.")]
    public virtual string? UrlSegment => this.UrlSegment(VariationContextAccessor);
}
