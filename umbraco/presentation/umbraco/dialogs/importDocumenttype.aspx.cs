using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.web;
using System.Xml;

namespace umbraco.presentation.umbraco.dialogs
{
	/// <summary>
	/// Summary description for importDocumentType.
	/// </summary>
	public class importDocumentType : BasePages.UmbracoEnsuredPage
	{
		protected System.Web.UI.WebControls.Literal FeedBackMessage;
		protected System.Web.UI.WebControls.Literal jsShowWindow;
		protected System.Web.UI.WebControls.Panel Wizard;
		protected System.Web.UI.HtmlControls.HtmlTable Table1;
		protected System.Web.UI.HtmlControls.HtmlInputHidden tempFile;
		protected System.Web.UI.HtmlControls.HtmlInputFile documentTypeFile;
		protected System.Web.UI.WebControls.Button submit;
		protected System.Web.UI.WebControls.Panel Confirm;
		protected System.Web.UI.WebControls.Literal dtName;
		protected System.Web.UI.WebControls.Literal dtAlias;
		protected System.Web.UI.WebControls.Button import;
		protected System.Web.UI.WebControls.Literal dtNameConfirm;
		protected System.Web.UI.WebControls.Panel done;
		private string tempFileName = "";

		private void Page_Load(object sender, System.EventArgs e)
		{
			if (!IsPostBack) 
			{
				submit.Text = ui.Text("import");
				import.Text = ui.Text("import");
			} 
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.submit.Click += new System.EventHandler(this.submit_Click);
			this.import.Click += new System.EventHandler(this.import_Click);
			this.Load += new System.EventHandler(this.Page_Load);

		}
		#endregion

		private void import_Click(object sender, System.EventArgs e)
		{
			XmlDocument xd = new XmlDocument();
			xd.Load(Server.MapPath(tempFile.Value));
			cms.businesslogic.packager.Installer.ImportDocumentType(xd.DocumentElement, base.getUser(), true);
			dtNameConfirm.Text = xd.DocumentElement.SelectSingleNode("/DocumentType/Info/Name").FirstChild.Value;

			Wizard.Visible = false;
			Confirm.Visible = false;
			done.Visible = true;
		}

		private void submit_Click(object sender, System.EventArgs e)
		{
			tempFileName = "justDelete_" + Guid.NewGuid().ToString() + ".udt";
			string fileName = GlobalSettings.StorageDirectory + System.IO.Path.DirectorySeparatorChar + tempFileName;
			tempFile.Value = fileName;
			documentTypeFile.PostedFile.SaveAs(Server.MapPath(fileName));

			XmlDocument xd = new XmlDocument();
			xd.Load(Server.MapPath(fileName));
			dtName.Text = xd.DocumentElement.SelectSingleNode("/DocumentType/Info/Name").FirstChild.Value;
			dtAlias.Text = xd.DocumentElement.SelectSingleNode("/DocumentType/Info/Alias").FirstChild.Value;


			Wizard.Visible = false;
			done.Visible = false;
			Confirm.Visible = true;

		}

	}
}
