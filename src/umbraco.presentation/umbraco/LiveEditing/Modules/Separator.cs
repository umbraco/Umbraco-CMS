using System.Web.UI.HtmlControls;

namespace umbraco.presentation.LiveEditing.Modules
{
    /// <summary>
    /// Module separator item.
    /// </summary>
    public class Separator : HtmlGenericControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Separator"/> class.
        /// </summary>
        public Separator() : base("span")
        {
            Attributes["class"] = "separator";
            InnerHtml = @"<span>&nbsp;</span>";
        }
    }
}
