using System;
using System.Linq;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.propertytype;

namespace umbraco
{
	/// <summary>
	/// uQuery extensions for the ContentType object.
	/// </summary>
	public static class ContentTypeExtensions
	{
		/// <summary>
		/// Gets the type of the property, regardless of the casing of the alias name.
		/// </summary>
		/// <param name="contentType">Type of the content.</param>
		/// <param name="alias">The alias.</param>
		/// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
		/// <returns></returns>
		public static PropertyType getPropertyType(this ContentType contentType, string alias, bool ignoreCase)
		{
			if (ignoreCase)
			{
				return contentType.PropertyTypes.FirstOrDefault(pt => string.Equals(pt.Alias, alias, StringComparison.OrdinalIgnoreCase));
			}

			return contentType.getPropertyType(alias);
		}
	}
}