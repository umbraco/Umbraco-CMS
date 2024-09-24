using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Delivery.Indexing.Selectors;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Querying.Selectors;

public sealed class AncestorsSelector : QueryOptionBase, ISelectorHandler
{
    private readonly IPublishedContentCache _publishedContentCache;
    private readonly IDocumentNavigationQueryService _navigationQueryService;
    private const string AncestorsSpecifier = "ancestors:";

    public AncestorsSelector(
        IPublishedContentCache publishedContentCache,
        IRequestRoutingService requestRoutingService,
        IDocumentNavigationQueryService navigationQueryService)
        : base(publishedContentCache, requestRoutingService)
    {
        _publishedContentCache = publishedContentCache;
        _navigationQueryService = navigationQueryService;
    }


    [Obsolete("Use the constructor that takes all parameters. Scheduled for removal in V17")]
    public AncestorsSelector(IPublishedContentCache publishedContentCache, IRequestRoutingService requestRoutingService)
        : this(publishedContentCache, requestRoutingService, StaticServiceProvider.Instance.GetRequiredService<IDocumentNavigationQueryService>())
    {
    }

    /// <inheritdoc />
    public bool CanHandle(string query)
        => query.StartsWith(AncestorsSpecifier, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public SelectorOption BuildSelectorOption(string selector)
    {
        var fieldValue = selector[AncestorsSpecifier.Length..];
        Guid? id = GetGuidFromQuery(fieldValue);

        if (id is null)
        {
            // Setting the Value to "" since that would yield no results.
            // It won't be appropriate to return null here since if we reached this,
            // it means that CanHandle() returned true, meaning that this Selector should be able to handle the selector value
            return new SelectorOption
            {
                FieldName = AncestorsSelectorIndexer.FieldName,
                Values = Array.Empty<string>()
            };
        }

        IPublishedContent contentItem = _publishedContentCache.GetById((Guid)id)
                                        ?? throw new InvalidOperationException("Could not obtain the content cache");

        var ancestorKeys = contentItem.Ancestors(_publishedContentCache, _navigationQueryService).Select(a => a.Key.ToString("D")).ToArray();

        return new SelectorOption
        {
            FieldName = AncestorsSelectorIndexer.FieldName,
            Values = ancestorKeys
        };
    }
}
