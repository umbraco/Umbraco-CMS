using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using umbraco.BusinessLogic;
using umbraco.DataLayer;

namespace umbraco.editorControls.ultraSimpleMailer
{
	/// <summary>
	/// Summary description for mailerConfiguratorPreValueEditor.
	/// </summary>
	public class mailerConfiguratorPreValueEditor : umbraco.editorControls.tinymce.tinyMCEPreValueConfigurator
	{
	
		// UI controls
		private TextBox _textboxEmail;
		private TextBox _textboxSender;
		private DropDownList _dropdownlistMG;
				
		// referenced datatype
		private cms.businesslogic.datatype.BaseDataType _datatype;

        public static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

		public mailerConfiguratorPreValueEditor(cms.businesslogic.datatype.BaseDataType DataType) : base(DataType)
		{
			// state it knows its datatypedefinitionid
			_datatype = DataType;
			setupChildControls();

		}
		
		private void setupChildControls() 
		{
			
			_dropdownlistMG = new DropDownList();
			_dropdownlistMG.ID = "memberGroup";

			_textboxSender = new TextBox();
			_textboxSender.ID = "SenderName";
			_textboxEmail = new TextBox();
			_textboxEmail.ID = "SenderEmail";

			// put the childcontrols in context - ensuring that
			// the viewstate is persisted etc.
			Controls.Add(_dropdownlistMG);
			Controls.Add(_textboxSender);
			Controls.Add(_textboxEmail);


			// Get all membergroups
			foreach(cms.businesslogic.member.MemberGroup mg in cms.businesslogic.member.MemberGroup.GetAll)
				_dropdownlistMG.Items.Add(new ListItem(mg.Text, mg.Id.ToString()));
		}
		
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad (e);
			if (!Page.IsPostBack)
			{
				string[] config = Configuration.Split("|".ToCharArray());
				if (config.Length > 9) 
				{
					_textboxSender.Text = config[9];
					_textboxEmail.Text = config[10];
					_dropdownlistMG.SelectedValue = config[11];
				}

			}
		}
		
		public Control Editor 
		{
			get
			{
				return this;
			}
		}

	    public override void Save() 
		{
            base.Save();

			// Generate data-string
			string data = Configuration + "|" + _textboxSender.Text + "|"+ _textboxEmail.Text + "|" + _dropdownlistMG.SelectedValue;
			// If the add new prevalue textbox is filled out - add the value to the collection.
			IParameter[] SqlParams = new IParameter[] {
									SqlHelper.CreateParameter("@value",data),
									SqlHelper.CreateParameter("@dtdefid",_datatype.DataTypeDefinitionId)};
			SqlHelper.ExecuteNonQuery("delete from cmsDataTypePreValues where datatypenodeid = @dtdefid",SqlParams);
			SqlHelper.ExecuteNonQuery("insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,'')",SqlParams);
		}

		protected override void Render(HtmlTextWriter writer)
		{
			writer.WriteLine("<table>");
			writer.Write("<tr><th>Sender name:</th><td>");
			_textboxSender.RenderControl(writer);
			writer.Write("</td></tr>");
			writer.Write("<tr><th>Sender email:</th><td>");
			_textboxEmail.RenderControl(writer);
			writer.Write("</td></tr>");
			writer.Write("<tr><th>Membergroup to recieve mail:</th><td>");
			_dropdownlistMG.RenderControl(writer);
			writer.Write("</td></tr>");
			writer.Write("</table>");
            base.Render(writer);
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

	}
}
