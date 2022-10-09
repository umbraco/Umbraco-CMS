namespace Umbraco.Cms.Core.Mapping;

/// <summary>
///     Represents a mapper context.
/// </summary>
public class MapperContext
{
    private readonly IUmbracoMapper _mapper;
    private IDictionary<string, object?>? _items;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MapperContext" /> class.
    /// </summary>
    public MapperContext(IUmbracoMapper mapper) => _mapper = mapper;

    /// <summary>
    ///     Gets a value indicating whether the context has items.
    /// </summary>
    public bool HasItems => _items != null;

    /// <summary>
    ///     Gets the context items.
    /// </summary>
    public IDictionary<string, object?> Items => _items ??= new Dictionary<string, object?>();

    #region Map

    /// <summary>
    ///     Maps a source object to a new target object.
    /// </summary>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <returns>The target object.</returns>
    public TTarget? Map<TTarget>(object? source)
        => _mapper.Map<TTarget>(source, this);

    // let's say this is a bad (dangerous) idea, and leave it out for now
    /*
    /// <summary>
    /// Maps a source object to a new target object.
    /// </summary>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="f">A mapper context preparation method.</param>
    /// <returns>The target object.</returns>
    public TTarget Map<TTarget>(object source, Action<MapperContext> f)
    {
        f(this);
        return _mapper.Map<TTarget>(source, this);
    }
    */

    /// <summary>
    ///     Maps a source object to a new target object.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <returns>The target object.</returns>
    public TTarget? Map<TSource, TTarget>(TSource? source)
        => _mapper.Map<TSource, TTarget>(source, this);

    // let's say this is a bad (dangerous) idea, and leave it out for now
    /*
    /// <summary>
    /// Maps a source object to a new target object.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="f">A mapper context preparation method.</param>
    /// <returns>The target object.</returns>
    public TTarget Map<TSource, TTarget>(TSource source, Action<MapperContext> f)
    {
        f(this);
        return _mapper.Map<TSource, TTarget>(source, this);
    }
    */

    /// <summary>
    ///     Maps a source object to an existing target object.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    /// <returns>The target object.</returns>
    public TTarget Map<TSource, TTarget>(TSource source, TTarget target)
        => _mapper.Map(source, target, this);

    // let's say this is a bad (dangerous) idea, and leave it out for now
    /*
    /// <summary>
    /// Maps a source object to an existing target object.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    /// <param name="f">A mapper context preparation method.</param>
    /// <returns>The target object.</returns>
    public TTarget Map<TSource, TTarget>(TSource source, TTarget target, Action<MapperContext> f)
    {
        f(this);
        return _mapper.Map(source, target, this);
    }
    */

    /// <summary>
    ///     Maps an enumerable of source objects to a new list of target objects.
    /// </summary>
    /// <typeparam name="TSourceElement">The type of the source objects.</typeparam>
    /// <typeparam name="TTargetElement">The type of the target objects.</typeparam>
    /// <param name="source">The source objects.</param>
    /// <returns>A list containing the target objects.</returns>
    public List<TTargetElement> MapEnumerable<TSourceElement, TTargetElement>(IEnumerable<TSourceElement> source) =>
        source.Select(Map<TSourceElement, TTargetElement>).Where(x => x is not null).ToList()!;

    #endregion
}
