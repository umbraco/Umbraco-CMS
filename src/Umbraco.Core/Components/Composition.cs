using System;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Core.Components
{
    /// <summary>
    /// Represents a composition.
    /// </summary>
    /// <remarks>Although a composition exposes the application's service container, people should use the
    /// extension methods (such as <c>PropertyEditors()</c> or <c>SetPublishedContentModelFactory()</c>) and
    /// avoid accessing the container. This is because everything needs to be properly registered and with
    /// the proper lifecycle. These methods will take care of it. Directly registering into the container
    /// may cause issues.</remarks>
    public class Composition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Composition"/> class.
        /// </summary>
        /// <param name="container">A container.</param>
        /// <param name="level">The runtime level.</param>
        public Composition(IServiceCollection services, IServiceProvider container, RuntimeLevel level)
        {
            Services = services;
            Container = container;
            RuntimeLevel = level;
        }

        /// <summary>
        /// Gets the container.
        /// </summary>
        /// <remarks>Use with care!</remarks>
        public IServiceProvider Container { get; }

        public IServiceCollection Services { get; }

        /// <summary>
        /// Gets the runtime level.
        /// </summary>
        public RuntimeLevel RuntimeLevel { get; }
    }
}
