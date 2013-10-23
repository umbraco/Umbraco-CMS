using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [SupportTags]
    [PropertyEditor(Constants.PropertyEditors.TagsAlias, "Tags", "tags")]
    public class TagsPropertyEditor : PropertyEditor
    {
    }
}