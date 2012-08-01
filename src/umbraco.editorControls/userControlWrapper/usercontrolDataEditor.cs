using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using umbraco.interfaces;
using umbraco.editorControls;
using umbraco.cms.businesslogic.datatype;
using System.Collections.Generic;
using umbraco.IO;

namespace umbraco.editorControls.userControlGrapper
{
    [ValidationProperty("Value")]
    public class usercontrolDataEditor : System.Web.UI.WebControls.PlaceHolder, umbraco.interfaces.IDataEditor
	{
		private umbraco.interfaces.IData _data;
        private string _usercontrolPath;

        public object Value
        {
            get
            {
                return ((IUsercontrolDataEditor)Controls[0]).value;
            }
        }

        public usercontrolDataEditor(umbraco.interfaces.IData Data, string UsercontrolPath)
        {
			_data = Data;
            _usercontrolPath = UsercontrolPath;
		}

		public virtual bool TreatAsRichTextEditor 
		{
			get {return false;}
		}

		public bool ShowLabel 
		{
			get {return true;}
		}

		public Control Editor {get{return this;}}

		public void Save() 
		{
            IUsercontrolDataEditor uc =
                (IUsercontrolDataEditor)Controls[0] as IUsercontrolDataEditor;

			_data.Value = uc.value;
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit (e);

            Control oControl = new System.Web.UI.UserControl().LoadControl(_usercontrolPath);

            if (HasSettings(oControl.GetType()))
            {
                DataEditorSettingsStorage ss = new DataEditorSettingsStorage();
                List<Setting<string, string>> s = ss.GetSettings(((umbraco.cms.businesslogic.datatype.DefaultData)_data).DataTypeDefinitionId);
                ss.Dispose();

                foreach (Setting<string, string> setting in s)
                {
                    try
                    {
                        if(!string.IsNullOrEmpty(setting.Key))
                        {
                            oControl.GetType().InvokeMember(setting.Key, System.Reflection.BindingFlags.SetProperty, null, oControl, new object[] { setting.Value });
                        }

                    }
                    catch (MissingMethodException) { }
                }
                
            }

            this.Controls.Add(oControl);

            if (!Page.IsPostBack)
                ((IUsercontrolDataEditor)Controls[0] as IUsercontrolDataEditor).value = _data.Value;
              
		}

        private bool HasSettings(Type t)
        {
            bool hasSettings = false;
            foreach (System.Reflection.PropertyInfo p in t.GetProperties())
            {
                object[] o = p.GetCustomAttributes(typeof(DataEditorSetting), true);

                if (o.Length > 0)
                {
                    hasSettings = true;
                    break;
                }
            }

            return hasSettings;
        }

	}
}