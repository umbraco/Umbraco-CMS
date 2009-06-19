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

using System.Xml;
using umbraco.cms.helpers;

namespace umbraco.cms.presentation
{
	/// <summary>
	/// Summary description for editMedia.
	/// </summary>
	public partial class editMedia : BasePages.UmbracoEnsuredPage
	{
		protected uicontrols.TabView TabView1;
		protected System.Web.UI.WebControls.TextBox documentName;
		private cms.businesslogic.media.Media _document;
		controls.ContentControl tmp;

		//protected System.Web.UI.WebControls.Literal SyncPath;
		
		protected void Page_Load(object sender, System.EventArgs e)
		{
			//if (!IsPostBack) 
			//{
			//    SyncPath.Text = _document.Path;
			//    newName.Text = _document.Text.Replace("'", "\\'");
			//}
			if (!IsPostBack)
			{
				ClientTools.SyncTree(_document.Path, false);
			}			
		}

		protected void Save(object sender, System.EventArgs e) 
		{
            // error handling test
            if (!Page.IsValid)
            {
                foreach (uicontrols.TabPage tp in tmp.GetPanels())
                {
                    tp.ErrorControl.Visible = true;
                    tp.ErrorHeader = ui.Text("errorHandling", "errorHeader");
                    tp.CloseCaption = ui.Text("close");
                }
            }
            else if (Page.IsPostBack)
            {
                // hide validation summaries
                foreach (uicontrols.TabPage tp in tmp.GetPanels())
                {
                    tp.ErrorControl.Visible = false;
                }
            }
            _document.Save();
			_document.XmlGenerate(new XmlDocument());
			ClientTools.SyncTree(_document.Path, true);
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);

			_document = new cms.businesslogic.media.Media(int.Parse(Request.QueryString["id"]));

            // Save media on first load
            bool exists = SqlHelper.ExecuteScalar<int>("SELECT COUNT(nodeId) FROM cmsContentXml WHERE nodeId = @nodeId",
                                       SqlHelper.CreateParameter("@nodeId", _document.Id)) > 0;
            if (!exists)
                _document.XmlGenerate(new XmlDocument());


			tmp = new controls.ContentControl(_document,controls.ContentControl.publishModes.NoPublish, "TabView1");
			tmp.Width = Unit.Pixel(666);
			tmp.Height = Unit.Pixel(666);
			plc.Controls.Add(tmp);

			tmp.Save += new System.EventHandler(Save);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    

		}
		#endregion
	}
}
