namespace Umbraco.Cms.Core.Mapping;

public interface IUmbracoMapper
{
    /// <summary>
    ///     Defines a mapping.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    void Define<TSource, TTarget>();

    /// <summary>
    ///     Defines a mapping.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="map">A mapping method.</param>
    void Define<TSource, TTarget>(Action<TSource, TTarget, MapperContext> map);

    /// <summary>
    ///     Defines a mapping.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="ctor">A constructor method.</param>
    void Define<TSource, TTarget>(Func<TSource, MapperContext, TTarget> ctor);

    /// <summary>
    ///     Defines a mapping.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="ctor">A constructor method.</param>
    /// <param name="map">A mapping method.</param>
    void Define<TSource, TTarget>(
        Func<TSource, MapperContext, TTarget> ctor,
        Action<TSource, TTarget, MapperContext> map);

    /// <summary>
    ///     Maps a source object to a new target object.
    /// </summary>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <returns>The target object.</returns>
    TTarget? Map<TTarget>(object? source);

    /// <summary>
    ///     Maps a source object to a new target object.
    /// </summary>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="f">A mapper context preparation method.</param>
    /// <returns>The target object.</returns>
    TTarget? Map<TTarget>(object? source, Action<MapperContext> f);

    /// <summary>
    ///     Maps a source object to a new target object.
    /// </summary>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="context">A mapper context.</param>
    /// <returns>The target object.</returns>
    TTarget? Map<TTarget>(object? source, MapperContext context);

    /// <summary>
    ///     Maps a source object to a new target object.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <returns>The target object.</returns>
    TTarget? Map<TSource, TTarget>(TSource? source);

    /// <summary>
    ///     Maps a source object to a new target object.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="f">A mapper context preparation method.</param>
    /// <returns>The target object.</returns>
    TTarget? Map<TSource, TTarget>(TSource source, Action<MapperContext> f);

    /// <summary>
    ///     Maps a source object to a new target object.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="context">A mapper context.</param>
    /// <returns>The target object.</returns>
    TTarget? Map<TSource, TTarget>(TSource? source, MapperContext context);

    /// <summary>
    ///     Maps a source object to an existing target object.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    /// <returns>The target object.</returns>
    TTarget Map<TSource, TTarget>(TSource source, TTarget target);

    /// <summary>
    ///     Maps a source object to an existing target object.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    /// <param name="f">A mapper context preparation method.</param>
    /// <returns>The target object.</returns>
    TTarget Map<TSource, TTarget>(TSource source, TTarget target, Action<MapperContext> f);

    /// <summary>
    ///     Maps a source object to an existing target object.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    /// <param name="context">A mapper context.</param>
    /// <returns>The target object.</returns>
    TTarget Map<TSource, TTarget>(TSource source, TTarget target, MapperContext context);

    /// <summary>
    ///     Maps an enumerable of source objects to a new list of target objects.
    /// </summary>
    /// <typeparam name="TSourceElement">The type of the source objects.</typeparam>
    /// <typeparam name="TTargetElement">The type of the target objects.</typeparam>
    /// <param name="source">The source objects.</param>
    /// <returns>A list containing the target objects.</returns>
    List<TTargetElement> MapEnumerable<TSourceElement, TTargetElement>(IEnumerable<TSourceElement> source);

    /// <summary>
    ///     Maps an enumerable of source objects to a new list of target objects.
    /// </summary>
    /// <typeparam name="TSourceElement">The type of the source objects.</typeparam>
    /// <typeparam name="TTargetElement">The type of the target objects.</typeparam>
    /// <param name="source">The source objects.</param>
    /// <param name="f">A mapper context preparation method.</param>
    /// <returns>A list containing the target objects.</returns>
    List<TTargetElement> MapEnumerable<TSourceElement, TTargetElement>(
        IEnumerable<TSourceElement> source,
        Action<MapperContext> f);

    /// <summary>
    ///     Maps an enumerable of source objects to a new list of target objects.
    /// </summary>
    /// <typeparam name="TSourceElement">The type of the source objects.</typeparam>
    /// <typeparam name="TTargetElement">The type of the target objects.</typeparam>
    /// <param name="source">The source objects.</param>
    /// <param name="context">A mapper context.</param>
    /// <returns>A list containing the target objects.</returns>
    List<TTargetElement> MapEnumerable<TSourceElement, TTargetElement>(
        IEnumerable<TSourceElement> source,
        MapperContext context);
}
