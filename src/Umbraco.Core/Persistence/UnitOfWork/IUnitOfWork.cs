using System;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Defines a Unit Of Work
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IUnitOfWork<T> : IDisposable
    {
        void Commit();
        T Storage { get; }//TODO This won't work! Need to change it
    }
}