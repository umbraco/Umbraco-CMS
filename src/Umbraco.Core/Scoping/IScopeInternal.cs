using System.Data;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Scoping
{
    /// <summary>
    /// Provides additional, internal scope functionnalities.
    /// </summary>
    internal interface IScopeInternal : IScope // fixme - define what's internal and why
    {
        /// <summary>
        /// Gets the parent scope, if any, or null.
        /// </summary>
        IScopeInternal ParentScope { get; }

        /// <summary>
        /// Gets a value indicating whether this scope should be registered in
        /// call context even when an http context is available.
        /// </summary>
        bool CallContext { get; }

        /// <summary>
        /// Gets the scope transaction isolation level.
        /// </summary>
        IsolationLevel IsolationLevel { get; }

        /// <summary>
        /// Gets the scope database, if any, else null.
        /// </summary>
        IUmbracoDatabase DatabaseOrNull { get; }

        /// <summary>
        /// Get the scope event messages, if any, else null.
        /// </summary>
        EventMessages MessagesOrNull { get; }

        /// <summary>
        /// Gets a value indicating whether filesystems are scoped.
        /// </summary>
        bool ScopedFileSystems { get; }

        /// <summary>
        /// Registers that a child has completed.
        /// </summary>
        /// <param name="completed">The child's completion status.</param>
        /// <remarks>Completion status can be true (completed), false (could not complete), or null (not properly exited).</remarks>
        void ChildCompleted(bool? completed);

        /// <summary>
        /// Resets the scope.
        /// </summary>
        /// <remarks>Reset completion to "unspecified".</remarks>
        void Reset();
    }
}
