using System;
using System.Configuration;
using System.Drawing;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;
using Umbraco.Core;
using umbraco.DataLayer;
using umbraco.BusinessLogic;

using umbraco.editorControls;
using Umbraco.Core.IO;
using System.Collections.Generic;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.userControlGrapper
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class usercontrolPrevalueEditor : System.Web.UI.WebControls.PlaceHolder, umbraco.interfaces.IDataPrevalue
	{
		public ISqlHelper SqlHelper
		{
			get { return Application.SqlHelper; }
		}

		#region IDataPrevalue Members

		// referenced datatype
		private umbraco.cms.businesslogic.datatype.BaseDataType _datatype;

		private DropDownList _dropdownlist;
		private DropDownList _dropdownlistUserControl;
		private PlaceHolder _phSettings;

		private Dictionary<string, DataEditorSettingType> dtSettings = new Dictionary<string, DataEditorSettingType>();

		public usercontrolPrevalueEditor(umbraco.cms.businesslogic.datatype.BaseDataType DataType) 
		{
			// state it knows its datatypedefinitionid
			_datatype = DataType;
			setupChildControls();

		}
		
		private void setupChildControls() 
		{
			_dropdownlist = new DropDownList();
			_dropdownlist.ID = "dbtype";
			_dropdownlist.Items.Add(DBTypes.Date.ToString());
			_dropdownlist.Items.Add(DBTypes.Integer.ToString());
			_dropdownlist.Items.Add(DBTypes.Ntext.ToString());
			_dropdownlist.Items.Add(DBTypes.Nvarchar.ToString());
			
			_dropdownlistUserControl = new DropDownList();
			_dropdownlistUserControl.ID = "usercontrol";

			_phSettings = new PlaceHolder();
			_phSettings.ID = "settings";

			// put the childcontrols in context - ensuring that
			// the viewstate is persisted etc.
			Controls.Add(_dropdownlist);
			Controls.Add(_dropdownlistUserControl);
			Controls.Add(_phSettings);

			// populate the usercontrol dropdown
			_dropdownlistUserControl.Items.Add(new ListItem(ui.Text("choose"), ""));
			populateUserControls( IOHelper.MapPath( SystemDirectories.UserControls) );

		   
		}

		private void populateUserControls(string path)
		{
			DirectoryInfo di = new DirectoryInfo(path);
            if (di.Exists == false) return;

			foreach (FileInfo uc in di.GetFiles("*.ascx"))
			{
                string ucRoot = IOHelper.MapPath(SystemDirectories.UserControls);

				_dropdownlistUserControl.Items.Add(

                    new ListItem(SystemDirectories.UserControls +
                            uc.FullName.Substring(ucRoot.Length).Replace(IOHelper.DirSepChar, '/'))

					/*
					new ListItem( 
						uc.FullName.Substring( uc.FullName.IndexOf(root), uc.FullName.Length - uc.FullName.IndexOf(root)).Replace(IOHelper.DirSepChar, '/'))
					  */  
						);

			}
			foreach (DirectoryInfo dir in di.GetDirectories())
				populateUserControls(dir.FullName);
		}

		public Control Editor
		{
			get
			{
				return this;
			}
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!Page.IsPostBack)
			{
				string config = Configuration;
				if (config != "")
				{
					_dropdownlistUserControl.SelectedValue = config;

					
				}
				_dropdownlist.SelectedValue = _datatype.DBType.ToString();


				
			}

			//check for settings
			if (!string.IsNullOrEmpty(Configuration))
				LoadSetttings(Configuration);
		   

		}

		private Dictionary<string, DataEditorSetting> GetSettings(Type t)
		{
			Dictionary<string, DataEditorSetting> settings = new Dictionary<string, DataEditorSetting>();

			foreach (System.Reflection.PropertyInfo p in t.GetProperties())
			{

				object[] o = p.GetCustomAttributes(typeof(DataEditorSetting), true);

				if (o.Length > 0)
					settings.Add(p.Name, (DataEditorSetting)o[0]);
			}
			return settings;
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

		private void LoadSetttings(string fileName)
		{
            // due to legacy, some user controls may not have the tilde start
            if (fileName.StartsWith("~/"))
                fileName = fileName.Substring(2);

			if (System.IO.File.Exists(IOHelper.MapPath("~/" + fileName)))
			{
			  
				UserControl oControl = (UserControl)this.Page.LoadControl(@"~/" + fileName);

				Type type = oControl.GetType();


				Dictionary<string, DataEditorSetting> settings = GetSettings(type);

				foreach (KeyValuePair<string, DataEditorSetting> kv in settings)
				{
					DataEditorSettingType dst = kv.Value.GetDataEditorSettingType();
					dtSettings.Add(kv.Key, dst);

					DataEditorPropertyPanel panel = new DataEditorPropertyPanel();
					panel.Text = kv.Value.GetName();
					panel.Text += "<br/><small>" + kv.Value.description + "</small>";


					if (HasSettings(type))
					{
						DataEditorSettingsStorage ss = new DataEditorSettingsStorage();

						List<Setting<string, string>> s = ss.GetSettings(_datatype.DataTypeDefinitionId);
						ss.Dispose();

						if (s.Find(set => set.Key == kv.Key).Value != null)
							dst.Value = s.Find(set => set.Key == kv.Key).Value;

					}

					panel.Controls.Add(dst.RenderControl(kv.Value));

					Label invalid = new Label();
					invalid.Attributes.Add("style", "color:#8A1F11");
					invalid.ID = "lbl" + kv.Key;
					panel.Controls.Add(invalid);

					_phSettings.Controls.Add(panel);
					
				}
			}
		}

		public void Save()
		{
			bool hasErrors = false;
			foreach (KeyValuePair<string, DataEditorSettingType> k in dtSettings)
			{
				var result = k.Value.Validate();
                Label lbl = _phSettings.FindControlRecursive<Label>("lbl" + k.Key);
				if(result == null && lbl != null)
				{
					if(lbl != null)
						lbl.Text = string.Empty;
				}
				else
				{
					if(hasErrors == false)
						hasErrors = true;

					if (lbl != null)
						lbl.Text = " " + result.ErrorMessage;
				}
			}

			if (!hasErrors)
			{
				_datatype.DBType =
					(umbraco.cms.businesslogic.datatype.DBTypes)
					Enum.Parse(typeof (umbraco.cms.businesslogic.datatype.DBTypes), _dropdownlist.SelectedValue, true);

				// Generate data-string
				string data = _dropdownlistUserControl.SelectedValue;

				// If the add new prevalue textbox is filled out - add the value to the collection.
				IParameter[] SqlParams = new IParameter[]
											 {
												 SqlHelper.CreateParameter("@value", data),
												 SqlHelper.CreateParameter("@dtdefid", _datatype.DataTypeDefinitionId)
											 };
				SqlHelper.ExecuteNonQuery("delete from cmsDataTypePreValues where datatypenodeid = @dtdefid", SqlParams);
				// we need to populate the parameters again due to an issue with SQL CE
				SqlParams = new IParameter[]
								{
									SqlHelper.CreateParameter("@value", data),
									SqlHelper.CreateParameter("@dtdefid", _datatype.DataTypeDefinitionId)
								};
				SqlHelper.ExecuteNonQuery(
					"insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,'')",
					SqlParams);


				//settings

				DataEditorSettingsStorage ss = new DataEditorSettingsStorage();

				//ss.ClearSettings(_datatype.DataTypeDefinitionId);

				int i = 0;
				foreach (KeyValuePair<string, DataEditorSettingType> k in dtSettings)
				{
					ss.InsertSetting(_datatype.DataTypeDefinitionId, k.Key, k.Value.Value, i);
					i++;

				}

				ss.Dispose();

				if (dtSettings.Count == 0)
				{
					if (!string.IsNullOrEmpty(Configuration))
						LoadSetttings(Configuration);
				}

			}
		}

		protected override void Render(HtmlTextWriter writer)
		{
			writer.WriteLine("<div class=\"propertyItem\">");
			writer.WriteLine("<div class=\"propertyItemheader\">Database datatype</div>");
			writer.WriteLine("<div class=\"propertyItemContent\">");
			_dropdownlist.RenderControl(writer);
			writer.Write("</div></div>");

			writer.WriteLine("<div class=\"propertyItem\">");
			writer.WriteLine("<div class=\"propertyItemheader\">Usercontrol:</div>");
			writer.WriteLine("<div class=\"propertyItemContent\">");
			_dropdownlistUserControl.RenderControl(writer);
			writer.Write("</div></div>");

			_phSettings.RenderControl(writer);
		}

		public string Configuration
		{
			get
			{
				object conf =
					SqlHelper.ExecuteScalar<object>("select value from cmsDataTypePreValues where datatypenodeid = @datatypenodeid",
											SqlHelper.CreateParameter("@datatypenodeid", _datatype.DataTypeDefinitionId));
				if (conf != null)
					return conf.ToString();
				else
					return "";

			}
		}

		#endregion
	}
}
