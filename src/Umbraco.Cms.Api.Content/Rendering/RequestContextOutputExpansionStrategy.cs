using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Content.Rendering;

internal sealed class RequestContextOutputExpansionStrategy : IOutputExpansionStrategy
{
    private readonly bool _expandAll;
    private readonly string[] _expandAliases;

    private ExpansionState _state;

    public RequestContextOutputExpansionStrategy(IHttpContextAccessor httpContextAccessor)
    {
        (bool ExpandAll, string[] ExpanedAliases) initialState = InitialRequestState(httpContextAccessor);
        _expandAll = initialState.ExpandAll;
        _expandAliases = initialState.ExpanedAliases;
        _state = ExpansionState.Initial;
    }

    public IDictionary<string, object?> MapElementProperties(IPublishedElement element)
        => MapProperties(element.Properties);

    public IDictionary<string, object?> MapProperties(IEnumerable<IPublishedProperty> properties)
        => properties.ToDictionary(
            p => p.Alias,
            p => p.GetDeliveryApiValue(_state == ExpansionState.Expanding));

    public IDictionary<string, object?> MapContentProperties(IPublishedContent content)
    {
        // in the initial state, content properties should always be rendered (expanded if the requests dictates it).
        // this corresponds to the root level of a content item, i.e. when the initial content rendering starts.
        if (_state == ExpansionState.Initial)
        {
            // update state to pending so we don't end up here the next time around
            _state = ExpansionState.Pending;
            var rendered = content.Properties.ToDictionary(
                property => property.Alias,
                property =>
                {
                    // update state to expanding if the property should be expanded (needed for nested elements)
                    if (_expandAll || _expandAliases.Contains(property.Alias))
                    {
                        _state = ExpansionState.Expanding;
                    }

                    var value = property.GetDeliveryApiValue(_state == ExpansionState.Expanding);

                    // always revert to pending after rendering the property value
                    _state = ExpansionState.Pending;
                    return value;
                });
            _state = ExpansionState.Initial;
            return rendered;
        }

        // in an expanding state, properties should always be rendered as collapsed.
        // this corresponds to properties of a content based property placed directly below a root level property that is being expanded
        // (i.e. properties for picked content for an expanded content picker at root level).
        if (_state == ExpansionState.Expanding)
        {
            _state = ExpansionState.Expanded;
            var rendered = content.Properties.ToDictionary(
                property => property.Alias,
                property => property.GetDeliveryApiValue(false));
            _state = ExpansionState.Expanding;
            return rendered;
        }

        return new Dictionary<string, object?>();
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

    private enum ExpansionState
    {
        Initial,
        Pending,
        Expanding,
        Expanded
    }
}
