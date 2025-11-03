using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Rendering;

public class ElementOnlyOutputExpansionStrategy : IOutputExpansionStrategy
{
    protected const string All = "$all";
    protected const string None = "";
    protected const string ExpandParameterName = "expand";
    protected const string FieldsParameterName = "fields";

    private readonly IApiPropertyRenderer _propertyRenderer;

    protected Stack<Node?> ExpandProperties { get; } = new();

    protected Stack<Node?> IncludeProperties { get; } = new();

    public ElementOnlyOutputExpansionStrategy(
        IApiPropertyRenderer propertyRenderer)
    {
        _propertyRenderer = propertyRenderer;
    }

    public virtual IDictionary<string, object?> MapContentProperties(IPublishedContent content)
        => content.ItemType == PublishedItemType.Content
            ? MapProperties(content.Properties)
            : throw new ArgumentException($"Invalid item type. This method can only be used with item type {nameof(PublishedItemType.Content)}, got: {content.ItemType}");

    public virtual IDictionary<string, object?> MapMediaProperties(IPublishedContent media, bool skipUmbracoProperties = true)
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

    public virtual IDictionary<string, object?> MapElementProperties(IPublishedElement element)
        => MapProperties(element.Properties, true);

    private IDictionary<string, object?> MapProperties(IEnumerable<IPublishedProperty> properties, bool forceExpandProperties = false)
    {
        Node? currentExpandProperties = ExpandProperties.Count > 0 ? ExpandProperties.Peek() : null;
        if (ExpandProperties.Count > 1 && currentExpandProperties is null && forceExpandProperties is false)
        {
            return new Dictionary<string, object?>();
        }

        Node? currentIncludeProperties = IncludeProperties.Count > 0 ? IncludeProperties.Peek() : null;
        var result = new Dictionary<string, object?>();
        foreach (IPublishedProperty property in properties)
        {
            Node? nextIncludeProperties = GetNextProperties(currentIncludeProperties, property.Alias);
            if (currentIncludeProperties is not null && currentIncludeProperties.Items.Any() && nextIncludeProperties is null)
            {
                continue;
            }

            Node? nextExpandProperties = GetNextProperties(currentExpandProperties, property.Alias);

            IncludeProperties.Push(nextIncludeProperties);
            ExpandProperties.Push(nextExpandProperties);

            result[property.Alias] = GetPropertyValue(property);

            ExpandProperties.Pop();
            IncludeProperties.Pop();
        }

        return result;
    }

    private Node? GetNextProperties(Node? currentProperties, string propertyAlias)
        => currentProperties?.Items.FirstOrDefault(i => i.Key == All)
           ?? currentProperties?.Items.FirstOrDefault(i => i.Key == "properties")?.Items.FirstOrDefault(i => i.Key == All || i.Key == propertyAlias);

    private object? GetPropertyValue(IPublishedProperty property)
        => _propertyRenderer.GetPropertyValue(property, ExpandProperties.Peek() is not null);

    protected sealed class Node
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
