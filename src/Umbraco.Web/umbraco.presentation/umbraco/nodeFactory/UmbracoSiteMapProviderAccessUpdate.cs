using umbraco.businesslogic;
using umbraco.cms.businesslogic;
using umbraco.interfaces;

namespace umbraco.presentation.nodeFactory
{
	public class UmbracoSiteMapProviderAccessUpdate : IApplicationStartupHandler
	{
		public UmbracoSiteMapProviderAccessUpdate()
		{
			// Add events to security
			if (System.Web.SiteMap.Provider is UmbracoSiteMapProvider)
			{
				cms.businesslogic.web.Access.AfterAddMemberShipRoleToDocument += new global::umbraco.cms.businesslogic.web.Access.AddMemberShipRoleToDocumentEventHandler(Access_AfterAddMemberShipRoleToDocument);
				cms.businesslogic.web.Access.AfterRemoveMemberShipRoleToDocument += new global::umbraco.cms.businesslogic.web.Access.RemoveMemberShipRoleFromDocumentEventHandler(Access_AfterRemoveMemberShipRoleToDocument);
				cms.businesslogic.web.Access.AfterRemoveProtection += new global::umbraco.cms.businesslogic.web.Access.RemoveProtectionEventHandler(Access_AfterRemoveProtection);
			}

		}

		void Access_AfterRemoveProtection(global::umbraco.cms.businesslogic.web.Document sender, RemoveProtectionEventArgs e)
		{
			((UmbracoSiteMapProvider)System.Web.SiteMap.Provider).UpdateNode(new NodeFactory.Node(sender.Id));
		}


		void Access_AfterRemoveMemberShipRoleToDocument(global::umbraco.cms.businesslogic.web.Document sender, string role, RemoveMemberShipRoleFromDocumentEventArgs e)
		{
			((UmbracoSiteMapProvider)System.Web.SiteMap.Provider).UpdateNode(new NodeFactory.Node(sender.Id));
		}

		void Access_AfterAddMemberShipRoleToDocument(global::umbraco.cms.businesslogic.web.Document sender, string role, AddMemberShipRoleToDocumentEventArgs e)
		{
			((UmbracoSiteMapProvider)System.Web.SiteMap.Provider).UpdateNode(new NodeFactory.Node(sender.Id));
		}
	}
}