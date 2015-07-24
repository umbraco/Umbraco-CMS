using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.media;
using Umbraco.Core;

namespace umbraco.controls.Images
{
	public partial class ImageViewer : UserControl
	{

		public ImageViewer()
		{
			MediaItemPath = "#";
            MediaItemThumbnailPath = IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/images/blank.png";
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
		

		private int m_FileWidth;
		private int m_FileHeight;
		private bool m_IsBound;

		/// <summary>
		/// automatically bind if it's not explicitly called.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

		    View view = FindControl(ViewerStyle.ToString()) as View;
            MultiView.SetActiveView(view);

			if (m_IsBound == false)
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
            if (MediaId > 0 && Media.IsNode(MediaId))
            {
                Media m = new Media(MediaId);

                // TODO: Remove "Magic strings" from code.
                var pFile = m.getProperty("fileName");
                if (pFile == null) pFile = m.getProperty(Constants.Conventions.Media.File);
                if (pFile == null) pFile = m.getProperty("file");
                if (pFile == null)
                {
                    //the media requested does not correspond with the standard umbraco properties
                    return;
                }

                MediaItemPath = pFile.Value != null && !string.IsNullOrEmpty(pFile.Value.ToString())
                    ? IOHelper.ResolveUrl(pFile.Value.ToString())
                    : "#";
                AltText = MediaItemPath != "#" ? m.Text : ui.GetText("no") + " " + ui.GetText("media");

                var pWidth = m.getProperty(Constants.Conventions.Media.Width);
                var pHeight = m.getProperty(Constants.Conventions.Media.Height);

                if (pWidth != null && pWidth.Value != null && pHeight != null && pHeight.Value != null)
                {
                    int.TryParse(pWidth.Value.ToString(), out m_FileWidth);
                    int.TryParse(pHeight.Value.ToString(), out m_FileHeight);
                }

                string ext = MediaItemPath.Substring(MediaItemPath.LastIndexOf(".") + 1, MediaItemPath.Length - MediaItemPath.LastIndexOf(".") - 1);
                MediaItemThumbnailPath = MediaItemPath.Replace("." + ext, "_thumb." + ext);

                ImageFound = true;
            }
            else
            {
                ImageFound = false;
            }
        }

        #region Controls

        /// <summary>
        /// JsInclude1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude1;

        /// <summary>
        /// MultiView control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.MultiView MultiView;

        /// <summary>
        /// Basic control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.View Basic;

        /// <summary>
        /// ImageLink control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.View ImageLink;

        /// <summary>
        /// ThumbnailPreview control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.View ThumbnailPreview;

        #endregion
    }
}