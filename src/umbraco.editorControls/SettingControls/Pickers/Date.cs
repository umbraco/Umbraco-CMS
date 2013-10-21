using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.uicontrols.DatePicker;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.SettingControls.Pickers
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class Date: DataEditorSettingType
    {
        private DateTimePicker dp = new DateTimePicker();

        private string _val = string.Empty;
        public override string Value
        {
            get
            {
                return dp.DateTime.ToString();
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _val = value;
            }
        }

        public override System.Web.UI.Control RenderControl(DataEditorSetting sender)
        {

            dp.ShowTime = false;

            dp.ID = sender.GetName().Replace(" ", "_");

            if(!string.IsNullOrEmpty(_val))
                dp.DateTime = Convert.ToDateTime(_val);

            return dp;
        }
    }
   
}