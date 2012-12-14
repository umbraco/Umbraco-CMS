using System.Data;
using System.Data.Common;

namespace Umbraco.Core.Persistence
{
	/// <summary>
	/// Represents the Umbraco implementation of the PetaPoco Database object
	/// </summary>
	/// <remarks>
	/// Currently this object exists for 'future proofing' our implementation. By having our own inheritied implementation we 
	/// can then override any additional execution (such as additional loggging, functionality, etc...) that we need to without breaking compatibility since we'll always be exposing
	/// this object instead of the base PetaPoco database object.	
	/// </remarks>
	public class UmbracoDatabase : Database
	{
		public UmbracoDatabase(IDbConnection connection) : base(connection)
		{
		}

		public UmbracoDatabase(string connectionString, string providerName) : base(connectionString, providerName)
		{
		}

		public UmbracoDatabase(string connectionString, DbProviderFactory provider) : base(connectionString, provider)
		{
		}

		public UmbracoDatabase(string connectionStringName) : base(connectionStringName)
		{
		}
	}
}