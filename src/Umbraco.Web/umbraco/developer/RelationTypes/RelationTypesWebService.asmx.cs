using System.Web.Services;

namespace umbraco.cms.presentation.developer.RelationTypes
{
	/// <summary>
	/// Webservice to delete relation types, this allows deletion via a javacscript call hooked into the tree UI
	/// </summary>
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	[System.Web.Script.Services.ScriptService] // Allows this Web Service to be called from script, using ASP.NET AJAX
	public class RelationTypesWebService : System.Web.Services.WebService
	{
		/// <summary>
		/// Delete an Umbraco RelationType and all it's associated Relations
		/// </summary>
		/// <param name="relationTypeId">database id of the relation type to delete</param>
		[WebMethod]
		public void DeleteRelationType(int relationTypeId)
		{
			// Check user calling this service is of type administrator
			umbraco.BusinessLogic.User user = umbraco.BusinessLogic.User.GetCurrent();
			if (user.UserType.Name == "Administrators")
			{
				// Delete all relations for this relation type!
				uQuery.SqlHelper.ExecuteNonQuery(string.Format("DELETE FROM umbracoRelation WHERE relType = {0}", relationTypeId.ToString()));

				// Delete relation type
				uQuery.SqlHelper.ExecuteNonQuery(string.Format("DELETE FROM umbracoRelationType WHERE id = {0}", relationTypeId.ToString()));
			}
		}
	}
}
