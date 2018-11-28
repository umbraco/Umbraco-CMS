using Examine;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Examine
{

    public class MemberValueSetBuilder : BaseValueSetBuilder<IMember>
    {
        public MemberValueSetBuilder(PropertyEditorCollection propertyEditors)
            : base(propertyEditors)
        {
        }

        /// <inheritdoc />
        public override IEnumerable<ValueSet> GetValueSets(params IMember[] members)
        {
            foreach (var m in members)
            {
                var values = new Dictionary<string, object[]>
                {
                    {"icon", new object[] {m.ContentType.Icon}},
                    {"id", new object[] {m.Id}},
                    {"key", new object[] {m.Key}},
                    {"parentID", new object[] {m.Level > 1 ? m.ParentId : -1}},
                    {"level", new object[] {m.Level}},
                    {"creatorID", new object[] {m.CreatorId}},
                    {"sortOrder", new object[] {m.SortOrder}},
                    {"createDate", new object[] {m.CreateDate}},
                    {"updateDate", new object[] {m.UpdateDate}},
                    {"nodeName", new object[] {m.Name}},
                    {"path", new object[] {m.Path}},
                    {"nodeType", new object[] {m.ContentType.Id}},
                    {"loginName", new object[] {m.Username}},
                    {"email", new object[] {m.Email}},
                };

                foreach (var property in m.Properties)
                {
                    AddPropertyValue(property, null, null, values);
                }

                var vs = new ValueSet(m.Id.ToInvariantString(), IndexTypes.Content, m.ContentType.Alias, values);

                yield return vs;
            }
        }
    }

}
