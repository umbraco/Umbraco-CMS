using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Events;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Changes;
using Umbraco.Core.Sync;
using Umbraco.Infrastructure.Cache;
using Umbraco.Web;
using Umbraco.Web.Cache;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Search;

namespace Umbraco.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Provides extension methods to the <see cref="IUmbracoBuilder"/> class.
    /// </summary>
    public static partial class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Adds distributed cache support
        /// </summary>
        public static IUmbracoBuilder AddDistributedCache(this IUmbracoBuilder builder)
        {
            // NOTE: the `DistributedCache` is registered in UmbracoBuilder since it's a core service

            builder.SetDatabaseServerMessengerCallbacks(GetCallbacks);
            builder.SetServerMessenger<BatchedDatabaseServerMessenger>();
            builder.AddNotificationHandler<UmbracoApplicationStarting, DatabaseServerMessengerNotificationHandler>();

            // TODO: We don't need server registrar anymore
            // register a server registrar, by default it's the db registrar
            builder.Services.AddUnique<IServerRegistrar>(f =>
            {
                var globalSettings = f.GetRequiredService<IOptions<GlobalSettings>>().Value;

                // TODO:  we still register the full IServerMessenger because
                // even on 1 single server we can have 2 concurrent app domains
                var singleServer = globalSettings.DisableElectionForSingleServer;
                return singleServer
                    ? (IServerRegistrar)new SingleServerRegistrar(f.GetRequiredService<IRequestAccessor>())
                    : new DatabaseServerRegistrar(
                        new Lazy<IServerRegistrationService>(f.GetRequiredService<IServerRegistrationService>));
            });

            builder.Services.AddUnique<IDistributedCacheBinder, DistributedCacheBinder>();
            return builder;
        }

        /// <summary>
        /// Sets the server registrar.
        /// </summary>
        /// <typeparam name="T">The type of the server registrar.</typeparam>
        /// <param name="builder">The builder.</param>
        public static void SetServerRegistrar<T>(this IUmbracoBuilder builder)
            where T : class, IServerRegistrar
            => builder.Services.AddUnique<IServerRegistrar, T>();

        /// <summary>
        /// Sets the server registrar.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="factory">A function creating a server registrar.</param>
        public static void SetServerRegistrar(this IUmbracoBuilder builder, Func<IServiceProvider, IServerRegistrar> factory)
            => builder.Services.AddUnique(factory);

        /// <summary>
        /// Sets the server registrar.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="registrar">A server registrar.</param>
        public static void SetServerRegistrar(this IUmbracoBuilder builder, IServerRegistrar registrar)
            => builder.Services.AddUnique(registrar);

        /// <summary>
        /// Sets the database server messenger options.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="factory">A function creating the options.</param>
        /// <remarks>Use DatabaseServerRegistrarAndMessengerComposer.GetDefaultOptions to get the options that Umbraco would use by default.</remarks>
        public static void SetDatabaseServerMessengerCallbacks(this IUmbracoBuilder builder, Func<IServiceProvider, DatabaseServerMessengerCallbacks> factory)
            => builder.Services.AddUnique(factory);

        /// <summary>
        /// Sets the database server messenger options.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="options">Options.</param>
        /// <remarks>Use DatabaseServerRegistrarAndMessengerComposer.GetDefaultOptions to get the options that Umbraco would use by default.</remarks>
        public static void SetDatabaseServerMessengerOptions(this IUmbracoBuilder builder, DatabaseServerMessengerCallbacks options)
            => builder.Services.AddUnique(options);

        /// <summary>
        /// Sets the server messenger.
        /// </summary>
        /// <typeparam name="T">The type of the server registrar.</typeparam>
        /// <param name="builder">The builder.</param>
        public static void SetServerMessenger<T>(this IUmbracoBuilder builder)
            where T : class, IServerMessenger
            => builder.Services.AddUnique<IServerMessenger, T>();

        /// <summary>
        /// Sets the server messenger.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="factory">A function creating a server messenger.</param>
        public static void SetServerMessenger(this IUmbracoBuilder builder, Func<IServiceProvider, IServerMessenger> factory)
            => builder.Services.AddUnique(factory);

        /// <summary>
        /// Sets the server messenger.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="registrar">A server messenger.</param>
        public static void SetServerMessenger(this IUmbracoBuilder builder, IServerMessenger registrar)
            => builder.Services.AddUnique(registrar);

        private static DatabaseServerMessengerCallbacks GetCallbacks(IServiceProvider factory) => new DatabaseServerMessengerCallbacks
        {
            // These callbacks will be executed if the server has not been synced
            // (i.e. it is a new server or the lastsynced.txt file has been removed)
            InitializingCallbacks = new Action[]
                {
                    // rebuild the xml cache file if the server is not synced
                    () =>
                    {
                        IPublishedSnapshotService publishedSnapshotService = factory.GetRequiredService<IPublishedSnapshotService>();

                        // rebuild the published snapshot caches entirely, if the server is not synced
                        // this is equivalent to DistributedCache RefreshAll... but local only
                        // (we really should have a way to reuse RefreshAll... locally)
                        // note: refresh all content & media caches does refresh content types too
                        publishedSnapshotService.Notify(new[] { new DomainCacheRefresher.JsonPayload(0, DomainChangeTypes.RefreshAll) });
                        publishedSnapshotService.Notify(new[] { new ContentCacheRefresher.JsonPayload(0, null, TreeChangeTypes.RefreshAll) }, out _, out _);
                        publishedSnapshotService.Notify(new[] { new MediaCacheRefresher.JsonPayload(0, null, TreeChangeTypes.RefreshAll) }, out _);
                    },

                    // rebuild indexes if the server is not synced
                    // NOTE: This will rebuild ALL indexes including the members, if developers want to target specific
                    // indexes then they can adjust this logic themselves.
                    () =>
                    {
                        var indexRebuilder = factory.GetRequiredService<BackgroundIndexRebuilder>();
                        indexRebuilder.RebuildIndexes(false, 5000);
                    }
                }
        };
    }
}
