using Umbraco.Core.Models;

namespace Umbraco.Web
{
	internal interface IContentStore
	{
		IDocument GetDocumentById(int nodeId);
		IDocument GetDocumentByRoute(string route, bool? hideTopLevelNode = null);
		IDocument GetDocumentByUrlAlias(int rootNodeId, string alias);
		string GetDocumentProperty(IDocument node, string propertyAlias);
	}
}