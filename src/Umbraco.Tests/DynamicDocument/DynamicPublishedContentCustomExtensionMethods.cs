using System.Collections.Generic;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;

namespace Umbraco.Tests.DynamicDocument
{
	public static class DynamicPublishedContentCustomExtensionMethods
	{

		public static string DynamicDocumentNoParameters(this DynamicPublishedContentBase doc)
		{
			return "Hello world";
		}
		
		public static string DynamicDocumentCustomString(this DynamicPublishedContentBase doc, string custom)
		{
			return custom;
		}

		public static string DynamicDocumentMultiParam(this DynamicPublishedContentBase doc, string custom, int i, bool b)
		{
			return custom + i + b;
		}
	
		public static string DynamicDocumentListMultiParam(this DynamicPublishedContentList doc, string custom, int i, bool b)
		{
			return custom + i + b;
		}

		public static string DynamicDocumentEnumerableMultiParam(this IEnumerable<DynamicPublishedContentBase> doc, string custom, int i, bool b)
		{
			return custom + i + b;
		}

	}
}