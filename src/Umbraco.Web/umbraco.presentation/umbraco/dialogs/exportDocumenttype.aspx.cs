using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using umbraco.cms.businesslogic.web;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace umbraco.presentation.dialogs
{
	/// <summary>
	/// Summary description for exportDocumenttype.
	/// </summary>
	public class exportDocumenttype : BasePages.UmbracoEnsuredPage
	{
	    public exportDocumenttype()
	    {
            CurrentApp = BusinessLogic.DefaultApps.settings.ToString();

	    }
		private void Page_Load(object sender, System.EventArgs e)
		{
			int documentTypeId = Request.GetItemAs<int>("nodeID");
			if (documentTypeId > 0)
			{
			    var contentType = Services.ContentTypeService.GetContentType(documentTypeId);
                if (contentType == null) throw new NullReferenceException("No content type found with id " + documentTypeId);
                
                Response.AddHeader("Content-Disposition", "attachment;filename=" + contentType.Alias + ".udt");
                Response.ContentType = "application/octet-stream";

			    var serializer = new EntityXmlSerializer();
			    var xml = serializer.Serialize(
                    Services.DataTypeService,
                    Services.ContentTypeService,
                    contentType);

                xml.Save(Response.OutputStream);
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
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
