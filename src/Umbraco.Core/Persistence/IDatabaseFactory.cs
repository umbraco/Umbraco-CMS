using System;
using System.ComponentModel;

namespace Umbraco.Core.Persistence
{
    [Obsolete("Use IDatabaseFactory2 instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IDatabaseFactory : IDisposable
	{
        /// <summary>
        /// gets or creates the ambient database
        /// </summary>
        /// <returns></returns>
		UmbracoDatabase CreateDatabase();
	}

    /// <summary>
	/// Used to create the UmbracoDatabase for use in the DatabaseContext
	/// </summary>
#pragma warning disable 618
    public interface IDatabaseFactory2 : IDatabaseFactory
#pragma warning restore 618
    {
        /// <summary>
        /// creates a new database
        /// </summary>
        /// <returns></returns>
        UmbracoDatabase CreateNewDatabase();
    }
}