using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Umbraco.Core
{
	internal static class NameValueCollectionExtensions
	{

		public static bool ContainsKey(this NameValueCollection collection, string key)
		{
			return collection.Keys.Cast<object>().Any(k => (string) k == key);
		}
	}
}
