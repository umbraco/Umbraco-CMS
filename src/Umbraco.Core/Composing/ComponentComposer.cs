﻿using Umbraco.Core.Compose;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Provides a base class for composers which compose a component.
    /// </summary>
    /// <typeparam name="TComponent">The type of the component</typeparam>
    public abstract class ComponentComposer<TComponent> : IComposer
        where TComponent : IComponent
    {
        /// <inheritdoc />
        public virtual void Compose(Composition composition)
        {
            composition.Components().Append<TComponent>();
        }

        // note: thanks to this class, a component that does not compose anything can be
        // registered with one line:
        // public class MyComponentComposer : ComponentComposer<MyComponent> { }
    }
}
