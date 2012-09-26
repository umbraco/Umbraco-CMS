using Umbraco.Core.Models;

namespace Umbraco.Web
{
	/// <summary>
	/// Defines the methods to access published content
	/// </summary>
	public interface IPublishedContentStore : IPublishedStore
	{		
		IDocument GetDocumentByRoute(UmbracoContext umbracoContext, string route, bool? hideTopLevelNode = null);
		IDocument GetDocumentByUrlAlias(UmbracoContext umbracoContext, int rootNodeId, string alias);
	}
}