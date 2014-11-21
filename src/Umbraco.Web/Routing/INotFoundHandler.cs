namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Summary description for INotFoundHandler.
	/// </summary>
	public interface INotFoundHandler
	{
		bool Execute(string url);
		bool CacheUrl {get;}
		int RedirectId {get;}
	}
}
