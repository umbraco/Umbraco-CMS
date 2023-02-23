using Examine;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Infrastructure.Examine;

public class ContentApiValueSetBuilder : BaseValueSetBuilder<IContent>
{
    public ContentApiValueSetBuilder(PropertyEditorCollection propertyEditors)
    : base(propertyEditors, false) //published values only?
    {
    }

    /// <inheritdoc />
    public override IEnumerable<ValueSet> GetValueSets(params IContent[] contents)
    {
        foreach (IContent content in contents)
        {
            var indexValues = new Dictionary<string, object>
            {
                ["id"] = content.Key,
                ["path"] = content.Path,
            };

            yield return new ValueSet(content.Id.ToString(), IndexTypes.Content, content.ContentType.Alias, indexValues);
        }
    }
}
