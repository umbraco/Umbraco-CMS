using Examine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

internal sealed class DeliveryApiContentIndexValueSetBuilder : IDeliveryApiContentIndexValueSetBuilder
{
    private readonly ContentIndexHandlerCollection _contentIndexHandlerCollection;
    private readonly IContentService _contentService;
    private readonly IPublicAccessService _publicAccessService;
    private readonly ILogger<DeliveryApiContentIndexValueSetBuilder> _logger;
    private readonly IDeliveryApiContentIndexFieldDefinitionBuilder _deliveryApiContentIndexFieldDefinitionBuilder;
    private readonly IMemberService _memberService;
    private readonly IDeliveryApiCompositeIdHandler _deliveryApiCompositeIdHandler;
    private DeliveryApiSettings _deliveryApiSettings;

    [Obsolete("Please use ctor that takes an IDeliveryApiCompositeIdHandler. Scheduled for removal in v15")]
    public DeliveryApiContentIndexValueSetBuilder(
        ContentIndexHandlerCollection contentIndexHandlerCollection,
        IContentService contentService,
        IPublicAccessService publicAccessService,
        ILogger<DeliveryApiContentIndexValueSetBuilder> logger,
        IDeliveryApiContentIndexFieldDefinitionBuilder deliveryApiContentIndexFieldDefinitionBuilder,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings,
        IMemberService memberService)
    : this(
        contentIndexHandlerCollection,
        contentService,
        publicAccessService,
        logger,
        deliveryApiContentIndexFieldDefinitionBuilder,
        deliveryApiSettings,
        memberService,
        StaticServiceProvider.Instance.GetRequiredService<IDeliveryApiCompositeIdHandler>())
    {
    }

    public DeliveryApiContentIndexValueSetBuilder(
        ContentIndexHandlerCollection contentIndexHandlerCollection,
        IContentService contentService,
        IPublicAccessService publicAccessService,
        ILogger<DeliveryApiContentIndexValueSetBuilder> logger,
        IDeliveryApiContentIndexFieldDefinitionBuilder deliveryApiContentIndexFieldDefinitionBuilder,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings,
        IMemberService memberService,
        IDeliveryApiCompositeIdHandler deliveryApiCompositeIdHandler)
    {
        _contentIndexHandlerCollection = contentIndexHandlerCollection;
        _publicAccessService = publicAccessService;
        _logger = logger;
        _deliveryApiContentIndexFieldDefinitionBuilder = deliveryApiContentIndexFieldDefinitionBuilder;
        _memberService = memberService;
        _deliveryApiCompositeIdHandler = deliveryApiCompositeIdHandler;
        _contentService = contentService;
        _deliveryApiSettings = deliveryApiSettings.CurrentValue;
        deliveryApiSettings.OnChange(settings => _deliveryApiSettings = settings);
    }

    /// <inheritdoc />
    public IEnumerable<ValueSet> GetValueSets(params IContent[] contents)
    {
        FieldDefinitionCollection fieldDefinitions = _deliveryApiContentIndexFieldDefinitionBuilder.Build();
        foreach (IContent content in contents.Where(CanIndex))
        {
            var publishedCultures = PublishedCultures(content);
            var availableCultures = AvailableCultures(content);

            foreach (var culture in availableCultures)
            {
                var indexCulture = culture ?? "none";
                var isPublished = publishedCultures.Contains(culture);

                // required index values go here
                var indexValues = new Dictionary<string, IEnumerable<object>>(StringComparer.InvariantCultureIgnoreCase)
                {
                    [UmbracoExamineFieldNames.DeliveryApiContentIndex.Id] = new object[] { content.Id.ToString() }, // required for correct publishing handling and also needed for backoffice index browsing
                    [UmbracoExamineFieldNames.DeliveryApiContentIndex.ContentTypeId] = new object[] { content.ContentTypeId.ToString() }, // required for correct content type change handling
                    [UmbracoExamineFieldNames.DeliveryApiContentIndex.Culture] = new object[] { indexCulture }, // required for culture variant querying
                    [UmbracoExamineFieldNames.DeliveryApiContentIndex.Published] = new object[] { isPublished ? "y" : "n" }, // required for querying draft content
                    [UmbracoExamineFieldNames.IndexPathFieldName] = new object[] { content.Path }, // required for unpublishing/deletion handling
                    [UmbracoExamineFieldNames.NodeNameFieldName] = new object[] { content.GetPublishName(culture) ?? content.GetCultureName(culture) ?? string.Empty }, // primarily needed for backoffice index browsing
                };

                if (_deliveryApiSettings.MemberAuthorizationIsEnabled())
                {
                    var protectedAccessValue = ProtectedAccessValue(content, out var isProtected);
                    indexValues[UmbracoExamineFieldNames.DeliveryApiContentIndex.Protected] = new object[] { isProtected ? "y" : "n" }; // required for querying protected content
                    indexValues[UmbracoExamineFieldNames.DeliveryApiContentIndex.ProtectedAccess] = protectedAccessValue; // required for querying protected content
                }

                AddContentIndexHandlerFields(content, culture, fieldDefinitions, indexValues);

                yield return new ValueSet(_deliveryApiCompositeIdHandler.IndexId(content.Id, indexCulture), IndexTypes.Content, content.ContentType.Alias, indexValues);
            }
        }
    }

