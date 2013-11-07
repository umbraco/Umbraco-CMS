using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Used to dynamically change the tag group based on the pre-values
    /// </summary>
    internal class TagPropertyEditorTagDefinition : TagPropertyDefinition
    {
        public TagPropertyEditorTagDefinition(ContentPropertyData propertySaving, SupportTagsAttribute tagsAttribute)
            : base(propertySaving, tagsAttribute)
        {
        }

        public override string TagGroup
        {
            get
            {
                var preVals = PropertySaving.PreValues.FormatAsDictionary();
                return preVals.ContainsKey("group") ? preVals["group"].Value : "default";
            }
        }
    }
}