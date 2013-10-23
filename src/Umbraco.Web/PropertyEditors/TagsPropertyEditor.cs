using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    //STUB FOR FUTURE USE - need to declare one for the upgrade to work!
    [SupportTags]
    [PropertyEditor(Constants.PropertyEditors.TagsAlias, "Tags", "readonlyvalue")]
    public class TagsPropertyEditor : PropertyEditor
    {
    }
}