using Umbraco.Core.Models;

namespace Umbraco.Core
{
	internal static class DocumentPropertyExtensions
	{
		/// <summary>
		/// Returns the property as the specified type, if the property is not found or does not convert
		/// then the default value of type T is returned.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="prop"></param>
		/// <param name="alias"></param>
		/// <returns></returns>
		public static T GetProperty<T>(this IDocument prop, string alias)
		{
			var p = prop.GetProperty(alias);
			if (p == null)
				return default(T);
			var converted = p.TryConvertTo<T>();
			if (converted.Success)
				return converted.Result;
			return default(T);
		}
	}
}