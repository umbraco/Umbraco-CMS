using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Rendering;

public class ElementOnlyOutputExpansionStrategy : IOutputExpansionStrategy
{
    protected const string All = "$all";
    protected const string None = "";
    protected const string ExpandParameterName = "expand";
    protected const string FieldsParameterName = "fields";

    private readonly IApiPropertyRenderer _propertyRenderer;
    private readonly IApiPublishedContentCache _apiPublishedContentCache;
    private readonly IRequestMemberAccessService _requestMemberAccessService;

    protected Stack<Node?> ExpandProperties { get; } = new();

    protected Stack<Node?> IncludeProperties { get; } = new();

    [Obsolete("Please use the constructor that accepts all parameters. Scheduled for removal in V20.")]
    public ElementOnlyOutputExpansionStrategy(IApiPropertyRenderer propertyRenderer)
        : this(
            propertyRenderer,
            StaticServiceProvider.Instance.GetRequiredService<IApiPublishedContentCache>(),
            StaticServiceProvider.Instance.GetRequiredService<IRequestMemberAccessService>())
    {
    }

    public ElementOnlyOutputExpansionStrategy(
        IApiPropertyRenderer propertyRenderer,
        IApiPublishedContentCache apiPublishedContentCache,
        IRequestMemberAccessService requestMemberAccessService)
    {
        _propertyRenderer = propertyRenderer;
        _apiPublishedContentCache = apiPublishedContentCache;
        _requestMemberAccessService = requestMemberAccessService;
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
