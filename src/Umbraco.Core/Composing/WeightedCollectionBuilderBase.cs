namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Implements a weighted collection builder.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
/// <typeparam name="TCollection">The type of the collection.</typeparam>
/// <typeparam name="TItem">The type of the items.</typeparam>
public abstract class WeightedCollectionBuilderBase<TBuilder, TCollection, TItem> : CollectionBuilderBase<TBuilder, TCollection, TItem>
    where TBuilder : WeightedCollectionBuilderBase<TBuilder, TCollection, TItem>
    where TCollection : class, IBuilderCollection<TItem>
{
    private readonly Dictionary<Type, int> _customWeights = new();

    public virtual int DefaultWeight { get; set; } = 100;

    protected abstract TBuilder This { get; }

    /// <summary>
    ///     Clears all types in the collection.
    /// </summary>
    /// <returns>The builder.</returns>
    public TBuilder Clear()
    {
        Configure(types => types.Clear());
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
    ///     Adds types to the collection.
    /// </summary>
    /// <param name="types">The types to add.</param>
    /// <returns>The builder.</returns>
    public TBuilder Add(IEnumerable<Type> types)
    {
        Configure(list =>
        {
            foreach (Type type in types)
            {
                // would be detected by CollectionBuilderBase when registering, anyways, but let's fail fast
                EnsureType(type, "register");
                if (list.Contains(type) == false)
                {
                    list.Add(type);
                }
            }
        });
        return This;
    }

    /// <summary>
    ///     Removes a type from the collection.
    /// </summary>
    /// <typeparam name="T">The type to remove.</typeparam>
    /// <returns>The builder.</returns>
    public TBuilder Remove<T>()
        where T : TItem
    {
        Configure(types =>
        {
            Type type = typeof(T);
            if (types.Contains(type))
            {
                types.Remove(type);
            }
        });
        return This;
    }

    /// <summary>
    ///     Removes a type from the collection.
    /// </summary>
    /// <param name="type">The type to remove.</param>
    /// <returns>The builder.</returns>
    public TBuilder Remove(Type type)
    {
        Configure(types =>
        {
            EnsureType(type, "remove");
            if (types.Contains(type))
            {
                types.Remove(type);
            }
        });
        return This;
    }

    /// <summary>
    ///     Changes the default weight of an item
    /// </summary>
    /// <typeparam name="T">The type of item</typeparam>
    /// <param name="weight">The new weight</param>
    /// <returns></returns>
    public TBuilder SetWeight<T>(int weight)
        where T : TItem
    {
        _customWeights[typeof(T)] = weight;
        return This;
    }

    protected override IEnumerable<Type> GetRegisteringTypes(IEnumerable<Type> types)
    {
        var list = types.ToList();
        list.Sort((t1, t2) => GetWeight(t1).CompareTo(GetWeight(t2)));
        return list;
    }

    protected virtual int GetWeight(Type type)
    {
        if (_customWeights.ContainsKey(type))
        {
            return _customWeights[type];
        }

        WeightAttribute? attr = type.GetCustomAttributes(typeof(WeightAttribute), false).OfType<WeightAttribute>()
            .SingleOrDefault();
        return attr?.Weight ?? DefaultWeight;
    }
}
