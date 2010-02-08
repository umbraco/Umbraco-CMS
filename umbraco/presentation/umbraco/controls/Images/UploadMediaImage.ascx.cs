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
using System.Linq;
using System.Xml;
using umbraco.BasePages;
using umbraco.uicontrols;
using umbraco.interfaces;
using umbraco.cms.businesslogic.media;

namespace umbraco.controls.Images
{

    /// <summary>
    /// A control to render out the controls to upload a new image to media.
    /// Includes ability to select where in the media you would like it to upload and also supports client
    /// callback methods once complete.
    /// </summary>
    public partial class UploadMediaImage : System.Web.UI.UserControl
    {

        public UploadMediaImage()
        {
            OnClientUpload = "";
        }

        /// <summary>
        /// The JavaScript method to be invoked once the image is uploaded, the page is rendered and the document is ready.
        /// The method will receive a JSON object with the following parameters:
        /// - imagePath
        /// - thumbnailPath
        /// - width
        /// - height
        /// - id
        /// </summary>
        public string OnClientUpload { get; set; }

        protected IDataType UploadField = new cms.businesslogic.datatype.controls.Factory().GetNewObject(new Guid("5032a6e6-69e3-491d-bb28-cd31cd11086c"));

        protected override void OnInit(EventArgs e)
        {

            base.OnInit(e);
            // Get upload field from datafield factory
            UploadControl.Controls.Add((Control)UploadField.DataEditor);
        }

        protected void Page_Load(object sender, EventArgs e)
        {

            ((HtmlInputFile)UploadField.DataEditor).ID = "uploadFile";
            if (!IsPostBack)
            {
                DataBind();
            }
            

       }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            Media m = Media.MakeNew(TextBoxTitle.Text, cms.businesslogic.media.MediaType.GetByAlias("image"), BasePage.Current.getUser(), int.Parse(MediaPickerControl.Text));
            var props = m.getProperties;
            foreach (cms.businesslogic.property.Property p in props)
            {
                if (p.PropertyType.DataTypeDefinition.DataType.Id == UploadField.Id)
                {
                    UploadField.DataTypeDefinitionId = p.PropertyType.DataTypeDefinition.Id;
                    UploadField.Data.PropertyId = p.Id;
                }
            }
            UploadField.DataEditor.Save();

            // Generate xml on image
            m.XmlGenerate(new XmlDocument());
            pane_upload.Visible = false;
            
            //this seems real ugly since we apparently already have the properties above (props)... but this data layer is insane and undecipherable:)
            string mainImage = m.getProperty("umbracoFile").Value.ToString();
            string extension = mainImage.Substring(mainImage.LastIndexOf(".") + 1, mainImage.Length - mainImage.LastIndexOf(".") - 1);            
            var thumbnail = mainImage.Remove(mainImage.Length - extension.Length - 1, extension.Length + 1) + "_thumb.jpg";
            string width = m.getProperty("umbracoWidth").Value.ToString();
            string height = m.getProperty("umbracoHeight").Value.ToString();
            int id = m.Id;

            feedback.Style.Add("margin-top", "8px");
            feedback.type = uicontrols.Feedback.feedbacktype.success;
            feedback.Text += "<div style=\"text-align: center\"> <a target=\"_blank\" href='" + umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) + "/.." + mainImage + "'><img src='" + umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) + "/.." + thumbnail + "' style='border: none;'/><br/><br/>";
            feedback.Text += ui.Text("thumbnailimageclickfororiginal") + "</a><br/><br/></div>";

            if (!string.IsNullOrEmpty(OnClientUpload))
            {
                feedback.Text += @"
                <script type=""text/javascript"">
                jQuery(document).ready(function() { 
                " + OnClientUpload + @".call(this, {imagePath: '" + mainImage + @"', thumbnailPath: '" + thumbnail + @"', width: " + width + @", height: " + height + @", id: " + id.ToString() + @"});  
                });
                </script>";
            }
            
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ((HtmlInputFile)UploadField.DataEditor).Attributes.Add("onChange", "uploader_" + this.ClientID + ".validateImage();");
        }
    }
}