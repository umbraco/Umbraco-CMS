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

namespace umbraco.dialogs
{
	/// <summary>
	/// Summary description for uploadImage.
	/// </summary>
	public partial class uploadImage : BasePages.UmbracoEnsuredPage
	{
        protected controls.ContentPicker mediaPickerControl = new umbraco.controls.ContentPicker();

		protected interfaces.IDataType uploadField = new cms.businesslogic.datatype.controls.Factory().GetNewObject(new Guid("5032a6e6-69e3-491d-bb28-cd31cd11086c"));
	
		protected void Page_Load(object sender, System.EventArgs e)
		{
			//uploadField.Alias = "umbracoFile";
			((HtmlInputFile) uploadField.DataEditor).Attributes.Add("onChange", "validateImage()");
			((HtmlInputFile) uploadField.DataEditor).ID = "uploadFile";

            mediaPickerControl.ID = "mediaPickerControl";
            mediaPickerControl.AppAlias = "media";
            mediaPickerControl.TreeAlias = "media";
            mediaPickerControl.ModalHeight = 200;
            mediaPickerControl.ShowDelete = false;
            mediaPickerControl.Text = this.getUser().StartMediaId.ToString();
            mediaPicker.Controls.Add(mediaPickerControl);

			if (!IsPostBack) 
			{
				Button1.Text = ui.Text("save");
				//LiteralTitle.Text = ui.Text("name");
				//LiteralUpload.Text = ui.Text("choose");
            } 
			else 
			{
				
				cms.businesslogic.media.Media m = cms.businesslogic.media.Media.MakeNew(TextBoxTitle.Text, cms.businesslogic.media.MediaType.GetByAlias("image"), this.getUser(), int.Parse(mediaPickerControl.Text));
				foreach (cms.businesslogic.property.Property p in m.getProperties) {
					if (p.PropertyType.DataTypeDefinition.DataType.Id == uploadField.Id) 
					{
						uploadField.DataTypeDefinitionId = p.PropertyType.DataTypeDefinition.Id;
						uploadField.Data.PropertyId = p.Id;
					}
				}
				uploadField.DataEditor.Save();

				// Generate xml on image
				m.XmlGenerate(new XmlDocument());
                pane_upload.Visible = false;
				
				string imagename = m.getProperty("umbracoFile").Value.ToString();
                string extension = imagename.Substring(imagename.LastIndexOf(".") + 1, imagename.Length - imagename.LastIndexOf(".") -1);
				
                imagename = imagename.Remove(imagename.Length-extension.Length-1,extension.Length+1) + "_thumb.jpg";

                feedback.Style.Add("margin-top", "8px");
                feedback.type = umbraco.uicontrols.Feedback.feedbacktype.success;
                feedback.Text += "<div style=\"text-align: center\"> <a target=\"_blank\" href='" + umbraco.GlobalSettings.Path + "/.." + m.getProperty("umbracoFile").Value.ToString() + "'><img src='" + umbraco.GlobalSettings.Path + "/../" + imagename + "' style='border: none;'/><br/><br/>";
                feedback.Text += umbraco.ui.Text("thumbnailimageclickfororiginal") + "</a><br/><br/></div>";
    
                feedback.Text += "<script type=\"text/javascript\">\n parent.refreshTree(); \nparent.updateImageSource('" + umbraco.GlobalSettings.Path + "/.." + m.getProperty("umbracoFile").Value.ToString() + "', '" + TextBoxTitle.Text + "', " + m.getProperty("umbracoWidth").Value.ToString() + ", " + m.getProperty("umbracoHeight").Value.ToString() + ")\n</script>";

			}
			// Put user code to initialize the page here
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);

			// Get upload field from datafield factory
			PlaceHolder1.Controls.Add((Control) uploadField.DataEditor);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    

		}
		#endregion

		protected void Button1_Click(object sender, System.EventArgs e)
		{
		
		}
	}
}
