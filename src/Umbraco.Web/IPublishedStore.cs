using Umbraco.Core.Models;

namespace Umbraco.Web
{
	/// <summary>
	/// Defines the methods for published documents
	/// </summary>
	public interface IPublishedStore
	{
		IPublishedContent GetDocumentById(UmbracoContext umbracoContext, int nodeId);
	}
}