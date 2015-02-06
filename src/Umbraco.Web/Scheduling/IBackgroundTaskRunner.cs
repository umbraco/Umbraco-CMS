using System;
using System.Web.Hosting;

namespace Umbraco.Web.Scheduling
{
    /// <summary>
    /// Defines a service managing a queue of tasks of type <typeparamref name="T"/> and running them in the background.
    /// </summary>
    /// <typeparam name="T">The type of the managed tasks.</typeparam>
    /// <remarks>The interface is not complete and exists only to have the contravariance on T.</remarks>
    internal interface IBackgroundTaskRunner<in T> : IDisposable, IRegisteredObject
        where T : class, IBackgroundTask
    {
        bool IsCompleted { get; }
        void Add(T task);
        bool TryAdd(T task);

        // fixme - complete the interface?
    }
}