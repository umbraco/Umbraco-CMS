using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Search;
using Umbraco.Extensions;

namespace Umbraco.Search.ValueSet.ValueSetBuilders;

public class MemberValueSetBuilder : BaseValueSetBuilder<IMember>
{
    public MemberValueSetBuilder(PropertyEditorCollection propertyEditors)
        : base(propertyEditors, false)
    {
    }

    /// <inheritdoc />
    public override IEnumerable<UmbracoValueSet> GetValueSets(params IMember[] members)
    {
        foreach (IMember m in members)
        {
            var values = new Dictionary<string, IEnumerable<object?>>
            {
                { "icon", m.ContentType.Icon?.Yield() ?? Enumerable.Empty<string>() },
                { "id", new object[] { m.Id } },
                { UmbracoSearchFieldNames.NodeKeyFieldName, new object[] { m.Key } },
                { "parentID", new object[] { m.Level > 1 ? m.ParentId : -1 } },
                { "level", new object[] { m.Level } },
                { "creatorID", new object[] { m.CreatorId } },
                { "sortOrder", new object[] { m.SortOrder } },
                { "createDate", new object[] { m.CreateDate } },
                { "updateDate", new object[] { m.UpdateDate } },
                { UmbracoSearchFieldNames.NodeNameFieldName, m.Name?.Yield() ?? Enumerable.Empty<string>() },
                { "path", m.Path.Yield() },
                { "searchablePath", m.Path?.Split(',') ?? Enumerable.Empty<string>()},
                { "nodeType", m.ContentType.Id.ToString().Yield() },
                { "loginName", m.Username.Yield() },
                { "email", m.Email.Yield() },
            };

            foreach (IProperty property in m.Properties)
            {
                AddPropertyValue(property, null, null, values);
            }

            var vs = new UmbracoValueSet(m.Id.ToInvariantString(), IndexTypes.Member, m.ContentType.Alias, values!);

            yield return vs;
        }
    }
}
