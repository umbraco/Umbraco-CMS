using System;
using System.Web.UI;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.datatype;
using System.Collections.Generic;

namespace umbraco.editorControls.userControlGrapper
{
    [ValidationProperty("Value")]
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
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
            _usercontrolPath = IOHelper.FindFile(UsercontrolPath);
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

            // Add property data to the usercontrol if it supports it
            // TODO: Find the best way to test for an interface!
		    IUsercontrolPropertyData propertyData = oControl as IUsercontrolPropertyData;
            if (propertyData != null)
            {
                propertyData.PropertyObject = new Property(((usercontrolData)_data).PropertyId);
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