using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Rendering;

internal sealed class RequestContextOutputExpansionStrategy : IOutputExpansionStrategy
{
    private readonly IApiPropertyRenderer _propertyRenderer;
    private bool _expandAll;
    private readonly string[] _expandAliases;

    public RequestContextOutputExpansionStrategy(IHttpContextAccessor httpContextAccessor, IApiPropertyRenderer propertyRenderer)
    {
        _propertyRenderer = propertyRenderer;
        (bool ExpandAll, string[] ExpanedAliases) initialState = InitialRequestState(httpContextAccessor);
        _expandAll = initialState.ExpandAll;
        _expandAliases = initialState.ExpanedAliases;
    }

    public IDictionary<string, object?> MapElementProperties(IPublishedElement element)
        => element.Properties.ToDictionary(
            p => p.Alias,
            p => GetPropertyValue(p, _expandAll));

    public IDictionary<string, object?> MapContentProperties(IPublishedContent content)
        => content.ItemType == PublishedItemType.Content
            ? MapProperties(content.Properties)
            : throw new ArgumentException($"Invalid item type. This method can only be used with item type {nameof(PublishedItemType.Content)}, got: {content.ItemType}");

    public IDictionary<string, object?> MapMediaProperties(IPublishedContent media, bool skipUmbracoProperties = true)
    {
        if (media.ItemType != PublishedItemType.Media)
        {
            throw new ArgumentException($"Invalid item type. This method can only be used with item type {PublishedItemType.Media}, got: {media.ItemType}");
        }

        IPublishedProperty[] properties = media
            .Properties
            .Where(p => skipUmbracoProperties is false || p.Alias.StartsWith("umbraco") is false)
            .ToArray();

        return properties.Any()
            ? MapProperties(properties)
            : new Dictionary<string, object?>();
    }

    private IDictionary<string, object?> MapProperties(IEnumerable<IPublishedProperty> properties)
    {
        var result = new Dictionary<string, object?>();

        switch (_expandAll)
        {
            case true:
                result = properties.ToDictionary(property => property.Alias, property => GetPropertyValue(property, false));
                break;
            default:
            {
                foreach (IPublishedProperty property in properties)
                {
                    if (_expandAliases.Contains(property.Alias))
                        _expandAll = true;

                    result.Add(property.Alias, GetPropertyValue(property, _expandAll));
                }
                break;
            }
        }

        return result;
    }

    private (bool ExpandAll, string[] ExpanedAliases) InitialRequestState(IHttpContextAccessor httpContextAccessor)
    {
        string? toExpand = httpContextAccessor.HttpContext?.Request.Query["expand"];
        if (toExpand.IsNullOrWhiteSpace())
        {
            return new(false, Array.Empty<string>());
        }

        const string propertySpecifier = "property:";
        return new(
            toExpand == "all",
            toExpand.StartsWith(propertySpecifier)
                ? toExpand.Substring(propertySpecifier.Length).Split(Constants.CharArrays.Comma)
                : Array.Empty<string>());
    }

    private object? GetPropertyValue(IPublishedProperty property, bool expanding)
        => _propertyRenderer.GetPropertyValue(property, expanding);
}
