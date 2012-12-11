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

	    public IDatabaseUnitOfWork GetUnitOfWork()
        {
			return new PetaPocoUnitOfWork(DatabaseFactory.Current.Database);
        }

        #endregion
    }
}