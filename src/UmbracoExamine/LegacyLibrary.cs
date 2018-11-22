using System;
using System.Reflection;
using System.Xml.XPath;

namespace UmbracoExamine
{
	/// <summary>
	/// This is only used for backward compatibility to get access to the umbraco.library object but this needs to be done
	/// via reflection because of the circular reference we have between Umbraco.Web and UmbracoExamine. 
	/// </summary>
	internal static class LegacyLibrary
	{
		private static volatile Type _libraryType;
		private static readonly object Locker = new object();
		private static Type LibraryType
		{
			get
			{
				if (_libraryType == null)
				{
					lock (Locker)
					{
						if (_libraryType == null)
						{
							var ass = Assembly.Load("umbraco");
							if (ass == null)
								throw new InvalidOperationException("Could not load assembly umbraco.dll, the umbraco.dll needs to be loaded in the current app domain");
							var lib = ass.GetType("umbraco.library");
							if (lib == null)
								throw new InvalidOperationException("Could not load type umbraco.library, the umbraco.dll needs to be loaded in the current app domain");
							_libraryType = lib;
						}
					}
				}
				return _libraryType;
			}
		}

		internal static XPathNodeIterator GetXmlNodeById(string id)
		{
			var meth = LibraryType.GetMethod("GetXmlNodeById", BindingFlags.Public | BindingFlags.Static);
			return (XPathNodeIterator)meth.Invoke(null, new object[] { id });
		}

		internal static XPathNodeIterator GetMember(int id)
		{
			var meth = LibraryType.GetMethod("GetMember", BindingFlags.Public | BindingFlags.Static);
			return (XPathNodeIterator)meth.Invoke(null, new object[] { id });
		}

		internal static XPathNodeIterator GetXmlNodeByXPath(string xpathQuery)
		{
			var meth = LibraryType.GetMethod("GetXmlNodeByXPath", BindingFlags.Public | BindingFlags.Static);
			return (XPathNodeIterator)meth.Invoke(null, new object[] { xpathQuery });
		}
		
	}
}