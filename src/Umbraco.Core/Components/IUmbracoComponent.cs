using System;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Components
{
    /// <summary>
    /// Represents an Umbraco component.
    /// </summary>
    public interface IUmbracoComponent : IDiscoverable
    {
        /// <summary>
        /// Composes the component.
        /// </summary>
        /// <param name="composition">The composition.</param>
        void Compose(Composition composition);

        /// <summary>
        /// An optional <see cref="IComponentInitializer"/> that will be executed after composition is done.
        /// The initializer will be registered with the DI container and can have constructor dependencies.
        /// </summary>
        Type InitializerType { get; }

        /// <summary>
        /// Terminates the component.
        /// </summary>
        void Terminate();
    }

    public interface IComponentInitializer
    {
        void Initialize();
    }
}
