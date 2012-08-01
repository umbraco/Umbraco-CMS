using System;
using System.Xml.Serialization;
using System.Collections;
using System.Xml.Schema;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Text;

namespace umbraco.BusinessLogic.Utils
{
	/// <summary>
	/// A generic class to inherit from or use by itself so that the serialize/deserialize methods are available to it
	/// </summary>
	public class SerializableData
	{
		public static object Deserialize(string strXML, Type objectType)
		{
			StringReader sr = new StringReader(strXML);
			XmlSerializer xSer = new XmlSerializer(objectType);
			object objSerialized = xSer.Deserialize(sr);
			sr.Close();
			return objSerialized;
		}

		/// <summary>
		/// Generic Serialization method that will serialize object without the default namespaces:
		/// http://www.w3.org/2001/XMLSchema
		/// http://www.w3.org/2001/XMLSchema-instance
        /// This also ensures that the returned XML is always encoded in UTF-8.
		/// </summary>
		/// <param name="objDeserialized"></param>
		/// <param name="objectType"></param>
		/// <returns></returns>
		public static string Serialize(object objDeserialized, Type objectType)
		{
			//create empty namespaces so as to not render the default:
			//xmlns:xsd="http://www.w3.org/2001/XMLSchema"
			//xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            XmlSerializer xSer = new XmlSerializer(objectType);
            EncodedStringWriter sw = new EncodedStringWriter(new StringBuilder(), Encoding.UTF8);
            xSer.Serialize(sw, objDeserialized, ns);
            string str = sw.ToString();
            sw.Close();
            return str;
          
		}        

	}
}
