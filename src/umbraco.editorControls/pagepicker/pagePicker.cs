using System;
using System.Web;
using System.Web.UI;

using umbraco.cms.presentation.Trees;
using ClientDependency.Core;
using umbraco.presentation;
using ClientDependency.Core.Controls;
using umbraco.interfaces;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using umbraco.editorControls.pagepicker;
using umbraco.uicontrols;
using umbraco.uicontrols.TreePicker;
namespace umbraco.editorControls
{
    /// <summary>
    /// Summary description for pagePicker.
    /// </summary>
    [ValidationProperty("Value")]
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class pagePicker : BaseTreePickerEditor
    {

        public pagePicker() : base() { }
        public pagePicker(IData data) : base(data) { }
        
        public override string TreePickerUrl
        {
            get
            {
                if (Context.Request.QueryString["id"] != null)
                {
                    return TreeUrlGenerator.GetPickerUrl(Umbraco.Core.Constants.Applications.Content, "content") + "&selected=" + Context.Request.QueryString["id"];
                }

                return TreeUrlGenerator.GetPickerUrl(Umbraco.Core.Constants.Applications.Content, "content");
            }
        }

        public override string ModalWindowTitle
        {
            get
            {
                return ui.GetText("general", "choose") + " " + ui.GetText("sections", "content");
            }
        }
        
    }
}
