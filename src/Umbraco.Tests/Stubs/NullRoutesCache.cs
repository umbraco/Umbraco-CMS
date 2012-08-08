using Umbraco.Web.Routing;

namespace Umbraco.Tests.Stubs
{
	/// <summary>
	/// Used for testing, does not cache anything
	/// </summary>
	public class NullRoutesCache : IRoutesCache
	{
		public void Store(int nodeId, string route)
		{

		}

		public string GetRoute(int nodeId)
		{
			return null; //default;
		}

		public int GetNodeId(string route)
		{
			return 0; //default;
		}

		public void ClearNode(int nodeId)
		{

		}

		public void Clear()
		{

		}
	}
}