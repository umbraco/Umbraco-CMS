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
using umbraco.uicontrols.TreePicker;
namespace umbraco.editorControls
{
    /// <summary>
    /// Summary description for pagePicker.
    /// </summary>
    [ValidationProperty("Value")]
    public class pagePicker : BaseTreePickerEditor
    {

        public pagePicker() : base() { }
        public pagePicker(interfaces.IData data) : base(data) { }
        
        public override string TreePickerUrl
        {
            get
            {
                if(HttpContext.Current.Request.QueryString["id"] != null)
                    return TreeService.GetPickerUrl(Umbraco.Core.Constants.Applications.Content, "content") + "&selected=" + HttpContext.Current.Request.QueryString["id"];

                return TreeService.GetPickerUrl(Umbraco.Core.Constants.Applications.Content, "content");
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
