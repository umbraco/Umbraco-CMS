using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;
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
    private readonly IApiPublishedContentCache _apiPublishedContentCache;
    private readonly IRequestMemberAccessService _requestMemberAccessService;

    /// <summary>
    ///     Gets the stack of expand property nodes for tracking nested expansions.
    /// </summary>
    protected Stack<Node?> ExpandProperties { get; } = new();

    /// <summary>
    ///     Gets the stack of include property nodes for tracking nested field selections.
    /// </summary>
    protected Stack<Node?> IncludeProperties { get; } = new();

    [Obsolete("Please use the constructor that accepts all parameters. Scheduled for removal in V20.")]
    public ElementOnlyOutputExpansionStrategy(IApiPropertyRenderer propertyRenderer)
        : this(
            propertyRenderer,
            StaticServiceProvider.Instance.GetRequiredService<IApiPublishedContentCache>(),
            StaticServiceProvider.Instance.GetRequiredService<IRequestMemberAccessService>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ElementOnlyOutputExpansionStrategy"/> class.
    /// </summary>
    /// <param name="propertyRenderer">The property renderer for converting property values.</param>
    /// <param name="apiPublishedContentCache">The published content cache for the Delivery API.</param>
    /// <param name="requestMemberAccessService">The service responsible for contextual access control in the Delivery API.</param>
    public ElementOnlyOutputExpansionStrategy(
        IApiPropertyRenderer propertyRenderer,
        IApiPublishedContentCache apiPublishedContentCache,
        IRequestMemberAccessService requestMemberAccessService)
    {
        _propertyRenderer = propertyRenderer;
        _apiPublishedContentCache = apiPublishedContentCache;
        _requestMemberAccessService = requestMemberAccessService;
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
    {
        var expanding = ExpandProperties.Peek() is not null;
        var propertyValue = _propertyRenderer.GetPropertyValue(property, expanding);

        // If the property value is content (e.g. from a content picker or a multi-node tree picker), we need to
        // add additional access control handling, to prevent leaking content that should not have been output.
        return propertyValue switch
        {
            IApiContent apiContent => ApplyContentAccessControl(apiContent, expanding),
            IEnumerable<IApiContent> apiContents => apiContents
                .Select(apiContent => ApplyContentAccessControl(apiContent, expanding))
                .WhereNotNull()
                .ToArray(),
            _ => propertyValue,
        };
    }

    private IApiContent? ApplyContentAccessControl(IApiContent apiContent, bool expanding)
    {
        IPublishedContent? content = _apiPublishedContentCache.GetById(apiContent.Id);
        if (content is null)
        {
            // The content was actually resolved in the property value converter, but it is not permitted
            // to output through the Delivery API. Omit it from the output (an explicit null value for a single
            // reference; excluded from the collection for a multi-node tree picker).
            return null;
        }

        // Shortcut the access check below if it is not required.
        if (expanding is false)
        {
            return apiContent;
        }

        // The content is being expanded. We need to make sure it's permitted.
        PublicAccessStatus access = _requestMemberAccessService.MemberHasAccessToAsync(content).GetAwaiter().GetResult();
        if (access is not PublicAccessStatus.AccessAccepted)
        {
            // The current auth context does not allow access to the content. We still need to return the content
            // reference for rendering (i.e. a link to a page, which returns 401 and causes a redirect to a login
            // screen), but we must make sure the page content is not leaked.
            apiContent.Properties.Clear();
        }

        return apiContent;
    }

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
            ReadOnlySpan<char> valueAsSpan = value.AsSpan();
            if (valueAsSpan.Count('[') != valueAsSpan.Count(']'))
            {
                throw new ArgumentException("Value did not contain an equal number of start and end brackets");
            }

            // verify that the value does not start with a start bracket
            if (value.StartsWith('['))
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
