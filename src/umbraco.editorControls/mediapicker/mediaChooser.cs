using System;
using System.Web.UI;
using Umbraco.Core.IO;
using umbraco.cms.presentation.Trees;
using umbraco.interfaces;
using umbraco.controls.Images;
using System.Web.UI.HtmlControls;
using umbraco.editorControls.mediapicker;
using umbraco.cms.businesslogic;

namespace umbraco.editorControls
{
	/// <summary>
	/// Summary description for mediaChooser.
	/// </summary>    
	[ValidationProperty("Value")]
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class mediaChooser : BaseTreePickerEditor
	{
	    readonly bool _showpreview;
	    readonly bool _showadvanced;
		protected ImageViewer ImgViewer;		
		protected HtmlGenericControl PreviewContainer;

        public mediaChooser(IData data)
            : base(data) { }
        

        public mediaChooser(IData data, bool showPreview, bool showAdvanced)
            : base(data)
        {
            _showpreview = showPreview;
            _showadvanced = showAdvanced;
        }

        public override string ModalWindowTitle
        {
            get
            {
                return ui.GetText("general", "choose") + " " + ui.GetText("sections", "media");
            }
        }

        public override string TreePickerUrl
        {
            get
            {
                return _showadvanced ? IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/dialogs/mediaPicker.aspx" : TreeService.GetPickerUrl(Umbraco.Core.Constants.Applications.Media, "media");
            }
        }

        protected override string GetJSScript()
        {
            if (!_showpreview)
            {
                return base.GetJSScript();
            }
            else
            {
                /* 0 = this control's client id
            * 1 = label 
            * 2 = mediaIdValueClientID
            * 3 = previewContainerClientID
            * 4 = imgViewerClientID
            * 5 = mediaTitleClientID
            * 6 = mediaPickerUrl
            * 7 = popup width
            * 8 = popup height
            * 9 = umbraco path
           */
                return string.Format(@"
                var mc_{0} = new Umbraco.Controls.MediaChooser('{1}','{2}','{3}','{4}','{5}','{6}',{7},{8},'{9}');",
                    new string[]
                 {
                    this.ClientID,
                    ModalWindowTitle,
                    ItemIdValue.ClientID,
                    _showpreview ? PreviewContainer.ClientID : "__NOTSET",
                    _showpreview ? ImgViewer.ClientID : "_NOTSET",
                    ItemTitle.ClientID,
                    TreePickerUrl,
                    ModalWidth.ToString(),
                    ModalHeight.ToString(),
                    IOHelper.ResolveUrl(SystemDirectories.Umbraco).TrimEnd('/')
                 });
            }           
        }

        /// <summary>
        /// Renders the required media picker javascript
        /// </summary>
        protected override void RenderJSComponents()
        {
            base.RenderJSComponents();

            if (ScriptManager.GetCurrent(Page).IsInAsyncPostBack)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "MediaChooser", MediaChooserScripts.MediaPicker, true);
            }
            else
            {
                Page.ClientScript.RegisterClientScriptBlock(typeof(mediaChooser), "MediaChooser", MediaChooserScripts.MediaPicker, true);
            }
        }

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			//if preview is enabled, add it to the wrapper
			if (_showpreview)
			{
                //create the preview wrapper
                PreviewContainer = new HtmlGenericControl("div");
                PreviewContainer.ID = "preview";
                Controls.Add(PreviewContainer);

                ImgViewer = (ImageViewer)Page.LoadControl(IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/controls/Images/ImageViewer.ascx");
				ImgViewer.ID = "ImgViewer";
				ImgViewer.ViewerStyle = ImageViewer.Style.ImageLink;	
				
				PreviewContainer.Style.Add(HtmlTextWriterStyle.MarginTop, "5px");
				PreviewContainer.Controls.Add(ImgViewer);
			}			
		}

        /// <summary>
        /// sets the modal width/height based on properties
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ModalWidth = _showadvanced ? 530 : 320;
            ModalHeight = _showadvanced ? 565 : 400;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);


            if (_showpreview)
            {
                if (string.IsNullOrEmpty(ItemIdValue.Value) || !CMSNode.IsNode(int.Parse(ItemIdValue.Value)))
                {
                    PreviewContainer.Style.Add(HtmlTextWriterStyle.Display, "none");
                    ImgViewer.MediaId = -1;
                }
                else
                {
                    ImgViewer.MediaId = int.Parse(ItemIdValue.Value);
                }
            }
        }
	}
}
