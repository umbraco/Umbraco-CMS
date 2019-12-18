using Examine;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Examine
{

    public class MemberValueSetBuilder : BaseValueSetBuilder<IMember>
    {
        public MemberValueSetBuilder(PropertyEditorCollection propertyEditors)
            : base(propertyEditors, false)
        {
        }

        /// <inheritdoc />
        public override IEnumerable<ValueSet> GetValueSets(params IMember[] members)
        {
            foreach (var m in members)
            {
                var values = new Dictionary<string, IEnumerable<object>>
                {
                    {"icon", m.ContentType.Icon?.Yield() ?? Enumerable.Empty<string>()},
                    {"id", new object[] {m.Id}},
                    {UmbracoExamineIndex.NodeKeyFieldName, new object[] {m.Key}},
                    {"parentID", new object[] {m.Level > 1 ? m.ParentId : -1}},
                    {"level", new object[] {m.Level}},
                    {"creatorID", new object[] {m.CreatorId}},
                    {"sortOrder", new object[] {m.SortOrder}},
                    {"createDate", new object[] {m.CreateDate}},
                    {"updateDate", new object[] {m.UpdateDate}},
                    {"nodeName", m.Name?.Yield() ?? Enumerable.Empty<string>()},
                    {"path", m.Path?.Yield() ?? Enumerable.Empty<string>()},
                    {"nodeType", m.ContentType.Id.ToString().Yield() },
                    {"loginName", m.Username?.Yield() ?? Enumerable.Empty<string>()},
                    {"email", m.Email?.Yield() ?? Enumerable.Empty<string>()},
                };

                foreach (var property in m.Properties)
                {
                    AddPropertyValue(property, null, null, values);
                }

                var vs = new ValueSet(m.Id.ToInvariantString(), IndexTypes.Member, m.ContentType.Alias, values);

                yield return vs;
            }
        }
    }

}
