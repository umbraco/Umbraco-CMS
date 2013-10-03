using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.ComponentModel;
using System.Web.Script.Services;
using System.Web.UI;
using umbraco.controls.Images;
using System.IO;
using System.Web.Script.Serialization;
using umbraco.businesslogic.Utils;
using umbraco.presentation.webservices;

namespace umbraco.controls.Images
{
	/// <summary>
	/// An ajax service to return the html for an image based on a media id
	/// </summary>
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[ToolboxItem(false)]
	[ScriptService]
	public class ImageViewerUpdater : System.Web.Services.WebService
	{

		/// <summary>
		/// return the a json object with the properties
        /// html = the html returned for rendering the image viewer
        /// mediaId = the media id loaded
        /// width = the width of the media (0) if not found
        /// height = the height of the media (0) if not found
        /// url = the url of the image
        /// alt = the alt text for the image
		/// </summary>
		/// <returns></returns>
		[WebMethod]
        public Dictionary<string, string> UpdateImage(int mediaId, string style, string linkTarget)
		{
            legacyAjaxCalls.Authorize();


			//load the control with the specified properties and render the output as a string and return it
			Page page = new Page();
			string path = Umbraco.Core.IO.IOHelper.ResolveUrl(Umbraco.Core.IO.SystemDirectories.Umbraco) + "/controls/Images/ImageViewer.ascx";
			
			ImageViewer imageViewer = page.LoadControl(path) as ImageViewer;
			imageViewer.MediaId = mediaId;
            ImageViewer.Style _style = (ImageViewer.Style)Enum.Parse(typeof(ImageViewer.Style), style);
            imageViewer.ViewerStyle = _style;
			imageViewer.LinkTarget = linkTarget;

			//this adds only the anchor with image to be rendered, not the whole control!
			page.Controls.Add(imageViewer);
			
			imageViewer.DataBind();

			StringWriter sw = new StringWriter();
			HttpContext.Current.Server.Execute(page, sw, false);

            Dictionary<string, string> rVal = new Dictionary<string, string>();
            rVal.Add("html", sw.ToString());
            rVal.Add("mediaId", imageViewer.MediaId.ToString());
            rVal.Add("width", imageViewer.FileWidth.ToString());
            rVal.Add("height", imageViewer.FileHeight.ToString());
            rVal.Add("url", imageViewer.MediaItemPath);
            rVal.Add("alt", imageViewer.AltText);

            return rVal;
		}
	}
}
