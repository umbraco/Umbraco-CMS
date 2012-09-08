using Umbraco.Core.Models;

namespace Umbraco.Web
{
	/// <summary>
	/// Defines the methods to access published media
	/// </summary>
	internal interface IPublishedMediaStore
	{
		IDocument GetDocumentById(UmbracoContext umbracoContext, int nodeId);
		string GetDocumentProperty(UmbracoContext umbracoContext, IDocument node, string propertyAlias);
	}
}