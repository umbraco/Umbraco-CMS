using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.DependencyInjection;

/// <summary>
///     Provides extension methods to the <see cref="IUmbracoBuilder" /> class.
/// </summary>
public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Sets the culture dictionary factory.
    /// </summary>
    /// <typeparam name="T">The type of the factory.</typeparam>
    /// <param name="builder">The builder.</param>
    public static IUmbracoBuilder SetCultureDictionaryFactory<T>(this IUmbracoBuilder builder)
        where T : class, ICultureDictionaryFactory
    {
        builder.Services.AddUnique<ICultureDictionaryFactory, T>();
        return builder;
    }

    /// <summary>
    ///     Sets the default view content provider
    /// </summary>
    /// <typeparam name="T">The type of the provider.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns></returns>
    public static IUmbracoBuilder SetDefaultViewContentProvider<T>(this IUmbracoBuilder builder)
        where T : class, IDefaultViewContentProvider
    {
        builder.Services.AddUnique<IDefaultViewContentProvider, T>();
        return builder;
    }

    /// <summary>
    ///     Sets the culture dictionary factory.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="factory">A function creating a culture dictionary factory.</param>
    public static IUmbracoBuilder SetCultureDictionaryFactory(
        this IUmbracoBuilder builder,
        Func<IServiceProvider, ICultureDictionaryFactory> factory)
    {
        builder.Services.AddUnique(factory);
        return builder;
    }

    /// <summary>
    ///     Sets the culture dictionary factory.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="factory">A factory.</param>
    public static IUmbracoBuilder SetCultureDictionaryFactory(
        this IUmbracoBuilder builder,
        ICultureDictionaryFactory factory)
    {
        builder.Services.AddUnique(factory);
        return builder;
    }

    /// <summary>
    ///     Sets the published content model factory.
    /// </summary>
    /// <typeparam name="T">The type of the factory.</typeparam>
    /// <param name="builder">The builder.</param>
    public static IUmbracoBuilder SetPublishedContentModelFactory<T>(this IUmbracoBuilder builder)
        where T : class, IPublishedModelFactory
    {
        builder.Services.AddUnique<IPublishedModelFactory, T>();
        return builder;
    }

    /// <summary>
    ///     Sets the published content model factory.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="factory">A function creating a published content model factory.</param>
    public static IUmbracoBuilder SetPublishedContentModelFactory(
        this IUmbracoBuilder builder,
        Func<IServiceProvider, IPublishedModelFactory> factory)
    {
        builder.Services.AddUnique(factory);
        return builder;
    }

    /// <summary>
    ///     Sets the published content model factory.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="factory">A published content model factory.</param>
    public static IUmbracoBuilder SetPublishedContentModelFactory(
        this IUmbracoBuilder builder,
        IPublishedModelFactory factory)
    {
        builder.Services.AddUnique(factory);
        return builder;
    }

    /// <summary>
    ///     Sets the short string helper.
    /// </summary>
    /// <typeparam name="T">The type of the short string helper.</typeparam>
    /// <param name="builder">The builder.</param>
    public static IUmbracoBuilder SetShortStringHelper<T>(this IUmbracoBuilder builder)
        where T : class, IShortStringHelper
    {
        builder.Services.AddUnique<IShortStringHelper, T>();
        return builder;
    }

    /// <summary>
    ///     Sets the short string helper.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="factory">A function creating a short string helper.</param>
    public static IUmbracoBuilder SetShortStringHelper(
        this IUmbracoBuilder builder,
        Func<IServiceProvider, IShortStringHelper> factory)
    {
        builder.Services.AddUnique(factory);
        return builder;
    }

    /// <summary>
    ///     Sets the short string helper.
    /// </summary>
    /// <param name="builder">A builder.</param>
    /// <param name="helper">A short string helper.</param>
    public static IUmbracoBuilder SetShortStringHelper(this IUmbracoBuilder builder, IShortStringHelper helper)
    {
        builder.Services.AddUnique(helper);
        return builder;
    }

    /// <summary>
    ///     Sets the filesystem used by the MediaFileManager
    /// </summary>
    /// <param name="builder">A builder.</param>
    /// <param name="filesystemFactory">Factory method to create an IFileSystem implementation used in the MediaFileManager</param>
    public static IUmbracoBuilder SetMediaFileSystem(
        this IUmbracoBuilder builder,
        Func<IServiceProvider, IFileSystem> filesystemFactory)
    {
        builder.Services.AddUnique(
            provider =>
            {
                IFileSystem filesystem = filesystemFactory(provider);

                // We need to use the Filesystems to create a shadow wrapper,
                // because shadow wrapper requires the IsScoped delegate from the FileSystems.
                // This is used by the scope provider when taking control of the filesystems.
                FileSystems fileSystems = provider.GetRequiredService<FileSystems>();
                IFileSystem shadow = fileSystems.CreateShadowWrapper(filesystem, "media");

                return provider.CreateInstance<MediaFileManager>(shadow);
            });
        return builder;
    }

    /// <summary>
    ///     Register FileSystems with a method to configure the <see cref="FileSystems" />.
    /// </summary>
    /// <param name="builder">A builder.</param>
    /// <param name="configure">Method that configures the <see cref="FileSystems" />.</param>
    /// <exception cref="ArgumentNullException">Throws exception if <paramref name="configure" /> is null.</exception>
    /// <exception cref="InvalidOperationException">Throws exception if full path can't be resolved successfully.</exception>
    public static IUmbracoBuilder ConfigureFileSystems(
        this IUmbracoBuilder builder,
        Action<IServiceProvider, FileSystems> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        builder.Services.AddUnique(
            provider =>
            {
                FileSystems fileSystems = provider.CreateInstance<FileSystems>();
                configure(provider, fileSystems);
                return fileSystems;
            });
        return builder;
    }

    /// <summary>
    ///     Sets the log viewer.
    /// </summary>
    /// <typeparam name="T">The type of the log viewer.</typeparam>
    /// <param name="builder">The builder.</param>
    public static IUmbracoBuilder SetLogViewer<T>(this IUmbracoBuilder builder)
        where T : class, ILogViewer
    {
        builder.Services.AddUnique<ILogViewer, T>();
        return builder;
    }

    /// <summary>
    ///     Sets the log viewer.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="factory">A function creating a log viewer.</param>
    public static IUmbracoBuilder SetLogViewer(this IUmbracoBuilder builder, Func<IServiceProvider, ILogViewer> factory)
    {
        builder.Services.AddUnique(factory);
        return builder;
    }

    /// <summary>
    ///     Sets the log viewer.
    /// </summary>
    /// <param name="builder">A builder.</param>
    /// <param name="viewer">A log viewer.</param>
    public static IUmbracoBuilder SetLogViewer(this IUmbracoBuilder builder, ILogViewer viewer)
    {
        builder.Services.AddUnique(viewer);
        return builder;
    }
}
