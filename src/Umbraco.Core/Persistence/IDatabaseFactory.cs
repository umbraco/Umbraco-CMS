using System;

namespace Umbraco.Core.Persistence
{
	/// <summary>
	/// Used to create the UmbracoDatabase for use in the DatabaseContext
	/// </summary>
	public interface IDatabaseFactory : IDisposable
	{
        // gets or creates the ambient database
		UmbracoDatabase CreateDatabase();
	}

    public interface IDatabaseFactory2 : IDatabaseFactory
    {
        // creates a new database
        UmbracoDatabase CreateNewDatabase();
    }
}