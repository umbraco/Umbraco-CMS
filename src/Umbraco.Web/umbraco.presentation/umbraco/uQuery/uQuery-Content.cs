using System.Collections.Generic;
using umbraco.BusinessLogic;
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
                using (var sqlHelper = Application.SqlHelper)
                    return sqlHelper.ExecuteScalar<int>(sql, sqlHelper.CreateParameter("@propertyId", propertyId));
			}

			return uQuery.RootNodeId;
		}
	}
}