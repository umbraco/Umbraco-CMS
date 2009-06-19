using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;

[assembly: WebResource("umbraco.presentation.umbraco.controls.Extenders.CustomFloatingBehavior.js", "text/javascript")]

namespace umbraco.presentation.controls.Extenders
{
    [Designer(typeof(CustomFloatingBehaviorDesigner))]
    [ClientScriptResource("umbraco.presentation.umbraco.controls.Extenders.CustomFloatingBehavior", "umbraco.presentation.umbraco.controls.Extenders.CustomFloatingBehavior.js")]
    [TargetControlType(typeof(WebControl))]
    [RequiredScript(typeof(DragDropScripts))]
    public class CustomFloatingBehaviorExtender : ExtenderControlBase
    {
        [ExtenderControlProperty]
        [IDReferenceProperty(typeof(WebControl))]
        public string DragHandleID
        {
            get
            {
                return GetPropertyValue<String>("DragHandleID", string.Empty);
            }
            set
            {
                SetPropertyValue<String>("DragHandleID", value);
            }
        }
    }
}
