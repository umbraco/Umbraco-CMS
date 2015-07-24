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

using System.IO;
using umbraco.BusinessLogic;
using Umbraco.Core.IO;
using Umbraco.Core;

namespace umbraco.dialogs
{
	[Obsolete("Use the ImageViewer user control instead")]
	public partial class imageViewer : BasePages.UmbracoEnsuredPage
	{

	    public imageViewer()
	    {
	        CurrentApp = DefaultApps.media.ToString();
	    }
	
		protected void Page_Load(object sender, EventArgs e)
		{
			//Response.Write(umbraco.helper.Request("id"));
			//Response.End();
			// Put user code to initialize the page here
			if (Request.QueryString["id"] != null) 
			{
				if (Request.QueryString["id"] != "")  
				{
					//TODO: fix Nasty FAST'N'CANELINE HACK. ..
					var mediaId = int.Parse(Request.QueryString["id"]);
					
					image.Controls.Clear();
					var fileWidth = 0;
					var fileHeight = 0;
					var fileName = "/blank.gif";
					var altText = "";

				    try
				    {
				        var m = new cms.businesslogic.media.Media(mediaId);

				        // TODO: Remove "Magic strings" from code.
				        try
				        {
				            fileName = m.getProperty("fileName").Value.ToString();
				        }
				        catch
				        {
				            try
				            {
								fileName = m.getProperty(Constants.Conventions.Media.File).Value.ToString();
				            }
				            catch
				            {
				                fileName = m.getProperty("file").Value.ToString();
				            }
				        }

				        altText = m.Text;
				        try
				        {
								fileWidth = int.Parse(m.getProperty(Constants.Conventions.Media.Width).Value.ToString());
								fileHeight = int.Parse(m.getProperty(Constants.Conventions.Media.Height).Value.ToString());
				        }
				        catch
				        {

				        }
				        var fileNameOrg = fileName;
				        var ext = fileNameOrg.Substring(fileNameOrg.LastIndexOf(".") + 1, fileNameOrg.Length - fileNameOrg.LastIndexOf(".") - 1);
                        var fileNameThumb = SystemDirectories.Root + fileNameOrg.Replace("." + ext, "_thumb." + ext);
				        image.Controls.Add(new LiteralControl("<a href=\"" + SystemDirectories.Root + fileNameOrg + "\" title=\"Zoom\"><img src=\"" + fileNameThumb + "\" border=\"0\"/></a>"));
				    }
				    catch
				    {
				    }

				    image.Controls.Add(new LiteralControl("<script>\nparent.updateImageSource('" + SystemDirectories.Root  + fileName.Replace("'", "\\'") + "','" + altText + "','" + fileWidth.ToString() + "','" + fileHeight.ToString() + "')\n</script>"));

				}
			}
		}

	}
}
