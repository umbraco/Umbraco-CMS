using Examine;
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
    private DeliveryApiSettings _deliveryApiSettings;

    public DeliveryApiContentIndexValueSetBuilder(
        ContentIndexHandlerCollection contentIndexHandlerCollection,
        IScopeProvider scopeProvider,
        IPublicAccessService publicAccessService,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings)
    {
        _contentIndexHandlerCollection = contentIndexHandlerCollection;
        _scopeProvider = scopeProvider;
        _publicAccessService = publicAccessService;
        _deliveryApiSettings = deliveryApiSettings.CurrentValue;
        deliveryApiSettings.OnChange(settings => _deliveryApiSettings = settings);
    }

    /// <inheritdoc />
    public IEnumerable<ValueSet> GetValueSets(params IContent[] contents)
    {
        foreach (IContent content in contents.Where(CanIndex))
        {
            // mandatory index values go here
            var indexValues = new Dictionary<string, object>
            {
                ["id"] = content.Id, // required for unpublishing/deletion handling
                [UmbracoExamineFieldNames.IndexPathFieldName] = content.Path, // required for unpublishing/deletion handling
                [UmbracoExamineFieldNames.NodeNameFieldName] = content.PublishName ?? string.Empty, // primarily needed for backoffice index browsing
            };

            // add custom field values from index handlers (selectors, filters, sorts)
            IndexFieldValue[] fieldValues = _contentIndexHandlerCollection
                .SelectMany(handler => handler.GetFieldValues(content))
                .DistinctBy(fieldValue => fieldValue.FieldName, StringComparer.OrdinalIgnoreCase)
                .Where(fieldValue => indexValues.ContainsKeyIgnoreCase(fieldValue.FieldName) is false)
                .ToArray();
            foreach (IndexFieldValue fieldValue in fieldValues)
            {
                indexValues[fieldValue.FieldName] = fieldValue.Value;
            }

            // NOTE: must use content.Id here, not content.Key - otherwise automatic clean-up i.e. on deletion or unpublishing will not work
            yield return new ValueSet(content.Id.ToString(), IndexTypes.Content, content.ContentType.Alias, indexValues);
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
