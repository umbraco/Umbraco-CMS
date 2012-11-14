using System.Collections.Generic;
using Umbraco.Web.Models;

namespace Umbraco.Tests.PublishedContent
{
	public static class DynamicPublishedContentCustomExtensionMethods
	{

		public static string DynamicDocumentNoParameters(this DynamicPublishedContent doc)
		{
			return "Hello world";
		}
		
		public static string DynamicDocumentCustomString(this DynamicPublishedContent doc, string custom)
		{
			return custom;
		}

		public static string DynamicDocumentMultiParam(this DynamicPublishedContent doc, string custom, int i, bool b)
		{
			return custom + i + b;
		}
	
		public static string DynamicDocumentListMultiParam(this DynamicPublishedContentList doc, string custom, int i, bool b)
		{
			return custom + i + b;
		}

		public static string DynamicDocumentEnumerableMultiParam(this IEnumerable<DynamicPublishedContent> doc, string custom, int i, bool b)
		{
			return custom + i + b;
		}

	}
}