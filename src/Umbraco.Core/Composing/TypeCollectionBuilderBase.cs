using Microsoft.Extensions.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Provides a base class for collections of types.
/// </summary>
public abstract class
    TypeCollectionBuilderBase<TBuilder, TCollection, TConstraint> : ICollectionBuilder<TCollection, Type>
    where TBuilder : TypeCollectionBuilderBase<TBuilder, TCollection, TConstraint>
    where TCollection : class, IBuilderCollection<Type>
{
    private readonly HashSet<Type> _types = new();

    /// <summary>
    /// Gets the current builder instance.
    /// </summary>
    protected abstract TBuilder This { get; }

    /// <inheritdoc />
    public TCollection CreateCollection(IServiceProvider factory)
        => factory.CreateInstance<TCollection>(CreateItemsFactory());

    /// <inheritdoc />
    public void RegisterWith(IServiceCollection services)
        => services.Add(new ServiceDescriptor(typeof(TCollection), CreateCollection, ServiceLifetime.Singleton));

    /// <summary>
    /// Adds a type to the collection.
    /// </summary>
    /// <param name="type">The type to add.</param>
    /// <returns>The builder.</returns>
    public TBuilder Add(Type type)
    {
        _types.Add(Validate(type, "add"));
        return This;
    }

    /// <summary>
    /// Validates that the specified type is assignable from the constraint type.
    /// </summary>
    /// <param name="type">The type to validate.</param>
    /// <param name="action">The action being performed (used in error messages).</param>
    /// <returns>The validated type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the type does not inherit from or implement <typeparamref name="TConstraint" />.</exception>
    private static Type Validate(Type type, string action)
    {
        if (!typeof(TConstraint).IsAssignableFrom(type))
        {
            throw new InvalidOperationException(
                $"Cannot {action} type {type.FullName} as it does not inherit from/implement {typeof(TConstraint).FullName}.");
        }

        return type;
    }

    /// <summary>
    /// Adds a type to the collection.
    /// </summary>
    /// <typeparam name="T">The type to add.</typeparam>
    /// <returns>The builder.</returns>
    public TBuilder Add<T>()
    {
        Add(typeof(T));
        return This;
    }

    /// <summary>
    /// Adds multiple types to the collection.
    /// </summary>
    /// <param name="types">The types to add.</param>
    /// <returns>The builder.</returns>
    public TBuilder Add(IEnumerable<Type> types)
    {
        foreach (Type type in types)
        {
            Add(type);
        }

        return This;
    }

    /// <summary>
    /// Removes a type from the collection.
    /// </summary>
    /// <param name="type">The type to remove.</param>
    /// <returns>The builder.</returns>
    public TBuilder Remove(Type type)
    {
        _types.Remove(Validate(type, "remove"));
        return This;
    }

    /// <summary>
    /// Removes a type from the collection.
    /// </summary>
    /// <typeparam name="T">The type to remove.</typeparam>
    /// <returns>The builder.</returns>
    public TBuilder Remove<T>()
    {
        Remove(typeof(T));
        return This;
    }

    /// <summary>
    /// Creates a factory function that returns the types in the collection.
    /// </summary>
    /// <returns>A function that returns the collection of types.</returns>
    private Func<IEnumerable<Type>> CreateItemsFactory() => () => _types;
}
