using System;
using System.Collections;
using System.Xml;

namespace umbraco.editorControls.imagecropper
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class SaveData
    {
        public ArrayList data { get; set; }

        public string Xml(Config config, ImageInfo imageInfo)
        {
            XmlDocument doc = createBaseXmlDocument();
            XmlNode root = doc.DocumentElement;

            if (root == null) return null;

            XmlNode dateStampNode = doc.CreateNode(XmlNodeType.Attribute, "date", null);
            dateStampNode.Value = imageInfo.DateStamp.ToString("s");
            root.Attributes.SetNamedItem(dateStampNode);

            for (int i = 0; i < data.Count; i++)
            {
                Crop crop = (Crop) data[i];
                Preset preset = (Preset) config.presets[i];

                XmlNode newNode = doc.CreateElement("crop");

                XmlNode nameNode = doc.CreateNode(XmlNodeType.Attribute, "name", null);
                nameNode.Value = preset.Name;
                newNode.Attributes.SetNamedItem(nameNode);

                XmlNode xNode = doc.CreateNode(XmlNodeType.Attribute, "x", null);
                xNode.Value = crop.X.ToString();
                newNode.Attributes.SetNamedItem(xNode);

                XmlNode yNode = doc.CreateNode(XmlNodeType.Attribute, "y", null);
                yNode.Value = crop.Y.ToString();
                newNode.Attributes.SetNamedItem(yNode);

                XmlNode x2Node = doc.CreateNode(XmlNodeType.Attribute, "x2", null);
                x2Node.Value = crop.X2.ToString();
                newNode.Attributes.SetNamedItem(x2Node);

                XmlNode y2Node = doc.CreateNode(XmlNodeType.Attribute, "y2", null);
                y2Node.Value = crop.Y2.ToString();
                newNode.Attributes.SetNamedItem(y2Node);

                if (config.GenerateImages)
                {
                    XmlNode urlNode = doc.CreateNode(XmlNodeType.Attribute, "url", null);
                    urlNode.Value = String.Format("{0}/{1}_{2}.jpg",
                                                  imageInfo.RelativePath.Substring(0,
                                                                                   imageInfo.RelativePath.LastIndexOf(
                                                                                       '/')),
                                                  imageInfo.Name,
                                                  preset.Name);
                    newNode.Attributes.SetNamedItem(urlNode);
                }

                root.AppendChild(newNode);
            }

            return doc.InnerXml;
        }

        public SaveData()
        {
            data = new ArrayList();
        }

        public SaveData(string raw)
        {
            data = new ArrayList();

            string[] crops = raw.Split(';');

            foreach (string crop in crops)
            {
                var val = crop.Split(',');

                data.Add(
                    new Crop(
                        Convert.ToInt32(val[0]),
                        Convert.ToInt32(val[1]),
                        Convert.ToInt32(val[2]),
                        Convert.ToInt32(val[3])
                        )
                    );
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