using System;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Xml;
using System.Reflection;

namespace umbraco.presentation.xslt.Exslt
{
	/// <summary>
	/// This class implements the EXSLT functions in the http://exslt.org/common namespace.
	/// </summary>
	public class ExsltCommon
	{
		
/// <summary>
/// Implements the following function
///  string exsl:objecttype(object) 
/// </summary>
/// <param name="o"></param>
/// <returns></returns>
		public static string objecttype(object o){

			if(o is System.String){
				return "string";
			}else if(o is System.Boolean){
				return "boolean";
			}else if(o is Double || o is Int16 || o is UInt16 || o is Int32
				|| o is UInt32 || o is Int64 || o is UInt64 || o is Single || o is Decimal){
				return "number"; 
			}else if(o is System.Xml.XPath.XPathNavigator){
				return "RTF"; 
			}else if(o is System.Xml.XPath.XPathNodeIterator){
				return "node-set"; 
			}else{
				return "external"; 
			}

		}/* objecttype(object) */

/// <summary>
/// This method converts an ExsltNodeList to an XPathNodeIterator over the nodes in the list.
/// </summary>
/// <param name="list">The list to convert</param>
/// <returns>An XPathNodeIterator over the nodes in the original list</returns>
/// <remarks>Known Issues: A node list containing multiple instances of an attribute 
/// with the same namespace name and local name will cause an error.</remarks>
		internal static XPathNodeIterator ExsltNodeListToXPathNodeIterator(ExsltNodeList list){
		

			Assembly systemXml = typeof(XPathNodeIterator).Assembly; 
			Type arrayIteratorType = systemXml.GetType("System.Xml.XPath.XPathArrayIterator"); 

			return (XPathNodeIterator) Activator.CreateInstance( arrayIteratorType, 
				BindingFlags.Instance | BindingFlags.Public | 
				BindingFlags.CreateInstance, null, new object[]{ list.innerList}, null ); 

		}


	
		
	}
}
