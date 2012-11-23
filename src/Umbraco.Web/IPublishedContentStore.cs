using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.Models;

namespace Umbraco.Web
{
	/// <summary>
	/// Defines the methods to access published content
	/// </summary>
	[UmbracoExperimentalFeature("http://issues.umbraco.org/issue/U4-1153", "We need to create something like the IPublishListener interface to have proper published content storage. We'll also need to publicize the resolvers so people can set a resolver at app startup.")]
	internal interface IPublishedContentStore : IPublishedStore
	{		
		IPublishedContent GetDocumentByRoute(UmbracoContext umbracoContext, string route, bool? hideTopLevelNode = null);
		IPublishedContent GetDocumentByUrlAlias(UmbracoContext umbracoContext, int rootNodeId, string alias);
        bool HasContent(UmbracoContext umbracoContext);
	}
}