using System;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace umbraco.editorControls.userControlGrapper
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class SerializationHelper
    {
        public static object ValueFromXmlString(object value, Type type)
        {
            XmlSerializer ser = new XmlSerializer(type);
            StringReader strRdr = new StringReader(value.ToString());
            XmlTextReader xmlRdr = new XmlTextReader(strRdr);
            object obj = ser.Deserialize(xmlRdr);
            xmlRdr.Close();
            strRdr.Close();
            return obj;
        }

        public static string ValueToXmlString(object value)
        {
            MemoryStream str = new MemoryStream();
            XmlSerializer ser = new XmlSerializer(value.GetType());
            ser.Serialize(str, value);
            str.Seek(0, System.IO.SeekOrigin.Begin);
            XmlDocument doc = new XmlDocument();
            doc.Load(str);
            str.Close();
            return doc.InnerXml;
        }
    }
}
