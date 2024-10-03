using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Core.Composing;

/// <summary>
/// Provides a composer that appends a component.
/// </summary>
/// <typeparam name="TComponent">The type of the component.</typeparam>
/// <remarks>
/// Thanks to this class, a component that does not compose anything can be registered with one line:
/// <code>
/// <![CDATA[
/// public class MyComponentComposer : ComponentComposer<MyComponent> { }
/// ]]>
/// </code>
/// </remarks>
public abstract class ComponentComposer<TComponent> : IComposer
    where TComponent : IAsyncComponent
{
    /// <inheritdoc />
    public virtual void Compose(IUmbracoBuilder builder)
        => builder.Components().Append<TComponent>();
}
