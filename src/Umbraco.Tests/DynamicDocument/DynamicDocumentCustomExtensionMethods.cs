using System.Collections.Generic;
using Umbraco.Core.Dynamics;

namespace Umbraco.Tests.DynamicDocument
{
	public static class DynamicDocumentCustomExtensionMethods
	{

		public static string DynamicDocumentNoParameters(this Core.Dynamics.DynamicDocument doc)
		{
			return "Hello world";
		}
		
		public static string DynamicDocumentCustomString(this Core.Dynamics.DynamicDocument doc, string custom)
		{
			return custom;
		}

		public static string DynamicDocumentMultiParam(this Core.Dynamics.DynamicDocument doc, string custom, int i, bool b)
		{
			return custom + i + b;
		}
	
		public static string DynamicDocumentListMultiParam(this DynamicDocumentList doc, string custom, int i, bool b)
		{
			return custom + i + b;
		}

		public static string DynamicDocumentEnumerableMultiParam(this IEnumerable<Core.Dynamics.DynamicDocument> doc, string custom, int i, bool b)
		{
			return custom + i + b;
		}

	}
}