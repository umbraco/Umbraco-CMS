using System;
using Microsoft.Extensions.DependencyInjection;
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
        /// <param name="composition">The composition.</param>
        public static CacheRefresherCollectionBuilder CacheRefreshers(this Composition composition)
            => composition.WithCollectionBuilder<CacheRefresherCollectionBuilder>();

        /// <summary>
        /// Gets the mappers collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        public static MapperCollectionBuilder Mappers(this Composition composition)
            => composition.WithCollectionBuilder<MapperCollectionBuilder>();

        /// <summary>
        /// Gets the package actions collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        internal static PackageActionCollectionBuilder PackageActions(this Composition composition)
            => composition.WithCollectionBuilder<PackageActionCollectionBuilder>();

        /// <summary>
        /// Gets the data editor collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        public static DataEditorCollectionBuilder DataEditors(this Composition composition)
            => composition.WithCollectionBuilder<DataEditorCollectionBuilder>();

        /// <summary>
        /// Gets the data value reference factory collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        public static DataValueReferenceFactoryCollectionBuilder DataValueReferenceFactories(this Composition composition)
            => composition.WithCollectionBuilder<DataValueReferenceFactoryCollectionBuilder>();

        /// <summary>
        /// Gets the property value converters collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        public static PropertyValueConverterCollectionBuilder PropertyValueConverters(this Composition composition)
            => composition.WithCollectionBuilder<PropertyValueConverterCollectionBuilder>();

        /// <summary>
        /// Gets the url segment providers collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        public static UrlSegmentProviderCollectionBuilder UrlSegmentProviders(this Composition composition)
            => composition.WithCollectionBuilder<UrlSegmentProviderCollectionBuilder>();

        /// <summary>
        /// Gets the validators collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        internal static ManifestValueValidatorCollectionBuilder ManifestValueValidators(this Composition composition)
            => composition.WithCollectionBuilder<ManifestValueValidatorCollectionBuilder>();

        /// <summary>
        /// Gets the manifest filter collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        public static ManifestFilterCollectionBuilder ManifestFilters(this Composition composition)
            => composition.WithCollectionBuilder<ManifestFilterCollectionBuilder>();

        /// <summary>
        /// Gets the backoffice OEmbed Providers collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        public static EmbedProvidersCollectionBuilder OEmbedProviders(this Composition composition)
            => composition.WithCollectionBuilder<EmbedProvidersCollectionBuilder>();

        /// <summary>
        /// Gets the back office searchable tree collection builder
        /// </summary>
        /// <param name="composition"></param>
        /// <returns></returns>
        public static SearchableTreeCollectionBuilder SearchableTrees(this Composition composition)
            => composition.WithCollectionBuilder<SearchableTreeCollectionBuilder>();

        #endregion

        #region Uniques

        /// <summary>
        /// Sets the culture dictionary factory.
        /// </summary>
        /// <typeparam name="T">The type of the factory.</typeparam>
        /// <param name="composition">The composition.</param>
        public static void SetCultureDictionaryFactory<T>(this Composition composition)
            where T : class, ICultureDictionaryFactory
        {
            composition.Services.AddUnique<ICultureDictionaryFactory, T>();
        }

        /// <summary>
        /// Sets the culture dictionary factory.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A function creating a culture dictionary factory.</param>
        public static void SetCultureDictionaryFactory(this Composition composition, Func<IServiceProvider, ICultureDictionaryFactory> factory)
        {
            composition.Services.AddUnique(factory);
        }

        /// <summary>
        /// Sets the culture dictionary factory.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A factory.</param>
        public static void SetCultureDictionaryFactory(this Composition composition, ICultureDictionaryFactory factory)
        {
            composition.Services.AddUnique(_ => factory);
        }

        /// <summary>
        /// Sets the published content model factory.
        /// </summary>
        /// <typeparam name="T">The type of the factory.</typeparam>
        /// <param name="composition">The composition.</param>
        public static void SetPublishedContentModelFactory<T>(this Composition composition)
            where T : class, IPublishedModelFactory
        {
            composition.Services.AddUnique<IPublishedModelFactory, T>();
        }

        /// <summary>
        /// Sets the published content model factory.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A function creating a published content model factory.</param>
        public static void SetPublishedContentModelFactory(this Composition composition, Func<IServiceProvider, IPublishedModelFactory> factory)
        {
            composition.Services.AddUnique(factory);
        }

        /// <summary>
        /// Sets the published content model factory.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A published content model factory.</param>
        public static void SetPublishedContentModelFactory(this Composition composition, IPublishedModelFactory factory)
        {
            composition.Services.AddUnique(_ => factory);
        }

        /// <summary>
        /// Sets the server registrar.
        /// </summary>
        /// <typeparam name="T">The type of the server registrar.</typeparam>
        /// <param name="composition">The composition.</param>
        public static void SetServerRegistrar<T>(this Composition composition)
            where T : class, IServerRegistrar
        {
            composition.Services.AddUnique<IServerRegistrar, T>();
        }

        /// <summary>
        /// Sets the server registrar.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A function creating a server registrar.</param>
        public static void SetServerRegistrar(this Composition composition, Func<IServiceProvider, IServerRegistrar> factory)
        {
            composition.Services.AddUnique(factory);
        }

        /// <summary>
        /// Sets the server registrar.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="registrar">A server registrar.</param>
        public static void SetServerRegistrar(this Composition composition, IServerRegistrar registrar)
        {
            composition.Services.AddUnique(_ => registrar);
        }

        /// <summary>
        /// Sets the server messenger.
        /// </summary>
        /// <typeparam name="T">The type of the server registrar.</typeparam>
        /// <param name="composition">The composition.</param>
        public static void SetServerMessenger<T>(this Composition composition)
            where T : class, IServerMessenger
        {
            composition.Services.AddUnique<IServerMessenger, T>();
        }

        /// <summary>
        /// Sets the server messenger.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A function creating a server messenger.</param>
        public static void SetServerMessenger(this Composition composition, Func<IServiceProvider, IServerMessenger> factory)
        {
            composition.Services.AddUnique(factory);
        }

        /// <summary>
        /// Sets the server messenger.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="registrar">A server messenger.</param>
        public static void SetServerMessenger(this Composition composition, IServerMessenger registrar)
        {
            composition.Services.AddUnique(_ => registrar);
        }

        /// <summary>
        /// Sets the database server messenger options.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A function creating the options.</param>
        /// <remarks>Use DatabaseServerRegistrarAndMessengerComposer.GetDefaultOptions to get the options that Umbraco would use by default.</remarks>
        public static void SetDatabaseServerMessengerOptions(this Composition composition, Func<IServiceProvider, DatabaseServerMessengerOptions> factory)
        {
            composition.Services.AddUnique(factory);
        }

        /// <summary>
        /// Sets the database server messenger options.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="options">Options.</param>
        /// <remarks>Use DatabaseServerRegistrarAndMessengerComposer.GetDefaultOptions to get the options that Umbraco would use by default.</remarks>
        public static void SetDatabaseServerMessengerOptions(this Composition composition, DatabaseServerMessengerOptions options)
        {
            composition.Services.AddUnique(_ => options);
        }

        /// <summary>
        /// Sets the short string helper.
        /// </summary>
        /// <typeparam name="T">The type of the short string helper.</typeparam>
        /// <param name="composition">The composition.</param>
        public static void SetShortStringHelper<T>(this Composition composition)
            where T : class, IShortStringHelper
        {
            composition.Services.AddUnique<IShortStringHelper, T>();
        }

        /// <summary>
        /// Sets the short string helper.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A function creating a short string helper.</param>
        public static void SetShortStringHelper(this Composition composition, Func<IServiceProvider, IShortStringHelper> factory)
        {
            composition.Services.AddUnique(factory);
        }

        /// <summary>
        /// Sets the short string helper.
        /// </summary>
        /// <param name="composition">A composition.</param>
        /// <param name="helper">A short string helper.</param>
        public static void SetShortStringHelper(this Composition composition, IShortStringHelper helper)
        {
            composition.Services.AddUnique(_ => helper);
        }

        /// <summary>
        /// Sets the underlying media filesystem.
        /// </summary>
        /// <param name="composition">A composition.</param>
        /// <param name="filesystemFactory">A filesystem factory.</param>
        /// <remarks>
        /// Using this helper will ensure that your IFileSystem implementation is wrapped by the ShadowWrapper
        /// </remarks>
        public static void SetMediaFileSystem(this Composition composition, Func<IServiceProvider, IFileSystem> filesystemFactory)
            => composition.Services.AddUnique<IMediaFileSystem>(factory =>
            {
                var fileSystems = factory.GetRequiredService<IO.FileSystems>();
                return fileSystems.GetFileSystem<MediaFileSystem>(filesystemFactory(factory));
            });

        /// <summary>
        /// Sets the log viewer.
        /// </summary>
        /// <typeparam name="T">The type of the log viewer.</typeparam>
        /// <param name="composition">The composition.</param>
        public static void SetLogViewer<T>(this Composition composition)
            where T : class, ILogViewer
        {
            composition.Services.AddUnique<ILogViewer, T>();
        }

        /// <summary>
        /// Sets the log viewer.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A function creating a log viewer.</param>
        public static void SetLogViewer(this Composition composition, Func<IServiceProvider, ILogViewer> factory)
        {
            composition.Services.AddUnique(factory);
        }

        /// <summary>
        /// Sets the log viewer.
        /// </summary>
        /// <param name="composition">A composition.</param>
        /// <param name="helper">A log viewer.</param>
        public static void SetLogViewer(this Composition composition, ILogViewer viewer)
        {
            composition.Services.AddUnique(_ => viewer);
        }

        #endregion
    }
}
