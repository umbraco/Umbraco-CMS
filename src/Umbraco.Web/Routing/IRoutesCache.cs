namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Represents a bi-directional cache that binds node identifiers and routes.
	/// </summary>
	/// <remarks>
	/// <para>The cache is used both for inbound (map a route to a node) and outbound (map a node to a url).</para>
	/// <para>A route is <c>[rootId]/path/to/node</c> where <c>rootId</c> is the id of the node holding an Umbraco domain, or -1.</para>
	/// </remarks>
	internal interface IRoutesCache
	{
		/// <summary>
		/// Stores a route for a node.
		/// </summary>
		/// <param name="nodeId">The node identified.</param>
		/// <param name="route">The route.</param>
		void Store(int nodeId, string route);

		/// <summary>
		/// Gets a route for a node.
		/// </summary>
		/// <param name="nodeId">The node identifier.</param>
		/// <returns>The route for the node, else null.</returns>
		string GetRoute(int nodeId);

		/// <summary>
		/// Gets a node for a route.
		/// </summary>
		/// <param name="route">The route.</param>
		/// <returns>The node identified for the route, else zero.</returns>
		int GetNodeId(string route);

		/// <summary>
		/// Clears the route for a node.
		/// </summary>
		/// <param name="nodeId">The node identifier.</param>
		void ClearNode(int nodeId);

		/// <summary>
		/// Clears all routes.
		/// </summary>
		void Clear();
	}
}