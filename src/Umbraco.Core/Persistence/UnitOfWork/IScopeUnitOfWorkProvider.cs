using System.Data;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Defines a Unit of Work Provider for working with <see cref="IScope"/>
    /// </summary>
    public interface IScopeUnitOfWorkProvider : IDatabaseUnitOfWorkProvider
    {
        new IScopeUnitOfWork GetUnitOfWork();
        IScopeUnitOfWork GetUnitOfWork(IsolationLevel isolationLevel);
    }
}