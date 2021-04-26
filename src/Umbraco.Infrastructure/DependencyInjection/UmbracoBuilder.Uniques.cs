using System;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Provides extension methods to the <see cref="IUmbracoBuilder"/> class.
    /// </summary>
    public static partial class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Sets the culture dictionary factory.
        /// </summary>
        /// <typeparam name="T">The type of the factory.</typeparam>
        /// <param name="builder">The builder.</param>
        public static void SetCultureDictionaryFactory<T>(this IUmbracoBuilder builder)
            where T : class, ICultureDictionaryFactory
        {
            builder.Services.AddUnique<ICultureDictionaryFactory, T>();
        }

        /// <summary>
        /// Sets the culture dictionary factory.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="factory">A function creating a culture dictionary factory.</param>
        public static void SetCultureDictionaryFactory(this IUmbracoBuilder builder, Func<IServiceProvider, ICultureDictionaryFactory> factory)
        {
            builder.Services.AddUnique(factory);
        }

        /// <summary>
        /// Sets the culture dictionary factory.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="factory">A factory.</param>
        public static void SetCultureDictionaryFactory(this IUmbracoBuilder builder, ICultureDictionaryFactory factory)
        {
            builder.Services.AddUnique(factory);
        }

        /// <summary>
        /// Sets the published content model factory.
        /// </summary>
        /// <typeparam name="T">The type of the factory.</typeparam>
        /// <param name="builder">The builder.</param>
        public static void SetPublishedContentModelFactory<T>(this IUmbracoBuilder builder)
            where T : class, IPublishedModelFactory
        {
            builder.Services.AddUnique<IPublishedModelFactory, T>();
        }

        /// <summary>
        /// Sets the published content model factory.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="factory">A function creating a published content model factory.</param>
        public static void SetPublishedContentModelFactory(this IUmbracoBuilder builder, Func<IServiceProvider, IPublishedModelFactory> factory)
        {
            builder.Services.AddUnique(factory);
        }

        /// <summary>
        /// Sets the published content model factory.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="factory">A published content model factory.</param>
        public static void SetPublishedContentModelFactory(this IUmbracoBuilder builder, IPublishedModelFactory factory)
        {
            builder.Services.AddUnique(factory);
        }

        /// <summary>
        /// Sets the short string helper.
        /// </summary>
        /// <typeparam name="T">The type of the short string helper.</typeparam>
        /// <param name="builder">The builder.</param>
        public static void SetShortStringHelper<T>(this IUmbracoBuilder builder)
            where T : class, IShortStringHelper
        {
            builder.Services.AddUnique<IShortStringHelper, T>();
        }

        /// <summary>
        /// Sets the short string helper.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="factory">A function creating a short string helper.</param>
        public static void SetShortStringHelper(this IUmbracoBuilder builder, Func<IServiceProvider, IShortStringHelper> factory)
        {
            builder.Services.AddUnique(factory);
        }

        /// <summary>
        /// Sets the short string helper.
        /// </summary>
        /// <param name="builder">A builder.</param>
        /// <param name="helper">A short string helper.</param>
        public static void SetShortStringHelper(this IUmbracoBuilder builder, IShortStringHelper helper)
        {
            builder.Services.AddUnique(helper);
        }

        /// <summary>
        /// Sets the filesystem used by the MediaFileManager
        /// </summary>
        /// <param name="builder">A builder.</param>
        /// <param name="filesystemFactory">Factory method to create an IFileSystem implementation used in the MediaFileManager</param>
        public static void SetMediaFileSystem(this IUmbracoBuilder builder,
            Func<IServiceProvider, IFileSystem> filesystemFactory) => builder.Services.AddSingleton(
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

        /// <summary>
        /// Register FileSystems with a method to configure the <see cref="FileSystems"/>.
        /// </summary>
        /// <param name="builder">A builder.</param>
        /// <param name="configure">Method that configures the <see cref="FileSystems"/>.</param>
        /// <exception cref="ArgumentNullException">Throws exception if <paramref name="configure"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Throws exception if full path can't be resolved successfully.</exception>
        public static void AddFileSystems(this IUmbracoBuilder builder,
            Action<IServiceProvider, FileSystems> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            builder.AddFileSystems();

            builder.Services.AddUnique(
                provider =>
                {
                    FileSystems fileSystems = provider.CreateInstance<FileSystems>();
                    configure(provider, fileSystems);
                    return fileSystems;
                });
        }

        /// <summary>
        /// Sets the log viewer.
        /// </summary>
        /// <typeparam name="T">The type of the log viewer.</typeparam>
        /// <param name="builder">The builder.</param>
        public static void SetLogViewer<T>(this IUmbracoBuilder builder)
            where T : class, ILogViewer
        {
            builder.Services.AddUnique<ILogViewer, T>();
        }

        /// <summary>
        /// Sets the log viewer.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="factory">A function creating a log viewer.</param>
        public static void SetLogViewer(this IUmbracoBuilder builder, Func<IServiceProvider, ILogViewer> factory)
        {
            builder.Services.AddUnique(factory);
        }

        /// <summary>
        /// Sets the log viewer.
        /// </summary>
        /// <param name="builder">A builder.</param>
        /// <param name="helper">A log viewer.</param>
        public static void SetLogViewer(this IUmbracoBuilder builder, ILogViewer viewer)
        {
            builder.Services.AddUnique(viewer);
        }
    }
}
