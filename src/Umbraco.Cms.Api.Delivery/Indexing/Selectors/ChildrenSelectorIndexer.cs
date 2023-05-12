using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Delivery.Indexing.Selectors;

public sealed class ChildrenSelectorIndexer : IContentIndexHandler
{
    private readonly IEntityService _entityService;

    public ChildrenSelectorIndexer(IEntityService entityService)
        => _entityService = entityService;

    internal const string FieldName = "parentId";

    public IEnumerable<IndexFieldValue> GetFieldValues(IContent content, string? culture)
    {
        Guid parentKey = Guid.Empty;
        if (content.ParentId > 0)
        {
            Attempt<Guid> getKeyAttempt = _entityService.GetKey(content.ParentId, UmbracoObjectTypes.Document);
            parentKey = getKeyAttempt.Success ? getKeyAttempt.Result : parentKey;
        }

        yield return new IndexFieldValue
        {
            FieldName = FieldName,
            Values = new object[] { parentKey }
        };
    }

    public IEnumerable<IndexField> GetFields()
        => new[] { new IndexField { FieldName = FieldName, FieldType = FieldType.StringRaw, VariesByCulture = false } };
}
