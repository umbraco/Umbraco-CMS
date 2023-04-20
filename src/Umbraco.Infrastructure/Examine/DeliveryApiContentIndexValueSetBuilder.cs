using Examine;
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

    public DeliveryApiContentIndexValueSetBuilder(ContentIndexHandlerCollection contentIndexHandlerCollection, IScopeProvider scopeProvider, IPublicAccessService publicAccessService)
    {
        _contentIndexHandlerCollection = contentIndexHandlerCollection;
        _scopeProvider = scopeProvider;
        _publicAccessService = publicAccessService;
    }

    /// <inheritdoc />
    public IEnumerable<ValueSet> GetValueSets(params IContent[] contents)
    {
        foreach (IContent content in contents.Where(CanIndex))
        {
            // mandatory index values go here
            var indexValues = new Dictionary<string, object>
            {
                ["id"] = content.Key,
                [UmbracoExamineFieldNames.NodeNameFieldName] = content.PublishName ?? string.Empty
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

            yield return new ValueSet(content.Key.ToString(), IndexTypes.Content, content.ContentType.Alias, indexValues);
        }
    }

    private bool CanIndex(IContent content)
    {
        if (content.Published is false)
        {
            return false;
        }

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
