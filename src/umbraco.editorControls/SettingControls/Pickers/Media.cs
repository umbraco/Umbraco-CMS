using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.datatype;
using Umbraco.Core;

namespace umbraco.editorControls.SettingControls.Pickers
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class Media : DataEditorSettingType
    {

        private umbraco.controls.ContentPicker mp = new umbraco.controls.ContentPicker();

        private string _val = string.Empty;
        public override string Value
        {
            get
            {
 
                return mp.Value;
            }
            set
            {
                int output;
                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out output))
                    _val = value;
            }
        }

        public override System.Web.UI.Control RenderControl(DataEditorSetting sender)
        {

            mp.ID = sender.GetName().Replace(" ", "_");

            mp.AppAlias = Constants.Applications.Media;
            mp.TreeAlias = "media";

            int output;
            if (!string.IsNullOrEmpty(_val) && int.TryParse(_val, out output))
            {
                mp.Value = _val;
               
            }
            return mp;
        }
        
    }

}