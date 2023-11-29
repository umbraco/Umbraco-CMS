using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Indexing.Selectors;

public sealed class DescendantsSelectorIndexer : IContentIndexHandler
{
    internal const string FieldName = "ancestorIds";

    private readonly IEntityService _entityService;

    public DescendantsSelectorIndexer(IEntityService entityService)
        => _entityService = entityService;

    public IEnumerable<IndexFieldValue> GetFieldValues(IContent content, string? culture)
    {
        var ancestorKeys = content.GetAncestorIds()?.Select(id =>
        {
            Attempt<Guid> getKeyAttempt = _entityService.GetKey(id, UmbracoObjectTypes.Document);
            return getKeyAttempt.Success ? getKeyAttempt.Result : (object)Guid.Empty;
        }).ToArray() ?? Array.Empty<object>();

        yield return new IndexFieldValue
        {
            FieldName = FieldName,
            Values = ancestorKeys
        };
    }

    public IEnumerable<IndexField> GetFields()
        => new[] { new IndexField { FieldName = FieldName, FieldType = FieldType.StringRaw, VariesByCulture = false } };

}
