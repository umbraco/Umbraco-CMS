using System.Collections.Generic;
using umbraco.cms.businesslogic;

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
				var sql = "SELECT contentNodeId FROM cmsPropertyData WHERE id = @propertyId";
				return uQuery.SqlHelper.ExecuteScalar<int>(sql, uQuery.SqlHelper.CreateParameter("@propertyId", propertyId));
			}

			return uQuery.RootNodeId;
		}
	}
}