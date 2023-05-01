using Examine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

public class DeliveryApiContentIndexValueSetBuilder : IDeliveryApiContentIndexValueSetBuilder
{
    private readonly ContentIndexHandlerCollection _contentIndexHandlerCollection;
    private readonly IScopeProvider _scopeProvider;
    private readonly IPublicAccessService _publicAccessService;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<DeliveryApiContentIndexValueSetBuilder> _logger;
    private DeliveryApiSettings _deliveryApiSettings;

    public DeliveryApiContentIndexValueSetBuilder(
        ContentIndexHandlerCollection contentIndexHandlerCollection,
        IScopeProvider scopeProvider,
        IPublicAccessService publicAccessService,
        ILocalizationService localizationService,
        ILogger<DeliveryApiContentIndexValueSetBuilder> logger,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings)
    {
        _contentIndexHandlerCollection = contentIndexHandlerCollection;
        _scopeProvider = scopeProvider;
        _publicAccessService = publicAccessService;
        _localizationService = localizationService;
        _logger = logger;
        _deliveryApiSettings = deliveryApiSettings.CurrentValue;
        deliveryApiSettings.OnChange(settings => _deliveryApiSettings = settings);
    }

    /// <inheritdoc />
    public IEnumerable<ValueSet> GetValueSets(params IContent[] contents)
    {
        var allCultures = _localizationService.GetAllLanguages().Select(l => l.IsoCode).ToArray();
        foreach (IContent content in contents.Where(CanIndex))
        {
            var contentVariesByCulture = content.ContentType.VariesByCulture();

            // which cultures should we add to the index?
            // - if the content type is culture variant, only add the published cultures of the content
            // - otherwise "fake" all available cultures (thus indexing the same values for all cultures)
            var indexableCultures = contentVariesByCulture
                ? content.PublishedCultures.ToArray()
                : allCultures;

            var availableCulturesIndexValue = indexableCultures.Select(culture => culture.ToLowerInvariant()).ToArray();

            // required index values go here
            var indexValues = new Dictionary<string, IEnumerable<object>>(StringComparer.InvariantCultureIgnoreCase)
            {
                ["id"] = new object[] { content.Id }, // required for unpublishing/deletion handling
                ["cultures"] = availableCulturesIndexValue, // required for culture variant querying
                [UmbracoExamineFieldNames.IndexPathFieldName] = new object[] { content.Path }, // required for unpublishing/deletion handling
                [UmbracoExamineFieldNames.NodeNameFieldName] = new object[] { content.PublishName ?? string.Empty }, // primarily needed for backoffice index browsing
            };

            AddContentIndexHandlerFields(content, indexableCultures, indexValues);

            yield return new ValueSet(content.Id.ToString(), IndexTypes.Content, content.ContentType.Alias, indexValues);
        }
    }

    private void AddContentIndexHandlerFields(IContent content, string[] cultures, Dictionary<string, IEnumerable<object>> indexValues)
    {
        var contentVariesByCulture = content.ContentType.VariesByCulture();

        foreach (IContentIndexHandler handler in _contentIndexHandlerCollection)
        {
            var cultureVariantFields = handler
                .GetFields()
                .Where(f => f.VariesByCulture)
                .Select(f => f.FieldName)
                .ToArray();

            IndexFieldValue[] fieldValues;
            if (contentVariesByCulture is false)
            {
                // for culture invariant content we'll get the field values once and "expand" the culture variant fields
                // with one field per culture (see also the field definition builder)
                fieldValues = handler.GetFieldValues(content, null)
                    .SelectMany(fieldValue =>
                        // if the field is culture variant, create a dedicated field per culture - otherwise just use the field as is
                        cultureVariantFields.Contains(fieldValue.FieldName)
                            ? cultures.Select(culture => new IndexFieldValue
                            {
                                FieldName = DeliveryApiContentIndexFieldDefinitionBuilder.CultureVariantIndexFieldName(fieldValue.FieldName, culture),
                                Values = fieldValue.Values
                            })
                            : new[] { fieldValue })
                    .ToArray();
            }
            else
            {
                // for culture variant culture we'll get the field values for each culture one at a time
                fieldValues = cultures.SelectMany(culture =>
                        handler
                            .GetFieldValues(content, culture)
                            .Select(fieldValue =>
                                // if the field is culture variant, create a new field specifically for this culture - otherwise just use the field as is
                                cultureVariantFields.Contains(fieldValue.FieldName)
                                    ? new IndexFieldValue
                                    {
                                        FieldName = DeliveryApiContentIndexFieldDefinitionBuilder.CultureVariantIndexFieldName(fieldValue.FieldName, culture),
                                        Values = fieldValue.Values
                                    }
                                    : fieldValue))
                    // for culture invariant fields we expect duplicates (one per culture) - let's remove those
                    .DistinctBy(fieldValue => fieldValue.FieldName)
                    .ToArray();
            }

            foreach (IndexFieldValue fieldValue in fieldValues)
            {
                if (indexValues.ContainsKey(fieldValue.FieldName))
                {
                    _logger.LogWarning("Duplicate field value found for field name {FieldName} among the index handlers - first one wins.", fieldValue.FieldName);
                    continue;
                }

                indexValues[fieldValue.FieldName] = fieldValue.Values.ToArray();
            }
        }
    }

    private bool CanIndex(IContent content)
    {
        // is the content in a state that is allowed in the index?
        if (content.Published is false || content.Trashed)
        {
            return false;
        }

        // is the content type allowed in the index?
        if (_deliveryApiSettings.IsDisallowedContentType(content.ContentType.Alias))
        {
            return false;
        }

        // is the content protected?
        using (_scopeProvider.CreateScope(autoComplete: true))
        {
            if (_publicAccessService.IsProtected(content.Path).Success)
            {
                return false;
            }
        }

        return true;
    }
}
