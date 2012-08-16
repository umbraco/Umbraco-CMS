using Umbraco.Core.Models;

namespace Umbraco.Web
{
	internal interface IPublishedContentStore
	{
		IDocument GetDocumentById(UmbracoContext umbracoContext, int nodeId);
		IDocument GetDocumentByRoute(UmbracoContext umbracoContext, string route, bool? hideTopLevelNode = null);
		IDocument GetDocumentByUrlAlias(UmbracoContext umbracoContext, int rootNodeId, string alias);
		string GetDocumentProperty(UmbracoContext umbracoContext, IDocument node, string propertyAlias);
	}
}