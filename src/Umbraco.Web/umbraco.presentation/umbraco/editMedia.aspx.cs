using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using Umbraco.Core;
using umbraco.cms.businesslogic.datatype.controls;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.property;

namespace umbraco.cms.presentation
{
	/// <summary>
	/// Summary description for editMedia.
	/// </summary>
	public partial class editMedia : BasePages.UmbracoEnsuredPage
	{
        private readonly uicontrols.Pane _mediaPropertiesPane = new uicontrols.Pane();
        private readonly LiteralControl _updateDateLiteral = new LiteralControl();
        private readonly LiteralControl _mediaFileLinksLiteral = new LiteralControl();

	    public editMedia()
	    {
	        CurrentApp = BusinessLogic.DefaultApps.media.ToString();
	    }

		protected uicontrols.TabView TabView1;
		protected TextBox documentName;
		private businesslogic.media.Media _media;
		controls.ContentControl _contentControl;
        
        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            int id = int.Parse(Request.QueryString["id"]);

            //Loading Media via new public service to ensure that the Properties are loaded correct
            var media = ApplicationContext.Current.Services.MediaService.GetById(id);
            _media = new cms.businesslogic.media.Media(media);

            // Save media on first load
            bool exists = SqlHelper.ExecuteScalar<int>("SELECT COUNT(nodeId) FROM cmsContentXml WHERE nodeId = @nodeId",
                                       SqlHelper.CreateParameter("@nodeId", _media.Id)) > 0;
            if (!exists)
            {
                _media.XmlGenerate(new XmlDocument());
            }

            _contentControl = new controls.ContentControl(_media, controls.ContentControl.publishModes.NoPublish, "TabView1");
            _contentControl.Width = Unit.Pixel(666);
            _contentControl.Height = Unit.Pixel(666);

            //this must be set to false as we don't want to proceed to save anything if the page is invalid
            _contentControl.SavePropertyDataWhenInvalid = false;

            plc.Controls.Add(_contentControl);

            _contentControl.Save += new System.EventHandler(Save);

            this._updateDateLiteral.ID = "updateDate";
            this._updateDateLiteral.Text = _media.VersionDate.ToShortDateString() + " " + _media.VersionDate.ToShortTimeString();

            this._mediaFileLinksLiteral.ID = "mediaFileLinks";
            _mediaPropertiesPane.addProperty(ui.Text("content", "updateDate", base.getUser()), this._updateDateLiteral);

            this.UpdateMediaFileLinksLiteral();
            _mediaPropertiesPane.addProperty(ui.Text("content", "mediaLinks"), this._mediaFileLinksLiteral);

            // add the property pane to the page rendering
            _contentControl.tpProp.Controls.AddAt(1, _mediaPropertiesPane);                       
        }

		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				ClientTools.SyncTree(_media.Path, false);
			}			
		}

		protected void Save(object sender, EventArgs e) 
		{
            // do not continue saving anything if the page is invalid!
            // http://issues.umbraco.org/issue/U4-227
            if (!Page.IsValid)
            {
                foreach (uicontrols.TabPage tp in _contentControl.GetPanels())
                {
                    tp.ErrorControl.Visible = true;
                    tp.ErrorHeader = ui.Text("errorHandling", "errorHeader");
                    tp.CloseCaption = ui.Text("close");
                }
            }
            else
            {
                if (Page.IsPostBack)
                {
                    // hide validation summaries
                    foreach (uicontrols.TabPage tp in _contentControl.GetPanels())
                    {
                        tp.ErrorControl.Visible = false;
                    }
                }    

            //The value of the properties has been set on IData through IDataEditor in the ContentControl
            //so we need to 'retrieve' that value and set it on the property of the new IContent object.
            //NOTE This is a workaround for the legacy approach to saving values through the DataType instead of the Property 
            //- (The DataType shouldn't be responsible for saving the value - especically directly to the db).
            foreach (var item in tmp.DataTypes)
            {
                _media.getProperty(item.Key).Value = item.Value.Data.Value;
            }

                _media.Save();

                this._updateDateLiteral.Text = _media.VersionDate.ToShortDateString() + " " + _media.VersionDate.ToShortTimeString();
                this.UpdateMediaFileLinksLiteral();

                _media.XmlGenerate(new XmlDocument());
                ClientTools.ShowSpeechBubble(speechBubbleIcon.save, ui.Text("speechBubbles", "editMediaSaved"), ui.Text("editMediaSavedText"));
                ClientTools.SyncTree(_media.Path, true);
            }                               
		}

        private void UpdateMediaFileLinksLiteral()
        {
            var uploadField = new Factory().GetNewObject(new Guid("5032a6e6-69e3-491d-bb28-cd31cd11086c"));

            // always clear, incase the upload file was removed
            this._mediaFileLinksLiteral.Text = string.Empty;

            try
            {
                var uploadProperties = _media.GenericProperties
                    .Where(p => p.PropertyType.DataTypeDefinition.DataType.Id == uploadField.Id
                                && p.Value.ToString() != ""
                                && File.Exists(IOHelper.MapPath(p.Value.ToString())));

                var properties = uploadProperties as List<Property> ?? uploadProperties.ToList();

                if (properties.Any())
                {
                    this._mediaFileLinksLiteral.Text += "<table>";

                    foreach (var property in properties)
                    {
                        this._mediaFileLinksLiteral.Text += string.Format("<tr><td>{0}&nbsp;</td><td><a href=\"{1}\" target=\"_blank\">{1}</a></td></tr>", property.PropertyType.Name, property.Value);
                    }
                    
                    this._mediaFileLinksLiteral.Text += "</table>";
                }
            }
            catch
            {
                //the data type definition may not exist anymore at this point because another thread may
                //have deleted it.
            }
        }
	}
}
