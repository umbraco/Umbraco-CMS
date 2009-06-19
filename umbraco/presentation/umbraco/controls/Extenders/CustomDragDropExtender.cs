using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;

[assembly: WebResource("umbraco.presentation.umbraco.controls.Extenders.CustomDragDropBehavior.js", "text/javascript")]

namespace umbraco.presentation.controls.Extenders
{
    [Designer(typeof(CustomDragDropDesigner))]
    [ClientScriptResource("umbraco.presentation.umbraco.controls.Extenders.CustomDragDropBehavior", "umbraco.presentation.umbraco.controls.Extenders.CustomDragDropBehavior.js")]
    [TargetControlType(typeof(WebControl))]
    [RequiredScript(typeof(CommonToolkitScripts))]
    [RequiredScript(typeof(TimerScript))]
    //[RequiredScript(typeof(FloatingBehaviorScript))]
    [RequiredScript(typeof(DragDropScripts))]
    [RequiredScript(typeof(DragPanelExtender))]
    [RequiredScript(typeof(CustomFloatingBehaviorScript))]
    public class CustomDragDropExtender : ExtenderControlBase
    {
        [ExtenderControlProperty]
        public string DragItemClass
        {
            get
            {
                return GetPropertyValue<String>("DragItemClass", string.Empty);
            }
            set
            {
                SetPropertyValue<String>("DragItemClass", value);
            }
        }

        [ExtenderControlProperty]
        public string DragItemHandleClass
        {
            get
            {
                return GetPropertyValue<String>("DragItemHandleClass", string.Empty);
            }
            set
            {
                SetPropertyValue<String>("DragItemHandleClass", value);
            }
        }

        [ExtenderControlProperty]
        [IDReferenceProperty(typeof(WebControl))]
        public string DropCueID
        {
            get
            {
                return GetPropertyValue<String>("DropCueID", string.Empty);
            }
            set
            {
                SetPropertyValue<String>("DropCueID", value);
            }
        }

        [ExtenderControlProperty()]
        [DefaultValue("")]
        [ClientPropertyName("onDrop")]
        public string OnClientDrop
        {
            get
            {
                return GetPropertyValue<String>("OnClientDrop", string.Empty);
            }
            set
            {
                SetPropertyValue<String>("OnClientDrop", value);
            }
        }

    }
}
