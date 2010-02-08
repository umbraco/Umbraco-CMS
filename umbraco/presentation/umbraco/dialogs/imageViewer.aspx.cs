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
using umbraco.IO;

namespace umbraco.dialogs
{
	[Obsolete("Use the ImageViewer user control instead")]
	public partial class imageViewer : System.Web.UI.Page
	{
	
		protected void Page_Load(object sender, System.EventArgs e)
		{
			//Response.Write(umbraco.helper.Request("id"));
			//Response.End();
			// Put user code to initialize the page here
			if (Request.QueryString["id"] != null) 
			{
				if (Request.QueryString["id"] != "")  
				{
					//TODO: fix Nasty FAST'N'CANELINE HACK. ..
					int MediaId = int.Parse(Request.QueryString["id"]);
					
					image.Controls.Clear();
					int fileWidth = 0;
					int fileHeight = 0;
					string fileName = "/blank.gif";
					string altText = "";

					try 
					{
						cms.businesslogic.media.Media m = new cms.businesslogic.media.Media(MediaId);

						// TODO: Remove "Magic strings" from code.
						try 
						{
							fileName = m.getProperty("fileName").Value.ToString();
						} 
						catch 
						{
							try 
							{
								fileName = m.getProperty("umbracoFile").Value.ToString();
							} 
							catch 
							{
								fileName = m.getProperty("file").Value.ToString();
							}
						}

						altText = m.Text;
						try 
							{
							fileWidth = int.Parse(m.getProperty("umbracoWidth").Value.ToString());
							fileHeight = int.Parse(m.getProperty("umbracoHeight").Value.ToString());
							}
						catch {
						
						}
						string fileNameOrg = fileName;
						string ext = fileNameOrg.Substring(fileNameOrg.LastIndexOf(".")+1, fileNameOrg.Length-fileNameOrg.LastIndexOf(".")-1);
						string fileNameThumb = SystemDirectories.Root + fileNameOrg.Replace("."+ext, "_thumb.jpg");
						image.Controls.Add(new LiteralControl("<a href=\"" + SystemDirectories.Root + fileNameOrg + "\" title=\"Zoom\"><img src=\"" + fileNameThumb + "\" border=\"0\"/></a>"));
					} 
					catch {
					}

                    image.Controls.Add(new LiteralControl("<script>\nparent.updateImageSource('" + SystemDirectories.Root  + fileName.Replace("'", "\\'") + "','" + altText + "','" + fileWidth.ToString() + "','" + fileHeight.ToString() + "')\n</script>"));

				}
			}
		}

	}
}
