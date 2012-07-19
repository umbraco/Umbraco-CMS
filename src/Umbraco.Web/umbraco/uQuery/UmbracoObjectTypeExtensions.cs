using System;

namespace umbraco
{
	/// <summary>
	/// uQuery extensions for Umbraco object types.
	/// </summary>
	public static class UmbracoObjectTypeExtensions
	{
		/// <summary>
		/// Extension method for the UmbracoObjectType enum to return the enum GUID
		/// </summary>
		/// <param name="umbracoObjectType">UmbracoObjectType Enum value</param>
		/// <returns>a GUID value of the UmbracoObjectType</returns>
		public static Guid GetGuid(this uQuery.UmbracoObjectType umbracoObjectType)
		{
			var fieldInfo = umbracoObjectType.GetType().GetField(umbracoObjectType.ToString());
			var guidAttributes = (uQuery.GuidAttribute[])fieldInfo.GetCustomAttributes(typeof(uQuery.GuidAttribute), false);
			return (guidAttributes.Length > 0) ? new Guid(guidAttributes[0].ToString()) : Guid.Empty;
		}

		/// <summary>
		/// Extension method for the UmbracoObjectType enum to return the enum name
		/// </summary>
		/// <param name="umbracoObjectType">UmbracoObjectType value</param>
		/// <returns>The enum name of the UmbracoObjectType value</returns>
		public static string GetName(this uQuery.UmbracoObjectType umbracoObjectType)
		{
			return Enum.GetName(typeof(uQuery.UmbracoObjectType), umbracoObjectType);
		}

		/// <summary>
		/// Extension method for the UmbracoObejctType enum to return the enum friendly name
		/// </summary>
		/// <param name="umbracoObjectType">UmbracoObjectType value</param>
		/// <returns>a string of the FriendlyName</returns>
		public static string GetFriendlyName(this uQuery.UmbracoObjectType umbracoObjectType)
		{
			var fieldInfo = umbracoObjectType.GetType().GetField(umbracoObjectType.ToString());
			var friendlyNames = (uQuery.FriendlyNameAttribute[])fieldInfo.GetCustomAttributes(typeof(uQuery.FriendlyNameAttribute), false);
			return (friendlyNames.Length > 0) ? friendlyNames[0].ToString() : string.Empty;
		}
	}
}