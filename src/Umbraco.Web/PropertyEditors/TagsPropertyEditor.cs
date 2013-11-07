using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [SupportTags(typeof(TagPropertyEditorTagDefinition))]
    [PropertyEditor(Constants.PropertyEditors.TagsAlias, "Tags", "tags")]
    public class TagsPropertyEditor : PropertyEditor
    {
        public TagsPropertyEditor()
        {
            _defaultPreVals = new Dictionary<string, object>
                {
                    {"group", "default"}
                };
        }

        private IDictionary<string, object> _defaultPreVals;

        /// <summary>
        /// Override to supply the default group
        /// </summary>
        public override IDictionary<string, object> DefaultPreValues
        {
            get { return _defaultPreVals; }
            set { _defaultPreVals = value; }
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new TagPreValueEditor();
        }

        internal class TagPreValueEditor : PreValueEditor
        {
            public TagPreValueEditor()
            {
                Fields.Add(new PreValueField(new ManifestPropertyValidator { Type = "Required" })
                {
                    Description = "Define a tag group",
                    Key = "group",
                    Name = "Tag group",
                    View = "requiredfield"
                });
            }

            public override IDictionary<string, object> ConvertDbToEditor(IDictionary<string, object> defaultPreVals, Core.Models.PreValueCollection persistedPreVals)
            {
                var result = base.ConvertDbToEditor(defaultPreVals, persistedPreVals);
                return result;
            }
        }
    }
}