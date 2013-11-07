using System.Collections.Generic;
using umbraco.cms.businesslogic;
using Umbraco.Core;

namespace umbraco
{
	public static partial class uQuery
	{
		/// <summary>
		/// Gets the <c>Content</c> Id by property Id.
		/// </summary>
		/// <param name="propertyId">The property id.</param>
		/// <returns>Returns the <c>Content</c> Id.</returns>
		public static int GetContentIdByPropertyId(int propertyId)
		{
			if (propertyId > 0)
			{
                return ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int>(
                    "SELECT contentNodeId FROM cmsPropertyData WHERE id = @propertyId",
                    new { propertyId = propertyId });
			}

			return uQuery.RootNodeId;
		}
	}
}