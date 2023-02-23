using Examine;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

public class ContentApiValueSetBuilder : BaseValueSetBuilder<IContent>
{
    private readonly IEntityService _entityService;

    public ContentApiValueSetBuilder(PropertyEditorCollection propertyEditors, IEntityService entityService)
    : base(propertyEditors, false)
        => _entityService = entityService;

    /// <inheritdoc />
    public override IEnumerable<ValueSet> GetValueSets(params IContent[] contents)
    {
        foreach (IContent content in contents)
        {
            IEnumerable<int>? ancestors = content.GetAncestorIds();
            IEnumerable<Guid> ancestorKeys = Enumerable.Empty<Guid>();
            if (ancestors is not null)
            {
                ancestorKeys = ancestors.Select(GetContentKey);
            }

            var indexValues = new Dictionary<string, object>
            {
                ["id"] = content.Key,
                ["parentKey"] = ancestorKeys.LastOrDefault(),
                ["ancestorKeys"] = string.Join(",", ancestorKeys) // ToDo: Store as array if it is faster to search
            };

            yield return new ValueSet(content.Id.ToString(), IndexTypes.Content, content.ContentType.Alias, indexValues);
        }
    }

    private Guid GetContentKey(int id)
    {
        Attempt<Guid> guidAttempt = _entityService.GetKey(id, UmbracoObjectTypes.Document);
        Guid key = guidAttempt.Success ? guidAttempt.Result : Guid.Empty;

        return key;
    }
}
