using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;

namespace umbraco.editorControls.imagecropper
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class DataEditor : PlaceHolder, umbraco.interfaces.IDataEditor
    {
        private umbraco.interfaces.IData data;
        private Config config;
        private XmlDocument _xml;

        public Image imgImage = new Image();
        public HiddenField hdnJson = new HiddenField();
        public HiddenField hdnRaw = new HiddenField();
        public HiddenField hdnSer = new HiddenField();

        public DataEditor(umbraco.interfaces.IData Data, string Configuration)
        {
            data = Data;
            config = new Config(Configuration);
        }

        public virtual bool TreatAsRichTextEditor { get { return false; } }

        public bool ShowLabel { get { return config.ShowLabel; } }

        public Control Editor { get { return this; } }

        protected override void OnInit(EventArgs e)
        {
            this.ID = "ImageCropper";
            //base.OnInit(e);

            int propertyId = ((umbraco.cms.businesslogic.datatype.DefaultData)data).PropertyId;

            int currentDocumentId = ((umbraco.cms.businesslogic.datatype.DefaultData)data).NodeId;
            Property uploadProperty;

            // we need this ugly code because there's no way to use a base class
            CMSNode node = new CMSNode(currentDocumentId);
            if (node.nodeObjectType == Document._objectType)
            {
                uploadProperty = new Document(currentDocumentId).getProperty(config.UploadPropertyAlias);
            }
            else if (node.nodeObjectType == umbraco.cms.businesslogic.media.Media._objectType)
            {
                uploadProperty = new Media(currentDocumentId).getProperty(config.UploadPropertyAlias);
            }
            else if (node.nodeObjectType == Member._objectType)
            {
                uploadProperty = new Member(currentDocumentId).getProperty(config.UploadPropertyAlias);
            }
            else
            {
                throw new Exception("Unsupported Umbraco Node type for Image Cropper (only Document, Media and Members are supported.");
            }

            // upload property could be null here if the property wasn't found
            if (uploadProperty != null)
            {
                string relativeImagePath = uploadProperty.Value.ToString();

                ImageInfo imageInfo = new ImageInfo(relativeImagePath);

                imgImage.ImageUrl = relativeImagePath;
                imgImage.ID = String.Format("cropBox_{0}", propertyId);

                StringBuilder sbJson = new StringBuilder();
                StringBuilder sbRaw = new StringBuilder();

                try
                {
                    _xml = new XmlDocument();
                    _xml.LoadXml(data.Value.ToString());
                }
                catch
                {
                    _xml = createBaseXmlDocument();
                }

                sbJson.Append("{ \"current\": 0, \"crops\": [");

                for (int i = 0; i < config.presets.Count; i++)
                {
                    Preset preset = (Preset)config.presets[i];
                    Crop crop;

                    sbJson.Append("{\"name\":'" + preset.Name + "'");

                    sbJson.Append(",\"config\":{" +
                                  String.Format("\"targetWidth\":{0},\"targetHeight\":{1},\"keepAspect\":{2}",
                                                preset.TargetWidth, preset.TargetHeight,
                                                (preset.KeepAspect ? "true" : "false") + "}"));

                    if (imageInfo.Exists)
                    {
                        crop = preset.Fit(imageInfo);
                    }
                    else
                    {
                        crop.X = 0;
                        crop.Y = 0;
                        crop.X2 = preset.TargetWidth;
                        crop.Y2 = preset.TargetHeight;
                    }

                    // stored
                    if (_xml.DocumentElement != null && _xml.DocumentElement.ChildNodes.Count == config.presets.Count)
                    {
                        XmlNode xmlNode = _xml.DocumentElement.ChildNodes[i];

                        int xml_x = Convert.ToInt32(xmlNode.Attributes["x"].Value);
                        int xml_y = Convert.ToInt32(xmlNode.Attributes["y"].Value);
                        int xml_x2 = Convert.ToInt32(xmlNode.Attributes["x2"].Value);
                        int xml_y2 = Convert.ToInt32(xmlNode.Attributes["y2"].Value);

                        // only use xml values if image is the same and different from defaults (document is stored inbetween image upload and cropping)
                        //if (xml_x2 - xml_x != preset.TargetWidth || xml_y2 - xml_y != preset.TargetHeight)
                        //fileDate == imageInfo.DateStamp && (

                        if (crop.X != xml_x || crop.X2 != xml_x2 || crop.Y != xml_y || crop.Y2 != xml_y2)
                        {
                            crop.X = xml_x;
                            crop.Y = xml_y;
                            crop.X2 = xml_x2;
                            crop.Y2 = xml_y2;
                        }
                    }

                    sbJson.Append(",\"value\":{" + String.Format("\"x\":{0},\"y\":{1},\"x2\":{2},\"y2\":{3}", crop.X, crop.Y, crop.X2, crop.Y2) + "}}");
                    sbRaw.Append(String.Format("{0},{1},{2},{3}", crop.X, crop.Y, crop.X2, crop.Y2));

                    if (i < config.presets.Count - 1)
                    {
                        sbJson.Append(",");
                        sbRaw.Append(";");
                    }
                }

                sbJson.Append("]}");

                hdnJson.Value = sbJson.ToString();
                //hdnJson.ID = String.Format("json_{0}", propertyId);
                hdnRaw.Value = sbRaw.ToString();
                //hdnRaw.ID = String.Format("raw_{0}", propertyId);

                Controls.Add(imgImage);

                Controls.Add(hdnJson);
                Controls.Add(hdnRaw);

                string imageCropperInitScript =
                    "initImageCropper('" +
                    imgImage.ClientID + "', '" +
                    hdnJson.ClientID + "', '" +
                    hdnRaw.ClientID +
                    "');";

                Page.ClientScript.RegisterStartupScript(GetType(), ClientID + "_imageCropper", imageCropperInitScript, true);
                Page.ClientScript.RegisterClientScriptBlock(Resources.json2Script.GetType(), "json2Script", Resources.json2Script, true);
                Page.ClientScript.RegisterClientScriptBlock(Resources.jCropCSS.GetType(), "jCropCSS", Resources.jCropCSS);
                Page.ClientScript.RegisterClientScriptBlock(Resources.jCropScript.GetType(), "jCropScript", Resources.jCropScript, true);
                Page.ClientScript.RegisterClientScriptBlock(Resources.imageCropperScript.GetType(), "imageCropperScript", Resources.imageCropperScript, true);

            }

            base.OnInit(e);
        }

        /// <summary>
        /// Store data as string XML (overridden by ToXMl to store "real" XML
        /// XML format:
        /// <crops dateStamp="">
        ///     <crop name="" x="" y="" x2="" y2="" url="" />
        /// </crops>
        /// </summary>
        public void Save()
        {
            ImageInfo imageInfo = new ImageInfo(imgImage.ImageUrl);
            if (!imageInfo.Exists)
            {
                data.Value = "";
            }
            else
            {
                SaveData saveData = new SaveData(hdnRaw.Value);
                data.Value = saveData.Xml(config, imageInfo);
                imageInfo.GenerateThumbnails(saveData, config);
            }
        }

        private static XmlDocument createBaseXmlDocument()
        {
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.CreateElement("crops");
            doc.AppendChild(root);
            return doc;
        }
    }
}