using System.Security;
using System.Web;

namespace Umbraco.Core
{
	/// <summary>
	/// Static helper methods for returning information about the current System
	/// </summary>
	public static class SystemUtilities
	{

		/// <summary>
		/// Get the current trust level of the hosted application
		/// </summary>
		/// <returns></returns>
		public static AspNetHostingPermissionLevel GetCurrentTrustLevel()
		{
			foreach (var trustLevel in new[] {
			                                 	AspNetHostingPermissionLevel.Unrestricted,
			                                 	AspNetHostingPermissionLevel.High,
			                                 	AspNetHostingPermissionLevel.Medium,
			                                 	AspNetHostingPermissionLevel.Low,
			                                 	AspNetHostingPermissionLevel.Minimal })
			{
				try
				{
					new AspNetHostingPermission(trustLevel).Demand();
				}
				catch (SecurityException)
				{
					continue;
				}

				return trustLevel;
			}

			return AspNetHostingPermissionLevel.None;
		}
	}
}