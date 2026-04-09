using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.Rendering;

/// <summary>
///     Implements output expansion strategy for element-only rendering in the Delivery API.
/// </summary>
/// <remarks>
///     This strategy handles the expansion and filtering of properties when rendering content
///     through the Delivery API based on expand and fields query parameters.
/// </remarks>
public class ElementOnlyOutputExpansionStrategy : IOutputExpansionStrategy
{
    /// <summary>
    ///     The parameter value indicating all properties should be included.
    /// </summary>
    protected const string All = "$all";

    /// <summary>
    ///     The parameter value indicating no properties should be included.
    /// </summary>
    protected const string None = "";

    /// <summary>
    ///     The name of the expand query parameter.
    /// </summary>
    protected const string ExpandParameterName = "expand";

    /// <summary>
    ///     The name of the fields query parameter.
    /// </summary>
    protected const string FieldsParameterName = "fields";

    private readonly IApiPropertyRenderer _propertyRenderer;

    /// <summary>
    ///     Gets the stack of expand property nodes for tracking nested expansions.
    /// </summary>
    protected Stack<Node?> ExpandProperties { get; } = new();

    /// <summary>
    ///     Gets the stack of include property nodes for tracking nested field selections.
    /// </summary>
    protected Stack<Node?> IncludeProperties { get; } = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ElementOnlyOutputExpansionStrategy"/> class.
    /// </summary>
    /// <param name="propertyRenderer">The property renderer for converting property values.</param>
    public ElementOnlyOutputExpansionStrategy(
        IApiPropertyRenderer propertyRenderer)
    {
        _propertyRenderer = propertyRenderer;
    }

    /// <inheritdoc/>
    public virtual IDictionary<string, object?> MapContentProperties(IPublishedContent content)
        => content.ItemType == PublishedItemType.Content
            ? MapProperties(content.Properties)
            : throw new ArgumentException($"Invalid item type. This method can only be used with item type {nameof(PublishedItemType.Content)}, got: {content.ItemType}");

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <summary>
    ///     Represents a node in the parsed expand/fields parameter tree structure.
    /// </summary>
    protected sealed class Node
    {
        /// <summary>
        ///     Gets the key of this node.
        /// </summary>
        public string Key { get; private set; } = string.Empty;

        /// <summary>
        ///     Gets the child nodes of this node.
        /// </summary>
        public List<Node> Items { get; } = new();

        /// <summary>
        ///     Parses an expand/fields parameter value into a node tree structure.
        /// </summary>
        /// <param name="value">The parameter value to parse.</param>
        /// <returns>The root node of the parsed tree.</returns>
        /// <exception cref="ArgumentException">Thrown when the value has invalid syntax.</exception>
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
