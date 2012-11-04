using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Web
{
	/// <summary>
	/// Defines the methods for published documents
	/// </summary>
	internal interface IPublishedStore
	{
		IPublishedContent GetDocumentById(UmbracoContext umbracoContext, int nodeId);
		IEnumerable<IPublishedContent> GetRootDocuments(UmbracoContext umbracoContext);
	}
}