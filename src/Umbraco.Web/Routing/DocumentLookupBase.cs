using System;

namespace Umbraco.Web.Routing
{
	///// <summary>
	///// Abstract DocumentLookup class
	///// </summary>
	//internal abstract class DocumentLookupBase : IDocumentLookup
	//{
	//    public bool TrySetDocument(DocumentRequest docRequest)
	//    {
	//        if (docRequest == null) throw new ArgumentNullException("docRequest");
	//        if (docRequest.RoutingContext == null) throw new ArgumentNullException("docRequest.RoutingContext");
	//        if (docRequest.RoutingContext.UmbracoContext == null) throw new ArgumentNullException("docRequest.UmbracoContext");

	//        return TrySetDocument(docRequest, docRequest.RoutingContext, docRequest.UmbracoContext);
	//    }

	//    protected abstract bool TrySetDocument(DocumentRequest docRequest, RoutingContext routingContext, UmbracoContext umbracoContext);
	//}
}