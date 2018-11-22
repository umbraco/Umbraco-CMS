using System.Security;
using System.Web;

namespace Umbraco.Core
{
	/// <summary>
	/// Static helper methods for returning information about the current System
	/// </summary>
	public static class SystemUtilities
	{
	    private static bool _knowTrustLevel;
	    private static AspNetHostingPermissionLevel _trustLevel;

		/// <summary>
		/// Get the current trust level of the hosted application
		/// </summary>
		/// <returns></returns>
		public static AspNetHostingPermissionLevel GetCurrentTrustLevel()
		{
		    if (_knowTrustLevel) return _trustLevel;

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

                _trustLevel = trustLevel;
			    _knowTrustLevel = true;
			    return _trustLevel;
			}

			_trustLevel = AspNetHostingPermissionLevel.None;
            _knowTrustLevel = true;
            return _trustLevel;
        }
	}
}