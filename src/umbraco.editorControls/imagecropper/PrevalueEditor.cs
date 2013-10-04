using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BusinessLogic;
using umbraco.editorControls;
using umbraco.DataLayer;
using umbraco.interfaces;

namespace umbraco.editorControls.imagecropper
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class PrevalueEditor : PlaceHolder, IDataPrevalue
    {
        private readonly umbraco.cms.businesslogic.datatype.BaseDataType _dataType;

        private PropertyTypePicker imagePropertyTypePicker; // this has replaced txtPropertyAlias (a textbox used to enter a property alias)
        private RequiredFieldValidator imagePropertyRequiredFieldValidator;
       
        private CheckBox chkGenerateCrops;
        private CheckBox chkShowLabel;
        private Literal litQuality;
        private TextBox txtQuality;

        private SmartListBox slbPresets;
        private TextBox txtCropName;
        private TextBox txtTargetWidth;
        private TextBox txtTargetHeight;
        private CheckBox chkKeepAspect;
        private DropDownList ddlDefaultPosH;
        private DropDownList ddlDefaultPosV;

        private Button btnUp;
        private Button btnDown;
        private Button btnAdd;        
        private Button btnRemove;
        private Button btnGenerate;

        //private RegularExpressionValidator revName;
        //private RequiredFieldValidator rqfName;
        //private ValidationSummary vsErrors;

        public PrevalueEditor(umbraco.cms.businesslogic.datatype.BaseDataType dataType)
        {
            _dataType = dataType;
            SetupChildControls();
        }

        public void SetupChildControls() 
        {
            this.imagePropertyTypePicker = new PropertyTypePicker() { ID = "imagePropertyTypePicker" };
            this.imagePropertyRequiredFieldValidator = new RequiredFieldValidator()
                                                            {
                                                                ID = "imagePropertyRequiredFieldValidator",
                                                                Text = " Required",
                                                                InitialValue = string.Empty,
                                                                ControlToValidate = this.imagePropertyTypePicker.ID
                                                            };
            
            chkGenerateCrops = new CheckBox {ID = "generateimg", AutoPostBack = true};
            litQuality = new Literal {ID = "qualityLiteral", Text = " Quality ", Visible = false};
            txtQuality = new TextBox {ID = "quality", Width = Unit.Pixel(30), Visible = false};
            chkShowLabel = new CheckBox {ID = "label"};
            slbPresets = new SmartListBox
                             {
                                 ID = "presets",
                                 SelectionMode = ListSelectionMode.Multiple,
                                 Height = Unit.Pixel(123),
                                 Width = Unit.Pixel(350)
                             };

            txtCropName = new TextBox {ID = "presetname", Width = Unit.Pixel(100)};
            txtTargetWidth = new TextBox {ID = "presetw", Width = Unit.Pixel(50)};
            txtTargetHeight = new TextBox {ID = "preseth", Width = Unit.Pixel(50)};
            chkKeepAspect = new CheckBox {ID = "aspect", Checked = true};

            ddlDefaultPosH = new DropDownList {ID = "posh"};
            ddlDefaultPosH.Items.Add(new ListItem("Left", "L"));
            ddlDefaultPosH.Items.Add(new ListItem("Center", "C"));
            ddlDefaultPosH.Items.Add(new ListItem("Right", "R"));

            ddlDefaultPosV = new DropDownList {ID = "posv"};
            ddlDefaultPosV.Items.Add(new ListItem("Top", "T"));
            ddlDefaultPosV.Items.Add(new ListItem("Middle", "M"));
            ddlDefaultPosV.Items.Add(new ListItem("Bottom", "B"));

            btnUp = new Button {ID = "up", Text = "Up", Width = Unit.Pixel(60)};
            btnDown = new Button {ID = "down", Text = "Down", Width = Unit.Pixel(60)};
            btnAdd = new Button {ID = "add", Text = "Add", Width = Unit.Pixel(60)};
            btnRemove = new Button {ID = "remove", Text = "Remove", Width = Unit.Pixel(60)};
            btnGenerate = new Button {ID = "generate", Text = "Generate", Width = Unit.Pixel(60)};

            //vsErrors = new ValidationSummary {ID = "summary", ValidationGroup = "cropper"};
            //rqfName = new RequiredFieldValidator {ID = "namevalidator", ValidationGroup = "cropper", ControlToValidate = txtCropName.ClientID, ErrorMessage = "Crop name missing", Text="*" };

            //revName = new RegularExpressionValidator
            //               {
            //                   ID = "namevalidator",
            //                   ValidationExpression = ".*[a-zA-Z0-9-_ ].*",
            //                   ValidationGroup = "cropper",
            //                   ErrorMessage = "Invalid name. Alphanumerical only please as this will be the filename",
            //                   AssociatedControlID = txtCropName.ID
            //               };

            Controls.Add(this.imagePropertyTypePicker);
            Controls.Add(this.imagePropertyRequiredFieldValidator);

            Controls.Add(chkGenerateCrops);
            Controls.Add(litQuality);
            Controls.Add(txtQuality);
            Controls.Add(chkShowLabel);

            Controls.Add(slbPresets);
            Controls.Add(txtCropName);
            Controls.Add(txtTargetWidth);
            Controls.Add(txtTargetHeight);
            Controls.Add(chkKeepAspect);
            Controls.Add(ddlDefaultPosH);
            Controls.Add(ddlDefaultPosV);

            Controls.Add(btnUp);
            Controls.Add(btnDown);
            Controls.Add(btnAdd);
            Controls.Add(btnRemove);
            Controls.Add(btnGenerate);

            //Controls.Add(vsErrors);
            //Controls.Add(rqfName);
            //Controls.Add(revName);

            btnUp.Click += _upButton_Click;
            btnDown.Click += _downButton_Click;
            btnAdd.Click += _addButton_Click;
            btnRemove.Click += _removeButton_Click;
            
            //btnGenerate.Click += _generateButton_Click;

            chkGenerateCrops.CheckedChanged += _generateImagesCheckBox_CheckedChanged;            
        }

#if false
        void _generateButton_Click(object sender, EventArgs e)
        {
            Config config = new Config(Configuration);

            // get list of nodeids with this datatype
            using (IRecordsReader rdr = SqlHelper.ExecuteReader(
                "SELECT DISTINCT contentNodeId, " +
                "(SELECT Alias FROM cmsPropertyType WHERE Id = pd.propertyTypeId) AS propertyAlias " +
                "FROM cmsPropertyData pd " +
                "WHERE PropertyTypeId IN (SELECT Id FROM cmsPropertyType WHERE DataTypeId = " + _dataType.DataTypeDefinitionId + ")"))
            {
                while (rdr.Read())
                {
                    int documentId = rdr.GetInt("contentNodeId");
                    string propertyAlias = rdr.GetString("propertyAlias");

                    Document document = new Document(documentId);

                    Property cropProperty = document.getProperty(propertyAlias);
                    Property imageProperty = document.getProperty(config.UploadPropertyAlias);

                    if (cropProperty != null) // && cropProperty.Value.ToString() == ""
                    {
                        ImageInfo imageInfo = new ImageInfo(imageProperty.Value.ToString());

                        if (imageInfo.Exists)
                        {
                            SaveData saveData = new SaveData();

                            foreach (Preset preset in config.presets)
                            {
                                Crop crop = preset.Fit(imageInfo);
                                saveData.data.Add(crop);
                            }

                            //cropProperty.Value = saveData.Xml(config, imageInfo);

                            imageInfo.GenerateThumbnails(saveData, config);

                            if (document.Published)
                            {
                                //document.Publish(document.User);
                                //umbraco.library.UpdateDocumentCache(document.Id);
                            }
                            else
                            {
                                //document.Save();
                            }
                        }
                    }
                }
            }
        }
#endif

        void _generateImagesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            txtQuality.Visible = chkGenerateCrops.Checked;
            litQuality.Visible = chkGenerateCrops.Checked;
        }

        void _upButton_Click(object sender, EventArgs e)
        {
            slbPresets.MoveUp();
        }

        void _downButton_Click(object sender, EventArgs e)
        {
            slbPresets.MoveDown();    
        }

        void _removeButton_Click(object sender, EventArgs e)
        {
            for (int i = slbPresets.Items.Count - 1; i >= 0; i--)
            {
                if (slbPresets.Items[i].Selected)
                    slbPresets.Items.Remove(slbPresets.Items[i]);
            }            
        }

        void _addButton_Click(object sender, EventArgs e)
        {
            slbPresets.Items.Add(
                new ListItem(
                    getListItemDisplayName(
                        txtCropName.Text,
                        txtTargetWidth.Text,
                        txtTargetHeight.Text,
                        chkKeepAspect.Checked ? "1" : "0",
                        String.Concat(ddlDefaultPosH.SelectedValue, ddlDefaultPosV.SelectedValue)),
                    String.Format("{0},{1},{2},{3},{4}",
                                  txtCropName.Text,
                                  txtTargetWidth.Text,
                                  txtTargetHeight.Text,
                                  chkKeepAspect.Checked ? "1" : "0",
                                  String.Concat(ddlDefaultPosH.SelectedValue, ddlDefaultPosV.SelectedValue))
                    )
                );
            txtCropName.Text = "";
            txtTargetWidth.Text = "";
            txtTargetHeight.Text = "";
            chkKeepAspect.Checked = true;

        }

        public Control Editor
        {
            get
            {
                return this;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
                LoadData();
        }

        private void LoadData()
        {
            if (!string.IsNullOrEmpty(Configuration))
            {
                Config config = new Config(Configuration);

                if (this.imagePropertyTypePicker.Items.Contains(new ListItem(config.UploadPropertyAlias)))
                {
                    this.imagePropertyTypePicker.SelectedValue = config.UploadPropertyAlias;
                }

                chkGenerateCrops.Checked = config.GenerateImages;
                chkShowLabel.Checked = config.ShowLabel;
                txtQuality.Visible = chkShowLabel.Checked;
                txtQuality.Text = config.Quality.ToString();
                litQuality.Visible = chkShowLabel.Checked;

                foreach (Preset preset in config.presets)
                {
                    slbPresets.Items.Add(
                        new ListItem(
                            getListItemDisplayName(
                                preset.Name,
                                preset.TargetWidth.ToString(),
                                preset.TargetHeight.ToString(),
                                preset.KeepAspect ? "1" : "0",
                                String.Concat(preset.PositionH, preset.PositionV)),
                            String.Format("{0},{1},{2},{3},{4}{5}",
                                          preset.Name,
                                          preset.TargetWidth,
                                          preset.TargetHeight,
                                          preset.KeepAspect ? "1" : "0",
                                          preset.PositionH, preset.PositionV)));
                }
            }
        }

        private static string getListItemDisplayName(string presetTemplateName, string width, string height, string keepAspect, string position)
        {
            return String.Format("{0}, width: {1}px, height: {2}px, keep aspect: {3}, {4}",
                                 presetTemplateName,
                                 width,
                                 height,
                                 keepAspect == "1" ? "yes" : "no",
                                 position);
        }

        /// <summary>
        /// Serialize configuration to:
        /// uploadPropertyAlias,generateImages,quality,showLabel|presetTemplateName,targetWidth,targetHeight,keepAspect;
        /// </summary>
        public void Save()
        {
            _dataType.DBType = (umbraco.cms.businesslogic.datatype.DBTypes)Enum.Parse(typeof(umbraco.cms.businesslogic.datatype.DBTypes), DBTypes.Ntext.ToString(), true);

            string generalData = String.Format("{0},{1},{2},{3}",
                                               this.imagePropertyTypePicker.SelectedValue,
                                               chkGenerateCrops.Checked ? "1" : "0",
                                               chkShowLabel.Checked ? "1" : "0",
                                               txtQuality.Text
                );

            string templateData = "";

            for (int i = 0; i < slbPresets.Items.Count; i++)
            {
                templateData += slbPresets.Items[i].Value;
                if (i < slbPresets.Items.Count - 1) templateData += ";";
            }

            string data = String.Format("{0}|{1}", generalData, templateData);

            SqlHelper.ExecuteNonQuery("delete from cmsDataTypePreValues where datatypenodeid = @dtdefid",
                                      SqlHelper.CreateParameter("@dtdefid", _dataType.DataTypeDefinitionId));

            SqlHelper.ExecuteNonQuery("insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,'')",
                                      SqlHelper.CreateParameter("@dtdefid", _dataType.DataTypeDefinitionId), SqlHelper.CreateParameter("@value", data));
        }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write("<p><strong>General</strong></p>");
            writer.Write("<table>");

            writer.Write(@" <tr>
                                <td>Property alias: </td>");
            writer.Write("      <td>");
            this.imagePropertyTypePicker.RenderControl(writer);
            this.imagePropertyRequiredFieldValidator.RenderControl(writer);            
            writer.Write(@"      </td>
                            </tr>");
            
            writer.Write("  <tr><td>Save crop images (/media/(imageid)/(filename)_(cropname).jpg):</td><td>");
            chkGenerateCrops.RenderControl(writer);
            litQuality.RenderControl(writer);
            txtQuality.RenderControl(writer);
            writer.Write("  </td></tr>");

            writer.Write("  <tr><td>Show Label:</td><td>");
            chkShowLabel.RenderControl(writer);
            writer.Write("  </td></tr>");

            writer.Write("</table>");

            writer.Write("<p><strong>Crops</strong></p>");

            writer.Write("<table>");
            writer.Write("  <tr><td valign=\"top\">");

            writer.Write("      <table>");
            writer.Write("          <tr><td>Name</td><td>");
            txtCropName.RenderControl(writer);
            writer.Write("          </td></tr>");
            writer.Write("          <tr><td>Target width</td><td>");
            txtTargetWidth.RenderControl(writer);
            writer.Write("          px</td></tr>");
            writer.Write("          <tr><td>Target height</td><td>");
            txtTargetHeight.RenderControl(writer);
            writer.Write("          px</td></tr>");
            writer.Write("          <tr><td>Default position&nbsp;</td><td>");
            ddlDefaultPosH.RenderControl(writer);
            writer.Write(" ");
            ddlDefaultPosV.RenderControl(writer);
            writer.Write("          </td></tr>");
            writer.Write("          <tr><td>Keep aspect</td><td>");
            chkKeepAspect.RenderControl(writer);
            writer.Write("          </td></tr>");
            writer.Write("      </table><br />");
            btnAdd.RenderControl(writer);

            writer.Write("  </td><td valign=\"top\">&nbsp;&nbsp;");
            slbPresets.RenderControl(writer);
            writer.Write("  </td><td valign=\"top\">");
            btnUp.RenderControl(writer);
            writer.Write("  <br />");
            btnDown.RenderControl(writer);
            writer.Write("  <br /><br /><br /><br /><br />");
            btnRemove.RenderControl(writer);
            writer.Write("  </td></tr>");
            writer.Write("</table>");
            
            //_generateButton.RenderControl(writer);
            //_vsErrors.RenderControl(writer);
            //_revName.RenderControl(writer);           
        }

        public string Configuration
        {
            get
            {
                object conf =
                    SqlHelper.ExecuteScalar<object>("select value from cmsDataTypePreValues where datatypenodeid = @datatypenodeid",
                                                    SqlHelper.CreateParameter("@datatypenodeid", _dataType.DataTypeDefinitionId));

                if (conf != null)
                    return conf.ToString();

                return string.Empty;
            }
        }

        public static ISqlHelper SqlHelper
        {
            get
            {
                return Application.SqlHelper;
            }
        }
    }
}