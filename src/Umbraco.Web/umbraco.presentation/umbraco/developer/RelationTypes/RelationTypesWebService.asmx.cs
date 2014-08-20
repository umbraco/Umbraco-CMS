using System.Web.Services;
using Umbraco.Core;
using Umbraco.Web;

namespace umbraco.cms.presentation.developer.RelationTypes
{
	/// <summary>
	/// Webservice to delete relation types, this allows deletion via a javacscript call hooked into the tree UI
	/// </summary>
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	[System.Web.Script.Services.ScriptService] // Allows this Web Service to be called from script, using ASP.NET AJAX
	public class RelationTypesWebService : WebService
	{
		/// <summary>
		/// Delete an Umbraco RelationType and all it's associated Relations
		/// </summary>
		/// <param name="relationTypeId">database id of the relation type to delete</param>
		[WebMethod]
		public void DeleteRelationType(int relationTypeId)
		{
		    var user = UmbracoContext.Current.Security.CurrentUser;
            
			if (user.UserType.Name == "Administrators")
			{
                var relationService = ApplicationContext.Current.Services.RelationService;
			    var relationType = relationService.GetRelationTypeById(relationTypeId);
			    relationService.Delete(relationType);
			}
		}
	}
}
