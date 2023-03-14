namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Implements a lazy collection builder.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
/// <typeparam name="TCollection">The type of the collection.</typeparam>
/// <typeparam name="TItem">The type of the items.</typeparam>
/// <remarks>
///     This type of collection builder is typically used when type scanning is required (i.e. plugins).
/// </remarks>
public abstract class
    LazyCollectionBuilderBase<TBuilder, TCollection, TItem> : CollectionBuilderBase<TBuilder, TCollection, TItem>
    where TBuilder : LazyCollectionBuilderBase<TBuilder, TCollection, TItem>
    where TCollection : class, IBuilderCollection<TItem>
{
    private readonly List<Type> _excluded = new();
    private readonly List<Func<IEnumerable<Type>>> _producers = new();

    protected abstract TBuilder This { get; }

    /// <summary>
    ///     Clears all types in the collection.
    /// </summary>
    /// <returns>The builder.</returns>
    public TBuilder Clear()
    {
        Configure(types =>
        {
            types.Clear();
            _producers.Clear();
            _excluded.Clear();
        });
        return This;
    }

    /// <summary>
    ///     Adds a type to the collection.
    /// </summary>
    /// <typeparam name="T">The type to add.</typeparam>
    /// <returns>The builder.</returns>
    public TBuilder Add<T>()
        where T : TItem
    {
        Configure(types =>
        {
            Type type = typeof(T);
            if (types.Contains(type) == false)
            {
                types.Add(type);
            }
        });
        return This;
    }

    /// <summary>
    ///     Adds a type to the collection.
    /// </summary>
    /// <param name="type">The type to add.</param>
    /// <returns>The builder.</returns>
    public TBuilder Add(Type type)
    {
        Configure(types =>
        {
            EnsureType(type, "register");
            if (types.Contains(type) == false)
            {
                types.Add(type);
            }
        });
        return This;
    }

    /// <summary>
    ///     Adds a types producer to the collection.
    /// </summary>
    /// <param name="producer">The types producer.</param>
    /// <returns>The builder.</returns>
    public TBuilder Add(Func<IEnumerable<Type>> producer)
    {
        Configure(types =>
        {
            _producers.Add(producer);
        });
        return This;
    }

    /// <summary>
    ///     Excludes a type from the collection.
    /// </summary>
    /// <typeparam name="T">The type to exclude.</typeparam>
    /// <returns>The builder.</returns>
    public TBuilder Exclude<T>()
        where T : TItem
    {
        Configure(types =>
        {
            Type type = typeof(T);
            if (_excluded.Contains(type) == false)
            {
                _excluded.Add(type);
            }
        });
        return This;
    }

    /// <summary>
    ///     Excludes a type from the collection.
    /// </summary>
    /// <param name="type">The type to exclude.</param>
    /// <returns>The builder.</returns>
    public TBuilder Exclude(Type type)
    {
        Configure(types =>
        {
            EnsureType(type, "exclude");
            if (_excluded.Contains(type) == false)
            {
                _excluded.Add(type);
            }
        });
        return This;
    }

    protected override IEnumerable<Type> GetRegisteringTypes(IEnumerable<Type> types) =>
        types
            .Union(_producers.SelectMany(x => x()))
            .Distinct()
            .Select(x => EnsureType(x, "register"))
            .Except(_excluded);
}
