using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Indexing.Selectors;

internal sealed class DescendantsSelectorIndexer : IContentIndexHandler
{
    internal const string FieldName = "ancestorIds";

    private readonly IEntityService _entityService;

    public DescendantsSelectorIndexer(IEntityService entityService)
        => _entityService = entityService;

    public IEnumerable<IndexFieldValue> GetFieldValues(IContent content)
    {
        Guid[] ancestorKeys = content.GetAncestorIds()?.Select(id =>
        {
            Attempt<Guid> getKeyAttempt = _entityService.GetKey(id, UmbracoObjectTypes.Document);
            return getKeyAttempt.Success ? getKeyAttempt.Result : Guid.Empty;
        }).ToArray() ?? Array.Empty<Guid>();

        yield return new IndexFieldValue
        {
            FieldName = FieldName,
            Value = string.Join(" ", ancestorKeys) // TODO: investigate if search executes faster if we store this as an array
        };
    }

    public IEnumerable<IndexField> GetFields()
        => new[] { new IndexField { FieldName = FieldName, FieldType = FieldType.String } };

}