    private string?[] AvailableCultures(IContent content)
        => content.ContentType.VariesByCulture()
            ? content.AvailableCultures.ToArray()
            : new string?[] { null };

    private string?[] PublishedCultures(IContent content)
    {
        if (content.Published == false)
        {
            return Array.Empty<string>();
        }

        var variesByCulture = content.ContentType.VariesByCulture();

        // if the content varies by culture, the indexable cultures are the published
        // cultures - otherwise "null" represents "no culture"
        var cultures = variesByCulture
            ? content.PublishedCultures.ToArray()
            : new string?[] { null };

        // now iterate all ancestors and make sure all cultures are published all the way up the tree
        foreach (var ancestorId in content.GetAncestorIds() ?? Array.Empty<int>())
        {
            IContent? ancestor = _contentService.GetById(ancestorId);
            if (ancestor is null || ancestor.Published is false)
            {
                // no published ancestor => don't index anything
                cultures = Array.Empty<string?>();
            }
            else if (variesByCulture && ancestor.ContentType.VariesByCulture())
            {
                // both the content and the ancestor are culture variant => only index the published cultures they have in common
                cultures = cultures.Intersect(ancestor.PublishedCultures).ToArray();
            }

            // if we've already run out of cultures to index, there is no reason to iterate the ancestors any further
            if (cultures.Any() == false)
            {
                break;
            }
        }

        return cultures;
    }

    private string[] ProtectedAccessValue(IContent content, out bool isProtected)
    {
        PublicAccessEntry? publicAccessEntry = _publicAccessService.GetEntryForContent(content.Path);
        isProtected = publicAccessEntry is not null;

        if (publicAccessEntry is null)
        {
            return Array.Empty<string>();
        }

        return publicAccessEntry
                .Rules
                // prefix member roles with "r:" and member keys with "u:" for clarity
                .Select(r =>
                {
                    if (r.RuleValue.IsNullOrWhiteSpace())
                    {
                        return null;
                    }

                    if (r.RuleType is Constants.Conventions.PublicAccess.MemberRoleRuleType)
                    {
                        return $"r:{r.RuleValue}";
                    }

                    IMember? member = _memberService.GetByUsername(r.RuleValue);
                    return member is not null ? $"u:{member.Key}" : null;
                })
                .WhereNotNull()
                .ToArray();
    }

    private void AddContentIndexHandlerFields(IContent content, string? culture, FieldDefinitionCollection fieldDefinitions, Dictionary<string, IEnumerable<object>> indexValues)
    {
        foreach (IContentIndexHandler handler in _contentIndexHandlerCollection)
        {
            IndexFieldValue[] fieldValues = handler.GetFieldValues(content, culture).ToArray();
            foreach (IndexFieldValue fieldValue in fieldValues)
            {
                if (indexValues.ContainsKey(fieldValue.FieldName))
                {
                    _logger.LogWarning("Duplicate field value found for field name {FieldName} among the index handlers - first one wins.", fieldValue.FieldName);
                    continue;
                }

                // Examine will be case sensitive in the default setup; we need to deal with that for sortable text fields
                if (fieldDefinitions.TryGetValue(fieldValue.FieldName, out FieldDefinition fieldDefinition)
                    && fieldDefinition.Type == FieldDefinitionTypes.FullTextSortable
                    && fieldValue.Values.All(value => value is string))
                {
                    indexValues[fieldValue.FieldName] = fieldValue.Values.OfType<string>().Select(value => value.ToLowerInvariant()).ToArray();
                }
                else
                {
                    indexValues[fieldValue.FieldName] = fieldValue.Values.ToArray();
                }
            }
        }
    }

    private bool CanIndex(IContent content)
    {
        // is the content in a state that is allowed in the index?
        if (content.Trashed)
        {
            return false;
        }

        // is the content type allowed in the index?
        if (_deliveryApiSettings.IsDisallowedContentType(content.ContentType.Alias))
        {
            return false;
        }

        // is the content protected and Delivery API member authorization disabled?
        if (_deliveryApiSettings.MemberAuthorizationIsEnabled() is false && _publicAccessService.IsProtected(content.Path).Success)
        {
            return false;
        }

        return true;
    }
}
