using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Services;

/// <inheritdoc />
/// <remarks>
/// This service has been built to mimic <see cref="ApiContentQueryService"/> with future extension in mind.
/// At this time, "fetch=childrenOf:" option is the only supported query option, so it's been hardcoded. In the
/// future we might implement fetch/filters/sorts as seen in the content equivalent.
/// </remarks>
public class ApiMediaQueryService : IApiMediaQueryService
{
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly ILogger<ApiMediaQueryService> _logger;

    public ApiMediaQueryService(IPublishedSnapshotAccessor publishedSnapshotAccessor, ILogger<ApiMediaQueryService> logger)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Attempt<PagedModel<Guid>, ApiMediaQueryOperationStatus> ExecuteQuery(string? fetch, IEnumerable<string> filters, IEnumerable<string> sorts, int skip, int take)
    {
        var emptyResult = new PagedModel<Guid>();
        const string childrenOfParameter = "children:";

        var childrenOf = string.Empty;
        if (fetch?.StartsWith(childrenOfParameter) is true)
        {
            childrenOf = fetch.TrimStart(childrenOfParameter);
        }
        if (childrenOf.IsNullOrWhiteSpace())
        {
            _logger.LogInformation($"The current implementation of {nameof(IApiMediaQueryService)} expects \"{childrenOfParameter}[id/path]\" in the \"{nameof(fetch)}\" query option");
            return Attempt.FailWithStatus(ApiMediaQueryOperationStatus.SelectorOptionNotFound, emptyResult);
        }

        if (filters.Any(filter => filter.IsNullOrWhiteSpace() is false))
        {
            _logger.LogInformation($"The current implementation of {nameof(IApiMediaQueryService)} does not support the \"{nameof(filters)}\" query option");
            return Attempt.FailWithStatus(ApiMediaQueryOperationStatus.FilterOptionNotFound, emptyResult);
        }

        if (sorts.Any(sort => sort.IsNullOrWhiteSpace() is false))
        {
            _logger.LogInformation($"The current implementation of {nameof(IApiMediaQueryService)} does not support the \"{nameof(sorts)}\" query option");
            return Attempt.FailWithStatus(ApiMediaQueryOperationStatus.SortOptionNotFound, emptyResult);
        }

        IPublishedMediaCache mediaCache = GetRequiredPublishedMediaCache();

        if (childrenOf.Trim(Constants.CharArrays.ForwardSlash).Length == 0)
        {
            return PagedResult(mediaCache.GetAtRoot(), skip, take);
        }

        IPublishedContent? startItem;
        if (Guid.TryParse(childrenOf, out Guid startItemKey))
        {
            startItem = mediaCache.GetById(startItemKey);
        }
        else
        {
            startItem = TryGetByPath(childrenOf, mediaCache);
        }

        return PagedResult(startItem?.Children ?? Array.Empty<IPublishedContent>(), skip, take);
    }

    /// <inheritdoc/>
    public IPublishedContent? GetByPath(string path)
        => TryGetByPath(path, GetRequiredPublishedMediaCache());

    private IPublishedMediaCache GetRequiredPublishedMediaCache()
        => _publishedSnapshotAccessor.GetRequiredPublishedSnapshot().Media
           ?? throw new InvalidOperationException("Could not obtain the published media cache");

    private IPublishedContent? TryGetByPath(string path, IPublishedMediaCache mediaCache)
    {
        var segments = path.Split(Constants.CharArrays.ForwardSlash, StringSplitOptions.RemoveEmptyEntries);
        IEnumerable<IPublishedContent> currentChildren = mediaCache.GetAtRoot();
        IPublishedContent? resolvedMedia = null;

        foreach (var segment in segments)
        {
            resolvedMedia = currentChildren.FirstOrDefault(c => segment.InvariantEquals(c.Name));
            if (resolvedMedia is null)
            {
                break;
            }

            currentChildren = resolvedMedia.Children;
        }

        return resolvedMedia;
    }

    private Attempt<PagedModel<Guid>, ApiMediaQueryOperationStatus> PagedResult(IEnumerable<IPublishedContent> children, int skip, int take)
    {
        IPublishedContent[] childrenAsArray = children as IPublishedContent[] ?? children.ToArray();
        var result = new PagedModel<Guid>
        {
            Total = childrenAsArray.Length,
            Items = childrenAsArray.Skip(skip).Take(take).Select(child => child.Key)
        };

        return Attempt.SucceedWithStatus(ApiMediaQueryOperationStatus.Success, result);
    }
}
