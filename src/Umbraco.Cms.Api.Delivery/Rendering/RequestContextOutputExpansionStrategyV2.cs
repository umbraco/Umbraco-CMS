using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Rendering;

internal sealed class RequestContextOutputExpansionStrategyV2 : IOutputExpansionStrategy
{
    private const string All = "$all";
    private const string None = "";
    private const string ExpandParameterName = "expand";
    private const string FieldsParameterName = "fields";

    private readonly IApiPropertyRenderer _propertyRenderer;
    private readonly ILogger<RequestContextOutputExpansionStrategyV2> _logger;

    private readonly Stack<Node?> _expandProperties;
    private readonly Stack<Node?> _includeProperties;

    public RequestContextOutputExpansionStrategyV2(
        IHttpContextAccessor httpContextAccessor,
        IApiPropertyRenderer propertyRenderer,
        ILogger<RequestContextOutputExpansionStrategyV2> logger)
    {
        _propertyRenderer = propertyRenderer;
        _logger = logger;
        _expandProperties = new Stack<Node?>();
        _includeProperties = new Stack<Node?>();

        InitializeExpandAndInclude(httpContextAccessor);
    }

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

    public IDictionary<string, object?> MapElementProperties(IPublishedElement element)
        => MapProperties(element.Properties, true);

    private void InitializeExpandAndInclude(IHttpContextAccessor httpContextAccessor)
    {
        string? QueryValue(string key) => httpContextAccessor.HttpContext?.Request.Query[key];

        var toExpand = QueryValue(ExpandParameterName) ?? None;
        var toInclude = QueryValue(FieldsParameterName) ?? All;

        try
        {
            _expandProperties.Push(Node.Parse(toExpand));
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"Could not parse the '{ExpandParameterName}' parameter. See exception for details.");
            throw new ArgumentException($"Could not parse the '{ExpandParameterName}' parameter: {ex.Message}");
        }

        try
        {
            _includeProperties.Push(Node.Parse(toInclude));
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"Could not parse the '{FieldsParameterName}' parameter. See exception for details.");
            throw new ArgumentException($"Could not parse the '{FieldsParameterName}' parameter: {ex.Message}");
        }
    }

    private IDictionary<string, object?> MapProperties(IEnumerable<IPublishedProperty> properties, bool forceExpandProperties = false)
    {
        Node? currentExpandProperties = _expandProperties.Peek();
        if (_expandProperties.Count > 1 && currentExpandProperties is null && forceExpandProperties is false)
        {
            return new Dictionary<string, object?>();
        }

        Node? currentIncludeProperties = _includeProperties.Peek();
        var result = new Dictionary<string, object?>();
        foreach (IPublishedProperty property in properties)
        {
            Node? nextIncludeProperties = GetNextProperties(currentIncludeProperties, property.Alias);
            if (currentIncludeProperties is not null && currentIncludeProperties.Items.Any() && nextIncludeProperties is null)
            {
                continue;
            }

            Node? nextExpandProperties = GetNextProperties(currentExpandProperties, property.Alias);

            _includeProperties.Push(nextIncludeProperties);
            _expandProperties.Push(nextExpandProperties);

            result[property.Alias] = GetPropertyValue(property);

            _expandProperties.Pop();
            _includeProperties.Pop();
        }

        return result;
    }

    private Node? GetNextProperties(Node? currentProperties, string propertyAlias)
        => currentProperties?.Items.FirstOrDefault(i => i.Key == All)
           ?? currentProperties?.Items.FirstOrDefault(i => i.Key == "properties")?.Items.FirstOrDefault(i => i.Key == All || i.Key == propertyAlias);

    private object? GetPropertyValue(IPublishedProperty property)
        => _propertyRenderer.GetPropertyValue(property, _expandProperties.Peek() is not null);

    private class Node
    {
        public string Key { get; private set; } = string.Empty;

        public List<Node> Items { get; } = new();

        public static Node Parse(string value)
        {
            // verify that there are as many start brackets as there are end brackets
            if (value.CountOccurrences("[") != value.CountOccurrences("]"))
            {
                throw new ArgumentException("Value did not contain an equal number of start and end brackets");
            }

            // verify that the value does not start with a start bracket
            if (value.StartsWith("["))
            {
                throw new ArgumentException("Value cannot start with a bracket");
            }

            // verify that there are no empty brackets
            if (value.Contains("[]"))
            {
                throw new ArgumentException("Value cannot contain empty brackets");
            }

            var stack = new Stack<Node>();
            var root = new Node { Key = "root" };
            stack.Push(root);

            var currentNode = new Node();
            root.Items.Add(currentNode);

            foreach (char c in value)
            {
                switch (c)
                {
                    case '[': // Start a new node, child of the current node
                        stack.Push(currentNode);
                        currentNode = new Node();
                        stack.Peek().Items.Add(currentNode);
                        break;
                    case ',': // Start a new node, but at the same level of the current node
                        currentNode = new Node();
                        stack.Peek().Items.Add(currentNode);
                        break;
                    case ']': // Back to parent of the current node
                        currentNode = stack.Pop();
                        break;
                    default: // Add char to current node key
                        currentNode.Key += c;
                        break;
                }
            }

            return root;
        }
    }
}
