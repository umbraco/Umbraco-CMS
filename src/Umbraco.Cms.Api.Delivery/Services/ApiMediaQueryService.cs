﻿using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Services;

/// <inheritdoc />
internal sealed class ApiMediaQueryService : IApiMediaQueryService
{
    private readonly IPublishedMediaCache _publishedMediaCache;
    private readonly ILogger<ApiMediaQueryService> _logger;
    private readonly IMediaNavigationQueryService _mediaNavigationQueryService;
    private readonly IPublishedMediaStatusFilteringService _publishedMediaStatusFilteringService;

    public ApiMediaQueryService(
        IPublishedMediaCache publishedMediaCache,
        ILogger<ApiMediaQueryService> logger,
        IMediaNavigationQueryService mediaNavigationQueryService,
        IPublishedMediaStatusFilteringService publishedMediaStatusFilteringService)
    {
        _publishedMediaCache = publishedMediaCache;
        _logger = logger;
        _mediaNavigationQueryService = mediaNavigationQueryService;
        _publishedMediaStatusFilteringService = publishedMediaStatusFilteringService;
    }

    /// <inheritdoc/>
    public Attempt<PagedModel<Guid>, ApiMediaQueryOperationStatus> ExecuteQuery(string? fetch, IEnumerable<string> filters, IEnumerable<string> sorts, int skip, int take)
    {
        var emptyResult = new PagedModel<Guid>();

        IEnumerable<IPublishedContent>? source = GetSource(fetch);
        if (source is null)
        {
            return Attempt.FailWithStatus(ApiMediaQueryOperationStatus.SelectorOptionNotFound, emptyResult);
        }

        source = ApplyFilters(source, filters);
        if (source is null)
        {
            return Attempt.FailWithStatus(ApiMediaQueryOperationStatus.FilterOptionNotFound, emptyResult);
        }

        source = ApplySorts(source, sorts);
        if (source is null)
        {
            return Attempt.FailWithStatus(ApiMediaQueryOperationStatus.SortOptionNotFound, emptyResult);
        }

        return PagedResult(source, skip, take);
    }

    /// <inheritdoc/>
    public IPublishedContent? GetByPath(string path)
        => TryGetByPath(path, GetRequiredPublishedMediaCache());

    private IPublishedMediaCache GetRequiredPublishedMediaCache()
        => _publishedMediaCache;

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

            currentChildren = resolvedMedia.Children(_mediaNavigationQueryService, _publishedMediaStatusFilteringService);
        }

        return resolvedMedia;
    }

    private IEnumerable<IPublishedContent>? GetSource(string? fetch)
    {
        const string childrenOfParameter = "children:";

        if (fetch?.StartsWith(childrenOfParameter, StringComparison.OrdinalIgnoreCase) is not true)
        {
            _logger.LogInformation($"The current implementation of {nameof(IApiMediaQueryService)} expects \"{childrenOfParameter}[id/path]\" in the \"{nameof(fetch)}\" query option");
            return null;
        }

        var childrenOf = fetch.TrimStart(childrenOfParameter);
        if (childrenOf.IsNullOrWhiteSpace())
        {
            // this mirrors the current behavior of the Content Delivery API :-)
            return Array.Empty<IPublishedContent>();
        }

        IPublishedMediaCache mediaCache = GetRequiredPublishedMediaCache();
        if (childrenOf.Trim(Constants.CharArrays.ForwardSlash).Length == 0)
        {
            return mediaCache.GetAtRoot();
        }

        IPublishedContent? parent = Guid.TryParse(childrenOf, out Guid parentKey)
            ? mediaCache.GetById(parentKey)
            : TryGetByPath(childrenOf, mediaCache);

        return parent?.Children(_mediaNavigationQueryService, _publishedMediaStatusFilteringService) ?? Array.Empty<IPublishedContent>();
    }

    private IEnumerable<IPublishedContent>? ApplyFilters(IEnumerable<IPublishedContent> source, IEnumerable<string> filters)
    {
        foreach (var filter in filters)
        {
            var parts = filter.Split(':');
            if (parts.Length != 2)
            {
                // invalid filter
                _logger.LogInformation("An invalid filter option was encountered. Please ensure that supplied filter options are two-part, separated by ':'.");
                return null;
            }

            switch (parts[0])
            {
                case "mediaType":
                    source = source.Where(c => c.ContentType.Alias == parts[1]);
                    break;
                case "name":
                    source = source.Where(c => c.Name.InvariantContains(parts[1]));
                    break;
                default:
                    // unknown filter
                    _logger.LogInformation("An unsupported filter option was supplied for the query. Please use only valid filter options. See the documentation for details.");
                    return null;
            }
        }

        return source;
    }

    private IEnumerable<IPublishedContent>? ApplySorts(IEnumerable<IPublishedContent> source, IEnumerable<string> sorts)
    {
        foreach (var sort in sorts)
        {
            var parts = sort.Split(':');
            if (parts.Length != 2)
            {
                // invalid sort
                _logger.LogInformation("An invalid sort option was encountered. Please ensure that the supplied sort options are two-part, separated by ':'.");
                return null;
            }

            Func<IPublishedContent, object> keySelector;
            switch (parts[0])
            {
                case "createDate":
                    keySelector = content => content.CreateDate;
                    break;
                case "updateDate":
                    keySelector = content => content.UpdateDate;
                    break;
                case "name":
                    keySelector = content => content.Name.ToLowerInvariant();
                    break;
                case "sortOrder":
                    keySelector = content => content.SortOrder;
                    break;
                default:
                    // unknown sort
                    _logger.LogInformation("An unsupported sort option was supplied for the query. Please use only valid sort options. See the documentation for details.");
                    return null;
            }

            source = parts[1].StartsWith("asc")
                ? source.OrderBy(keySelector)
                : source.OrderByDescending(keySelector);
        }

        return source;
    }


    private static Attempt<PagedModel<Guid>, ApiMediaQueryOperationStatus> PagedResult(IEnumerable<IPublishedContent> children, int skip, int take)
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
