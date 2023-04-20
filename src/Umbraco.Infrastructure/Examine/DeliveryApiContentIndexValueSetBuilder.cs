using Examine;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

public class DeliveryApiContentIndexValueSetBuilder : IDeliveryApiContentIndexValueSetBuilder
{
    private readonly ContentIndexHandlerCollection _contentIndexHandlerCollection;

    public DeliveryApiContentIndexValueSetBuilder(ContentIndexHandlerCollection contentIndexHandlerCollection)
        => _contentIndexHandlerCollection = contentIndexHandlerCollection;

    /// <inheritdoc />
    public IEnumerable<ValueSet> GetValueSets(params IContent[] contents)
    {
        foreach (IContent content in contents.Where(c => c.Published))
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
}
