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
		
		//TODO: SD: We should make this happen! This will allow us to natively do a GetByDocumentType query
		// on the UmbracoHelper (or an internal DataContext that it uses, etc...)
		// One issue is that we need to make media work as fast as we can and need to create a ConvertFromMediaObject
		// method in the DefaultPublishedMediaStore, there's already a TODO noting this but in order to do that we'll 
		// have to also use Examine as much as we can so we don't have to make db calls for looking up things like the 
		// node type alias, etc... in order to populate the created IPublishedContent object.
		//IEnumerable<IPublishedContent> GetDocumentsByType(string docTypeAlias);
	}
}