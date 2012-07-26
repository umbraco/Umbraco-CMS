namespace Umbraco.Web.Routing
{
	// this is a bi-directional cache that contains
	// - nodeId to route (used for NiceUrl)
	// - route to nodeId (used for inbound requests)
	//
	// a route is [rootId]/path/to/node
	// where rootId is the id of the "site root" node
	//

	internal interface IRoutesCache
	{
		void Store(int nodeId, string route);
		string GetRoute(int nodeId);
		int GetNodeId(string route);
		void ClearNode(int nodeId);
		void Clear();
	}
}