using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Linq;
using System.Xml;
using umbraco.BasePages;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;
using Media = umbraco.cms.businesslogic.media.Media;
using Umbraco.Core;

namespace umbraco.controls.Images
{

    /// <summary>
    /// A control to render out the controls to upload a new image to media.
    /// Includes ability to select where in the media you would like it to upload and also supports client
    /// callback methods once complete.
    /// </summary>
    public partial class UploadMediaImage : UserControl
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

        protected IDataType UploadField = DataTypeDefinition.GetByDataTypeId(new Guid(Constants.PropertyEditors.UploadField)).DataType;

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
            var media = Media.MakeNew(TextBoxTitle.Text, cms.businesslogic.media.MediaType.GetByAlias(Constants.Conventions.MediaTypes.Image), BasePage.Current.getUser(), int.Parse(MediaPickerControl.Value));

            foreach (var property in media.GenericProperties)
            {
                if (property.PropertyType.DataTypeDefinition.DataType.Id == UploadField.Id)
                {
                    UploadField.DataTypeDefinitionId = property.PropertyType.DataTypeDefinition.Id;
                    UploadField.Data.PropertyId = property.Id;
                }
            }
            UploadField.DataEditor.Save();
            //MCH NOTE: After having refactored the legacy api to use the new api under the hood, it is necessary to set the property value and save the media.
            var prop = media.GenericProperties.FirstOrDefault(x => x.PropertyType.DataTypeDefinition.DataType.Id == UploadField.Id);
            prop.Value = UploadField.Data.Value;
            media.Save();

            pane_upload.Visible = false;
            
            //this seems real ugly since we apparently already have the properties above (props)... but this data layer is insane and undecipherable:)
            string mainImage = media.getProperty(Constants.Conventions.Media.File).Value.ToString();
            string extension = mainImage.Substring(mainImage.LastIndexOf(".") + 1, mainImage.Length - mainImage.LastIndexOf(".") - 1);            
            var thumbnail = mainImage.Remove(mainImage.Length - extension.Length - 1, extension.Length + 1) + "_thumb.jpg";
            string width = media.getProperty(Constants.Conventions.Media.Width).Value.ToString();
            string height = media.getProperty(Constants.Conventions.Media.Height).Value.ToString();
            int id = media.Id;

            feedback.Style.Add("margin-top", "8px");
            feedback.type = uicontrols.Feedback.feedbacktype.success;
            if (mainImage.StartsWith("~")) mainImage = mainImage.Substring(1);
            if (thumbnail.StartsWith("~")) thumbnail = thumbnail.Substring(1);
            feedback.Text += "<div style=\"text-align: center\"> <a target=\"_blank\" href='" + mainImage + "'><img src='" + thumbnail + "' style='border: none;'/><br/><br/>";
            feedback.Text += ui.Text("thumbnailimageclickfororiginal") + "</a><br/><br/></div>";

            if (!string.IsNullOrEmpty(OnClientUpload))
            {
                feedback.Text += @"
                <script type=""text/javascript"">
                jQuery(document).ready(function() { 
                " + OnClientUpload + @".call(this, {imagePath: '" + mainImage + @"', thumbnailPath: '" + thumbnail + @"', width: " + width + @", height: " + height + @", id: " + id + @"});  
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