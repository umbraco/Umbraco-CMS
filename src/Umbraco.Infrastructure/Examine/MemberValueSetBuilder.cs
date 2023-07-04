using Examine;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;
[Obsolete("This class will be removed in v14, please check documentation of specific search provider", true)]

public class MemberValueSetBuilder : BaseValueSetBuilder<IMember>
{
    public MemberValueSetBuilder(PropertyEditorCollection propertyEditors)
        : base(propertyEditors, false)
    {
    }

    /// <inheritdoc />
    public override IEnumerable<ValueSet> GetValueSets(params IMember[] members)
    {
        foreach (IMember m in members)
        {
            var values = new Dictionary<string, IEnumerable<object?>>
            {
                { "icon", m.ContentType.Icon?.Yield() ?? Enumerable.Empty<string>() },
                { "id", new object[] { m.Id } },
                { UmbracoExamineFieldNames.NodeKeyFieldName, new object[] { m.Key } },
                { "parentID", new object[] { m.Level > 1 ? m.ParentId : -1 } },
                { "level", new object[] { m.Level } },
                { "creatorID", new object[] { m.CreatorId } },
                { "sortOrder", new object[] { m.SortOrder } },
                { "createDate", new object[] { m.CreateDate } },
                { "updateDate", new object[] { m.UpdateDate } },
                { UmbracoExamineFieldNames.NodeNameFieldName, m.Name?.Yield() ?? Enumerable.Empty<string>() },
                { "path", m.Path.Yield() },
                { "nodeType", m.ContentType.Id.ToString().Yield() },
                { "loginName", m.Username.Yield() },
                { "email", m.Email.Yield() },
            };

            foreach (IProperty property in m.Properties)
            {
                AddPropertyValue(property, null, null, values, m.AvailableCultures);
            }

            var vs = new ValueSet(m.Id.ToInvariantString(), IndexTypes.Member, m.ContentType.Alias, values);

            yield return vs;
        }
    }
}
