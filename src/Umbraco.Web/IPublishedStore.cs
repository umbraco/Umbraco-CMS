using Umbraco.Core.Models;

namespace Umbraco.Web
{
	/// <summary>
	/// Defines the methods for published documents
	/// </summary>
	internal interface IPublishedStore
	{
		IDocument GetDocumentById(UmbracoContext umbracoContext, int nodeId);
	}
}