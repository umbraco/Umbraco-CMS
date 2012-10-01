using Umbraco.Core.Models;

namespace Umbraco.Web
{
	/// <summary>
	/// Defines the methods to access published content
	/// </summary>
	internal interface IPublishedContentStore : IPublishedStore
	{		
		IPublishedContent GetDocumentByRoute(UmbracoContext umbracoContext, string route, bool? hideTopLevelNode = null);
		IPublishedContent GetDocumentByUrlAlias(UmbracoContext umbracoContext, int rootNodeId, string alias);
        bool HasContent(UmbracoContext umbracoContext);
	}
}