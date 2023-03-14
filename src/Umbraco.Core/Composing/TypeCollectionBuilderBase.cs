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

    protected abstract TBuilder This { get; }

    public TCollection CreateCollection(IServiceProvider factory)
        => factory.CreateInstance<TCollection>(CreateItemsFactory());

    public void RegisterWith(IServiceCollection services)
        => services.Add(new ServiceDescriptor(typeof(TCollection), CreateCollection, ServiceLifetime.Singleton));

    public TBuilder Add(Type type)
    {
        _types.Add(Validate(type, "add"));
        return This;
    }

    private static Type Validate(Type type, string action)
    {
        if (!typeof(TConstraint).IsAssignableFrom(type))
        {
            throw new InvalidOperationException(
                $"Cannot {action} type {type.FullName} as it does not inherit from/implement {typeof(TConstraint).FullName}.");
        }

        return type;
    }

    public TBuilder Add<T>()
    {
        Add(typeof(T));
        return This;
    }

    public TBuilder Add(IEnumerable<Type> types)
    {
        foreach (Type type in types)
        {
            Add(type);
        }

        return This;
    }

    public TBuilder Remove(Type type)
    {
        _types.Remove(Validate(type, "remove"));
        return This;
    }

    public TBuilder Remove<T>()
    {
        Remove(typeof(T));
        return This;
    }

    // used to resolve a Func<IEnumerable<TItem>> parameter
    private Func<IEnumerable<Type>> CreateItemsFactory() => () => _types;
}
