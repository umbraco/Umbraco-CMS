using System;

namespace Umbraco.Core.Persistence
{
	/// <summary>
	/// Used to create the UmbracoDatabase for use in the DatabaseContext
	/// </summary>
	public interface IDatabaseFactory : IDisposable
	{
		UmbracoDatabase CreateDatabase();
	}
}