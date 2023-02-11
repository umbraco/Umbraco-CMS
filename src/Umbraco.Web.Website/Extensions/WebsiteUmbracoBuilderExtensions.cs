using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods to the <see cref="IUmbracoBuilder" /> class.
/// </summary>
public static class WebsiteUmbracoBuilderExtensions
{
    #region Uniques

    /// <summary>
    ///     Sets the content last chance finder.
    /// </summary>
    /// <typeparam name="T">The type of the content last chance finder.</typeparam>
    /// <param name="builder">The builder.</param>
    public static IUmbracoBuilder SetContentLastChanceFinder<T>(this IUmbracoBuilder builder)
        where T : class, IContentLastChanceFinder
    {
        builder.Services.AddUnique<IContentLastChanceFinder, T>();
        return builder;
    }

    /// <summary>
    ///     Sets the content last chance finder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="factory">A function creating a last chance finder.</param>
    public static IUmbracoBuilder SetContentLastChanceFinder(
        this IUmbracoBuilder builder,
        Func<IServiceProvider, IContentLastChanceFinder> factory)
    {
        builder.Services.AddUnique(factory);
        return builder;
    }

    /// <summary>
    ///     Sets the content last chance finder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="finder">A last chance finder.</param>
    public static IUmbracoBuilder SetContentLastChanceFinder(
        this IUmbracoBuilder builder,
        IContentLastChanceFinder finder)
    {
        builder.Services.AddUnique(finder);
        return builder;
    }

    /// <summary>
    ///     Sets the site domain helper.
    /// </summary>
    /// <typeparam name="T">The type of the site domain helper.</typeparam>
    /// <param name="builder"></param>
    public static IUmbracoBuilder SetSiteDomainHelper<T>(this IUmbracoBuilder builder)
        where T : class, ISiteDomainMapper
    {
        builder.Services.AddUnique<ISiteDomainMapper, T>();
        return builder;
    }

    /// <summary>
    ///     Sets the site domain helper.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="factory">A function creating a helper.</param>
    public static IUmbracoBuilder SetSiteDomainHelper(
        this IUmbracoBuilder builder,
        Func<IServiceProvider, ISiteDomainMapper> factory)
    {
        builder.Services.AddUnique(factory);
        return builder;
    }

    /// <summary>
    ///     Sets the site domain helper.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="helper">A helper.</param>
    public static IUmbracoBuilder SetSiteDomainHelper(this IUmbracoBuilder builder, ISiteDomainMapper helper)
    {
        builder.Services.AddUnique(helper);
        return builder;
    }

    /// <summary>
    ///     Sets the UmbracoTreeSearcherFields to change fields that can be searched in the backoffice.
    /// </summary>
    /// <typeparam name="T">The type of the Umbraco tree searcher fields.</typeparam>
    /// <param name="builder">The builder.</param>
    public static IUmbracoBuilder SetTreeSearcherFields<T>(this IUmbracoBuilder builder)
        where T : class, IUmbracoTreeSearcherFields
    {
        builder.Services.AddUnique<IUmbracoTreeSearcherFields, T>();
        return builder;
    }

    /// <summary>
    ///     Sets the UmbracoTreeSearcherFields to change fields that can be searched in the backoffice.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="factory">A function creating a TreeSearcherFields</param>
    public static IUmbracoBuilder SetTreeSearcherFields(
        this IUmbracoBuilder builder,
        Func<IServiceProvider, IUmbracoTreeSearcherFields> factory)
    {
        builder.Services.AddUnique(factory);
        return builder;
    }

    /// <summary>
    ///     Sets the UmbracoTreeSearcherFields to change fields that can be searched in the backoffice.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="treeSearcherFields">An UmbracoTreeSearcherFields.</param>
    public static IUmbracoBuilder SetTreeSearcherFields(this IUmbracoBuilder builder, IUmbracoTreeSearcherFields treeSearcherFields)
    {
        builder.Services.AddUnique(treeSearcherFields);
        return builder;
    }

    #endregion
}
