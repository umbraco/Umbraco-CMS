using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.media;

namespace umbraco.controls.Images
{
	public partial class ImageViewer : System.Web.UI.UserControl
	{

		public ImageViewer()
		{
			MediaItemPath = "#";
            MediaItemThumbnailPath = umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) + "/images/blank.png";
			AltText = "No Image";
			ImageFound = false;
            ViewerStyle = Style.Basic;
			LinkTarget = "_blank";
		}
		

		/// <summary>
		/// A JS method to invoke when the image is loaded. The method should accept the media ID.
		/// </summary>
		public string ClientCallbackMethod { get; set; }

		public int MediaId { get; set; }

        /// <summary>
        /// The style to render the image viewer in
        /// </summary>
        public enum Style 
        { 
            Basic = 0, 
            ImageLink = 1, 
            ThumbnailPreview = 2
        }

        public Style ViewerStyle { get; set; }

		public string LinkTarget { get; set; }
		
		public string MediaItemPath { get; private set; }
		public string MediaItemThumbnailPath { get; private set; }
		public string AltText { get; private set; }
		public int FileWidth { get { return m_FileWidth; } }
		public int FileHeight { get { return m_FileHeight; } }

		protected bool ImageFound { get; private set; }
		

		private int m_FileWidth = 0;
		private int m_FileHeight = 0;
		private bool m_IsBound = false;

		/// <summary>
		/// automatically bind if it's not explicitly called.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if (!m_IsBound)
			{
				DataBind();
			}          
		}

		public override void DataBind()
		{
			LookupData();
			base.DataBind();
			this.m_IsBound = true;
		}

		private void LookupData()
		{			
			if (MediaId > 0)
			{
				Media m = new Media(MediaId);

				// TODO: Remove "Magic strings" from code.
				var pFile = m.getProperty("fileName");
				if (pFile == null) pFile = m.getProperty("umbracoFile");
				if (pFile == null) pFile = m.getProperty("file");
				if (pFile == null)
				{
					//the media requested does not correspond with the standard umbraco properties
					return;
				}

				MediaItemPath = pFile.Value != null && !string.IsNullOrEmpty(pFile.Value.ToString()) 
                    ? umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) + "/.." + pFile.Value.ToString() 
                    : "#";
				AltText = MediaItemPath != "#" ? m.Text : ui.GetText("no") + " " + ui.GetText("media");

				var pWidth = m.getProperty("umbracoWidth");
				var pHeight = m.getProperty("umbracoHeight");

				if (pWidth != null && pWidth.Value != null && pHeight != null && pHeight.Value != null)
				{
					int.TryParse(pHeight.Value.ToString(), out m_FileWidth);
					int.TryParse(pHeight.Value.ToString(), out m_FileHeight);
				}

				string ext = MediaItemPath.Substring(MediaItemPath.LastIndexOf(".") + 1, MediaItemPath.Length - MediaItemPath.LastIndexOf(".") - 1);
				MediaItemThumbnailPath = MediaItemPath.Replace("." + ext, "_thumb.jpg");

				ImageFound = true;
			}
		}
	}
}