using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using umbraco.interfaces;

namespace Umbraco.Core
{
	public static class DocumentExtensions
	{

		public static dynamic AsDynamic(this IDocument doc)
		{
			var dd = new DynamicDocument(doc);
			return dd.AsDynamic();
		}

		/// <summary>
		/// Returns the property as the specified type, if the property is not found or does not convert
		/// then the default value of type T is returned.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="prop"></param>
		/// <param name="alias"></param>
		/// <returns></returns>
		public static T GetPropertyValue<T>(this IDocument prop, string alias)
		{
			return prop.GetPropertyValue<T>(alias, default(T));
		}

		public static T GetPropertyValue<T>(this IDocument prop, string alias, T ifCannotConvert)
		{
			var p = prop.GetProperty(alias);
			if (p == null)
				return default(T);
			var converted = p.Value.TryConvertTo<T>();
			if (converted.Success)
				return converted.Result;
			return ifCannotConvert;
		}
		
	}
}