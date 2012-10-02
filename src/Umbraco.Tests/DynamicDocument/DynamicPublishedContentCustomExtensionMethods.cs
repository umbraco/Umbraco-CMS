using System.Collections.Generic;
using Umbraco.Core.Dynamics;

namespace Umbraco.Tests.DynamicDocument
{
	public static class DynamicPublishedContentCustomExtensionMethods
	{

		public static string DynamicDocumentNoParameters(this Core.Dynamics.DynamicPublishedContent doc)
		{
			return "Hello world";
		}
		
		public static string DynamicDocumentCustomString(this Core.Dynamics.DynamicPublishedContent doc, string custom)
		{
			return custom;
		}

		public static string DynamicDocumentMultiParam(this Core.Dynamics.DynamicPublishedContent doc, string custom, int i, bool b)
		{
			return custom + i + b;
		}
	
		public static string DynamicDocumentListMultiParam(this DynamicPublishedContentList doc, string custom, int i, bool b)
		{
			return custom + i + b;
		}

		public static string DynamicDocumentEnumerableMultiParam(this IEnumerable<Core.Dynamics.DynamicPublishedContent> doc, string custom, int i, bool b)
		{
			return custom + i + b;
		}

	}
}