using System;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Builder;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Dictionary;
using Umbraco.Core.IO;
using Umbraco.Core.Logging.Viewer;
using Umbraco.Core.Manifest;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PackageActions;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Strings;
using Umbraco.Core.Sync;
using Umbraco.Web.Media.EmbedProviders;
using Umbraco.Web.Search;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides extension methods to the <see cref="Composition"/> class.
    /// </summary>
    public static partial class CompositionExtensions
    {
        #region Collection Builders

        /// <summary>
        /// Gets the cache refreshers collection builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static CacheRefresherCollectionBuilder CacheRefreshers(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<CacheRefresherCollectionBuilder>();

        /// <summary>
        /// Gets the mappers collection builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static MapperCollectionBuilder Mappers(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<MapperCollectionBuilder>();

        /// <summary>
        /// Gets the package actions collection builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        internal static PackageActionCollectionBuilder PackageActions(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<PackageActionCollectionBuilder>();

        /// <summary>
        /// Gets the data editor collection builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static DataEditorCollectionBuilder DataEditors(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<DataEditorCollectionBuilder>();

        /// <summary>
        /// Gets the data value reference factory collection builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static DataValueReferenceFactoryCollectionBuilder DataValueReferenceFactories(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<DataValueReferenceFactoryCollectionBuilder>();

        /// <summary>
        /// Gets the property value converters collection builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static PropertyValueConverterCollectionBuilder PropertyValueConverters(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<PropertyValueConverterCollectionBuilder>();

        /// <summary>
        /// Gets the url segment providers collection builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static UrlSegmentProviderCollectionBuilder UrlSegmentProviders(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<UrlSegmentProviderCollectionBuilder>();

        /// <summary>
        /// Gets the validators collection builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        internal static ManifestValueValidatorCollectionBuilder ManifestValueValidators(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<ManifestValueValidatorCollectionBuilder>();

        /// <summary>
        /// Gets the manifest filter collection builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static ManifestFilterCollectionBuilder ManifestFilters(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<ManifestFilterCollectionBuilder>();

        /// <summary>
        /// Gets the backoffice OEmbed Providers collection builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static EmbedProvidersCollectionBuilder OEmbedProviders(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<EmbedProvidersCollectionBuilder>();

        /// <summary>
        /// Gets the back office searchable tree collection builder
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static SearchableTreeCollectionBuilder SearchableTrees(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<SearchableTreeCollectionBuilder>();

        #endregion

        #region Uniques

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
        /// Sets the server registrar.
        /// </summary>
        /// <typeparam name="T">The type of the server registrar.</typeparam>
        /// <param name="builder">The builder.</param>
        public static void SetServerRegistrar<T>(this IUmbracoBuilder builder)
            where T : class, IServerRegistrar
        {
            builder.Services.AddUnique<IServerRegistrar, T>();
        }

        /// <summary>
        /// Sets the server registrar.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="factory">A function creating a server registrar.</param>
        public static void SetServerRegistrar(this IUmbracoBuilder builder, Func<IServiceProvider, IServerRegistrar> factory)
        {
            builder.Services.AddUnique(factory);
        }

        /// <summary>
        /// Sets the server registrar.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="registrar">A server registrar.</param>
        public static void SetServerRegistrar(this IUmbracoBuilder builder, IServerRegistrar registrar)
        {
            builder.Services.AddUnique(registrar);
        }

        /// <summary>
        /// Sets the server messenger.
        /// </summary>
        /// <typeparam name="T">The type of the server registrar.</typeparam>
        /// <param name="builder">The builder.</param>
        public static void SetServerMessenger<T>(this IUmbracoBuilder builder)
            where T : class, IServerMessenger
        {
            builder.Services.AddUnique<IServerMessenger, T>();
        }

        /// <summary>
        /// Sets the server messenger.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="factory">A function creating a server messenger.</param>
        public static void SetServerMessenger(this IUmbracoBuilder builder, Func<IServiceProvider, IServerMessenger> factory)
        {
            builder.Services.AddUnique(factory);
        }

        /// <summary>
        /// Sets the server messenger.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="registrar">A server messenger.</param>
        public static void SetServerMessenger(this IUmbracoBuilder builder, IServerMessenger registrar)
        {
            builder.Services.AddUnique(registrar);
        }

        /// <summary>
        /// Sets the database server messenger options.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="factory">A function creating the options.</param>
        /// <remarks>Use DatabaseServerRegistrarAndMessengerComposer.GetDefaultOptions to get the options that Umbraco would use by default.</remarks>
        public static void SetDatabaseServerMessengerCallbacks(this IUmbracoBuilder builder, Func<IServiceProvider, DatabaseServerMessengerCallbacks> factory)
        {
            builder.Services.AddUnique(factory);
        }

        /// <summary>
        /// Sets the database server messenger options.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="options">Options.</param>
        /// <remarks>Use DatabaseServerRegistrarAndMessengerComposer.GetDefaultOptions to get the options that Umbraco would use by default.</remarks>
        public static void SetDatabaseServerMessengerOptions(this IUmbracoBuilder builder, DatabaseServerMessengerCallbacks options)
        {
            builder.Services.AddUnique(options);
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
        /// Sets the underlying media filesystem.
        /// </summary>
        /// <param name="builder">A builder.</param>
        /// <param name="filesystemFactory">A filesystem factory.</param>
        /// <remarks>
        /// Using this helper will ensure that your IFileSystem implementation is wrapped by the ShadowWrapper
        /// </remarks>
        public static void SetMediaFileSystem(this IUmbracoBuilder builder, Func<IServiceProvider, IFileSystem> filesystemFactory)
            => builder.Services.AddUnique<IMediaFileSystem>(factory =>
            {
                var fileSystems = factory.GetRequiredService<IO.FileSystems>();
                return fileSystems.GetFileSystem<MediaFileSystem>(filesystemFactory(factory));
            });

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

        #endregion
    }
}
