using System.IO;
using System.Xml.Serialization;

namespace umbraco.editorControls.imagecropper
{
    class Helper
    {


        public static string SerializeToString(object obj)
        {
            XmlSerializer serializer = new XmlSerializer(obj.GetType());

            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);

                return writer.ToString();
            }
        }
    }
}