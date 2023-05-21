using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods to the <see cref="IFactory" /> class.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    ///     Creates an instance with arguments.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    /// <param name="serviceProvider">The factory.</param>
    /// <param name="args">Arguments.</param>
    /// <returns>An instance of the specified type.</returns>
    /// <remarks>
    ///     <para>Throws an exception if the factory failed to get an instance of the specified type.</para>
    ///     <para>The arguments are used as dependencies by the factory.</para>
    /// </remarks>
    public static T CreateInstance<T>(this IServiceProvider serviceProvider, params object[] args)
        where T : class
        => (T)serviceProvider.CreateInstance(typeof(T), args);

    /// <summary>
    ///     Creates an instance of a service, with arguments.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider" /></param>
    /// <param name="type">The type of the instance.</param>
    /// <param name="args">Named arguments.</param>
    /// <returns>An instance of the specified type.</returns>
    /// <remarks>
    ///     <para>The instance type does not need to be registered into the factory.</para>
    ///     <para>
    ///         The arguments are used as dependencies by the factory. Other dependencies
    ///         are retrieved from the factory.
    ///     </para>
    /// </remarks>
    public static object CreateInstance(this IServiceProvider serviceProvider, Type type, params object[] args)
        => ActivatorUtilities.CreateInstance(serviceProvider, type, args);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static PublishedModelFactory CreateDefaultPublishedModelFactory(this IServiceProvider factory)
    {
        TypeLoader typeLoader = factory.GetRequiredService<TypeLoader>();
        IPublishedValueFallback publishedValueFallback = factory.GetRequiredService<IPublishedValueFallback>();
        IEnumerable<Type> types = typeLoader
            .GetTypes<PublishedElementModel>() // element models
            .Concat(typeLoader.GetTypes<PublishedContentModel>()); // content models
        return new PublishedModelFactory(types, publishedValueFallback);
    }
}
