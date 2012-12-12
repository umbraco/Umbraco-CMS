using System;
using System.Threading;
using System.Web;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Represents a Unit of Work Provider for creating a <see cref="PetaPocoUnitOfWork"/>
    /// </summary>
    internal class PetaPocoUnitOfWorkProvider : IDatabaseUnitOfWorkProvider
    {
	    
	    #region Implementation of IUnitOfWorkProvider

		/// <summary>
		/// Creates a Unit of work with a new UmbracoDatabase instance for the work item/transaction.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Each PetaPoco UOW uses it's own Database object, not the shared Database object that comes from
		/// the DatabaseContext.Current.Database. This is because each transaction should use it's own Database
		/// and we Dispose of this Database object when the UOW is disposed.
		/// </remarks>
	    public IDatabaseUnitOfWork GetUnitOfWork()
        {
			return new PetaPocoUnitOfWork(
				new UmbracoDatabase(
					GlobalSettings.UmbracoConnectionName));
        }

        #endregion
    }
}