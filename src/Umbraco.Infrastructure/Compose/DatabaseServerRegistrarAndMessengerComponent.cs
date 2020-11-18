using System;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Services.Changes;
using Umbraco.Core.Sync;
using Umbraco.Web.Cache;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Search;

namespace Umbraco.Web.Compose
{
    /// <summary>
    /// Ensures that servers are automatically registered in the database, when using the database server registrar.
    /// </summary>
    /// <remarks>
    /// <para>At the moment servers are automatically registered upon first request and then on every
    /// request but not more than once per (configurable) period. This really is "for information & debug" purposes so
    /// we can look at the table and see what servers are registered - but the info is not used anywhere.</para>
    /// <para>Should we actually want to use this, we would need a better and more deterministic way of figuring
    /// out the "server address" ie the address to which server-to-server requests should be sent - because it
    /// probably is not the "current request address" - especially in multi-domains configurations.</para>
    /// </remarks>
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]

    // during Initialize / Startup, we end up checking Examine, which needs to be initialized beforehand
    // TODO: should not be a strong dependency on "examine" but on an "indexing component"
    [ComposeAfter(typeof(ExamineComposer))]

    public sealed class DatabaseServerRegistrarAndMessengerComposer : ComponentComposer<DatabaseServerRegistrarAndMessengerComponent>, ICoreComposer
    {
        public static DatabaseServerMessengerCallbacks GetCallbacks(IServiceProvider factory)
        {
            return new DatabaseServerMessengerCallbacks
            {
                //These callbacks will be executed if the server has not been synced
                // (i.e. it is a new server or the lastsynced.txt file has been removed)
                InitializingCallbacks = new Action[]
                {
                    //rebuild the xml cache file if the server is not synced
                    () =>
                    {
                        var publishedSnapshotService = factory.GetRequiredService<IPublishedSnapshotService>();

                        // rebuild the published snapshot caches entirely, if the server is not synced
                        // this is equivalent to DistributedCache RefreshAll... but local only
                        // (we really should have a way to reuse RefreshAll... locally)
                        // note: refresh all content & media caches does refresh content types too
                        publishedSnapshotService.Notify(new[] { new DomainCacheRefresher.JsonPayload(0, DomainChangeTypes.RefreshAll) });
                        publishedSnapshotService.Notify(new[] { new ContentCacheRefresher.JsonPayload(0, null, TreeChangeTypes.RefreshAll) }, out _, out _);
                        publishedSnapshotService.Notify(new[] { new MediaCacheRefresher.JsonPayload(0, null, TreeChangeTypes.RefreshAll) }, out _);
                    },

                    //rebuild indexes if the server is not synced
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

        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            composition.SetDatabaseServerMessengerCallbacks(GetCallbacks);
            composition.SetServerMessenger<BatchedDatabaseServerMessenger>();
        }
    }

    public sealed class DatabaseServerRegistrarAndMessengerComponent : IComponent
    {
        private readonly IBatchedDatabaseServerMessenger _messenger;
        private readonly IRequestAccessor _requestAccessor;

        public DatabaseServerRegistrarAndMessengerComponent(
            IServerMessenger serverMessenger,
            IRequestAccessor requestAccessor)
        {
            _requestAccessor = requestAccessor;
            _messenger = serverMessenger as IBatchedDatabaseServerMessenger;
        }

        public void Initialize()
        {
            // The scheduled tasks - TouchServerTask and InstructionProcessTask - run as .NET Core hosted services.
            // The former (as well as other hosted services that run outside of an HTTP request context) depends on the application URL
            // being available (via IRequestAccessor), which can only be retrieved within an HTTP request (unless it's explicitly configured).
            // Hence we hook up a one-off task on an HTTP request to ensure this is retrieved, which caches the value and makes it available
            // for the hosted services to use when the HTTP request is not available.
            _requestAccessor.RouteAttempt += EnsureApplicationUrlOnce;

            // Must come last, as it references some _variables
            _messenger?.Startup();
        }

        public void Terminate()
        { }

        private void EnsureApplicationUrlOnce(object sender, RoutableAttemptEventArgs e)
        {
            if (e.Outcome == EnsureRoutableOutcome.IsRoutable || e.Outcome == EnsureRoutableOutcome.NotDocumentRequest)
            {
                _requestAccessor.RouteAttempt -= EnsureApplicationUrlOnce;
                EnsureApplicationUrl();
            }
        }

        private void EnsureApplicationUrl()
        {
            // By retrieving the application URL within the context of a request (as we are here in responding
            // to the IRequestAccessor's RouteAttempt event), we'll get it from the HTTP context and save it for
            // future requests that may not be within an HTTP request (e.g. from hosted services).
            _requestAccessor.GetApplicationUrl();
        }
    }
}
