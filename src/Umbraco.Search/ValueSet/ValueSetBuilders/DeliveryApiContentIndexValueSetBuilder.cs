﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Search.ValueSet.ValueSetBuilders;

internal sealed class DeliveryApiContentIndexValueSetBuilder : IDeliveryApiContentIndexValueSetBuilder
{
    private readonly ContentIndexHandlerCollection _contentIndexHandlerCollection;
    private readonly IContentService _contentService;
    private readonly IPublicAccessService _publicAccessService;
    private readonly ILogger<DeliveryApiContentIndexValueSetBuilder> _logger;
    private DeliveryApiSettings _deliveryApiSettings;

    public DeliveryApiContentIndexValueSetBuilder(
        ContentIndexHandlerCollection contentIndexHandlerCollection,
        IContentService contentService,
        IPublicAccessService publicAccessService,
        ILogger<DeliveryApiContentIndexValueSetBuilder> logger,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings)
    {
        _contentIndexHandlerCollection = contentIndexHandlerCollection;
        _publicAccessService = publicAccessService;
        _logger = logger;
        _contentService = contentService;
        _deliveryApiSettings = deliveryApiSettings.CurrentValue;
        deliveryApiSettings.OnChange(settings => _deliveryApiSettings = settings);
    }

    /// <inheritdoc />
    public IEnumerable<UmbracoValueSet> GetValueSets(params IContentBase[] contents)
    {
        foreach (IContent content in contents.Where(CanIndex))
        {
            var cultures = IndexableCultures(content, out var ancestors);

            foreach (var culture in cultures)
            {
                var indexCulture = culture ?? "none";

                // required index values go here
                var indexValues = new Dictionary<string, IEnumerable<object>>(StringComparer.InvariantCultureIgnoreCase)
                {
                    [UmbracoSearchFieldNames.DeliveryApiContentIndex.Id] =
                        new object[]
                        {
                            content.Id.ToString()
                        }, // required for correct publishing handling and also needed for backoffice index browsing
                    [UmbracoSearchFieldNames.DeliveryApiContentIndex.AncestorIds] =
                        ancestors.Select(x => x.ToString()).ToArray(), // required for descendant queries
                    [UmbracoSearchFieldNames.DeliveryApiContentIndex.ContentTypeId] =
                        new object[]
                        {
                            content.ContentTypeId.ToString()
                        }, // required for correct content type change handling
                    [UmbracoSearchFieldNames.DeliveryApiContentIndex.Culture] =
                        new object[] { indexCulture }, // required for culture variant querying
                    [UmbracoSearchFieldNames.IndexPathFieldName] =
                        new object[] { content.Path }, // required for unpublishing/deletion handling
                    [UmbracoSearchFieldNames.PublishedFieldName] =
                        new object[] { content.Published? "y": "n" }, // required for validating value sets
                    [UmbracoSearchFieldNames.NodeNameFieldName] =
                        new object[]
                        {
                            content.GetPublishName(culture) ?? string.Empty
                        }, // primarily needed for backoffice index browsing
                };

                AddContentIndexHandlerFields(content, culture, indexValues);

                yield return new UmbracoValueSet(DeliveryApiContentIndexUtilites.IndexId(content, indexCulture),
                    IndexTypes.Content, content.ContentType.Alias, indexValues);
            }
        }
    }

    private string?[] IndexableCultures(IContentBase content, out IEnumerable<Guid> ancestors)
    {
        var variesByCulture = content.ContentType.VariesByCulture();
        var list = new List<Guid>();
        // if the content varies by culture, the indexable cultures are the published
        // cultures - otherwise "null" represents "no culture"
        var cultures = variesByCulture && content is IContent contentModel
            ? contentModel.PublishedCultures.ToArray()
            : new string?[] { null };

        // now iterate all ancestors and make sure all cultures are published all the way up the tree
        foreach (var ancestorId in content.GetAncestorIds() ?? Array.Empty<int>())
        {
            IContent? ancestor = _contentService.GetById(ancestorId);
            if (ancestor != null)
            {
                list.Add(ancestor.Key);
            }
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
        ancestors = list;
        return cultures;
    }

    private void AddContentIndexHandlerFields(IContentBase content, string? culture,
        Dictionary<string, IEnumerable<object>> indexValues)
    {
        foreach (IContentIndexHandler handler in _contentIndexHandlerCollection)
        {
            IndexFieldValue[] fieldValues = handler.GetFieldValues(content, culture).ToArray();
            foreach (IndexFieldValue fieldValue in fieldValues)
            {
                if (indexValues.ContainsKey(fieldValue.FieldName))
                {
                    _logger.LogWarning(
                        "Duplicate field value found for field name {FieldName} among the index handlers - first one wins.",
                        fieldValue.FieldName);
                    continue;
                }

                indexValues[fieldValue.FieldName] = fieldValue.Values.ToArray();
            }
        }
    }

    private bool CanIndex(IContentBase content)
    {
        // is the content in a state that is allowed in the index?
        if (content is IContent contentModel && contentModel.Published is false || content.Trashed)
        {
            return false;
        }

        // is the content type allowed in the index?
        if (_deliveryApiSettings.IsDisallowedContentType(content.ContentType.Alias))
        {
            return false;
        }

        // is the content protected?
        if (_publicAccessService.IsProtected(content.Path).Success)
        {
            return false;
        }

        return true;
    }
}
