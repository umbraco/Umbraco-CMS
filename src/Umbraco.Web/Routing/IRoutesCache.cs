namespace Umbraco.Web.Routing
{
	internal interface IRoutesCache
	{
		void Store(int nodeId, string route);
		string GetRoute(int nodeId);
		int GetNodeId(string route);
		void ClearNode(int nodeId);
		void Clear();
	}
}