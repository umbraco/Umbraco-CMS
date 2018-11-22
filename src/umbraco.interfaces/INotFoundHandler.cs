using System;

namespace umbraco.interfaces
{
	/// <summary>
	/// Summary description for INotFoundHandler.
	/// </summary>
	public interface INotFoundHandler
	{
		bool Execute(string url);
		bool CacheUrl {get;}
		int redirectID {get;}
	}
}
