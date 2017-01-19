using System.Data;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.UnitOfWork
{
	/// <summary>
	/// Defines a Unit of Work Provider for working with an IDatabaseUnitOfWork
	/// </summary>
	public interface IDatabaseUnitOfWorkProvider
	{
		IDatabaseUnitOfWork GetUnitOfWork();
    }
    
    /// <summary>
	/// Defines a Unit of Work Provider for working with <see cref="IScope"/>
	/// </summary>
	internal interface IScopeUnitOfWorkProvider : IDatabaseUnitOfWorkProvider
    {
        IDatabaseUnitOfWork GetUnitOfWork(IsolationLevel isolationLevel);
    }
}