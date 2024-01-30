using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Changes;
using Umbraco.Core.Sync;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Default <see cref="IDistributedCacheBinder"/> implementation.
    /// </summary>
    public partial class DistributedCacheBinder : IDistributedCacheBinder
    {
        private static readonly ConcurrentDictionary<string, MethodInfo> FoundHandlers = new ConcurrentDictionary<string, MethodInfo>();
        private readonly DistributedCache _distributedCache;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ILogger _logger;
        private readonly BatchedDatabaseServerMessenger _serverMessenger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedCacheBinder"/> class.
        /// </summary>
        [Obsolete("Please use the constructor accepting an instance of IServerMessenger.  This constructor will be removed in a future version.")]
        public DistributedCacheBinder(DistributedCache distributedCache, IUmbracoContextFactory umbracoContextFactory, ILogger logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;
            _umbracoContextFactory = umbracoContextFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedCacheBinder"/> class.
        /// </summary>
        public DistributedCacheBinder(DistributedCache distributedCache, IUmbracoContextFactory umbracoContextFactory, ILogger logger, IServerMessenger serverMessenger)
            : this(distributedCache, umbracoContextFactory, logger)
        {
            _serverMessenger = serverMessenger as BatchedDatabaseServerMessenger;
        }

        // internal for tests
        internal static MethodInfo FindHandler(IEventDefinition eventDefinition)
        {
            var name = eventDefinition.Sender.GetType().Name + "_" + eventDefinition.EventName;

            return FoundHandlers.GetOrAdd(name, n => CandidateHandlers.Value.FirstOrDefault(x => x.Name == n));
        }

        private static readonly Lazy<MethodInfo[]> CandidateHandlers = new Lazy<MethodInfo[]>(() =>
        {
            return typeof(DistributedCacheBinder)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(x =>
                {
                    if (x.Name.Contains("_") == false) return null;

                    var parts = x.Name.Split(Constants.CharArrays.Underscore, StringSplitOptions.RemoveEmptyEntries).Length;
                    if (parts != 2) return null;

                    var parameters = x.GetParameters();
                    if (parameters.Length != 2) return null;
                    if (typeof(EventArgs).IsAssignableFrom(parameters[1].ParameterType) == false) return null;
                    return x;
                })
                .WhereNotNull()
                .ToArray();
        });

        /// <inheritdoc />
        public void HandleEvents(IEnumerable<IEventDefinition> events)
        {
            // Ensure we run with an UmbracoContext, because this may run in a background task,
            // yet developers may be using the 'current' UmbracoContext in the event handlers.
            using (var umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext())
            {
                // When it comes to content types, a change to any single one will trigger a reload of the content and media caches.
                // We can reduce the impact of that by grouping the events to invoke just one per type, providing a collection of the individual arguments.
                var groupedEvents = GetGroupedEventList(events);
                foreach (var e in groupedEvents)
                {
                    var handler = FindHandler(e);
                    if (handler == null)
                    {
                        // TODO: should this be fatal (ie, an exception)?
                        var name = e.Sender.GetType().Name + "_" + e.EventName;
                        _logger.Warn<DistributedCacheBinder, string>("Dropping event {EventName} because no corresponding handler was found.", name);
                        continue;
                    }

                    handler.Invoke(this, new[] { e.Sender, e.Args });
                }

                // Handled events may be triggering messages to be sent for load balanced servers to refresh their caches.
                // When the state changes that initiate the events are handled outside of an Umbraco request and rather in a
                // background task, we'll have ensured an Umbraco context, but using a newly created HttpContext.
                //
                // An example of this is when using an Umbraco Deploy content transfer operation
                // (see: https://github.com/umbraco/Umbraco.Deploy.Issues/issues/90).
                //
                // This will be used in the event handlers, and when the methods on BatchedDatabaseServerMessenger are called,
                // they'll be using this "ensured" HttpContext, populating a batch of message stored in HttpContext.Items.
                // When the FlushBatch method is called on the end of an Umbraco request (via the event handler wired up in
                // DatabaseServerRegistrarAndMessengerComponent), this will use the HttpContext associated with the request,
                // which will be a different one, and so won't have the batch stored in it's HttpContext.Items.
                //
                // As such by making an explicit call here, and providing the ensured HttpContext that will have had it's
                // Items dictionary populated with the batch of messages, we'll make sure the batch is flushed, and the
                // database instructions written.
                _serverMessenger?.FlushBatch(umbracoContextReference.UmbracoContext.HttpContext);
            }
        }

        // Internal for tests
        internal static IEnumerable<IEventDefinition> GetGroupedEventList(IEnumerable<IEventDefinition> events)
        {
            var groupedEvents = new List<IEventDefinition>();

            foreach (var group in events.GroupBy(x => x.GetType()))
            {
                if (group.Key == typeof(EventDefinition<IContentTypeService, SaveEventArgs<IContentType>>))
                {
                    groupedEvents.AddRange(GroupSaveEvents<IContentTypeService, IContentType>(group));
                }
                else if (group.Key == typeof(EventDefinition<IContentTypeService, ContentTypeChange<IContentType>.EventArgs>))
                {
                    groupedEvents.AddRange(GroupChangeEvents<IContentTypeService, IContentType>(group));
                }
                else if (group.Key == typeof(EventDefinition<IMediaTypeService, SaveEventArgs<IMediaType>>))
                {
                    groupedEvents.AddRange(GroupSaveEvents<IMediaTypeService, IMediaType>(group));
                }
                else if (group.Key == typeof(EventDefinition<IMediaTypeService, ContentTypeChange<IMediaType>.EventArgs>))
                {
                    groupedEvents.AddRange(GroupChangeEvents<IMediaTypeService, IMediaType>(group));
                }
                else if (group.Key == typeof(EventDefinition<IMemberTypeService, SaveEventArgs<IMemberType>>))
                {
                    groupedEvents.AddRange(GroupSaveEvents<IMemberTypeService, IMemberType>(group));
                }
                else if (group.Key == typeof(EventDefinition<IMemberTypeService, ContentTypeChange<IMemberType>.EventArgs>))
                {
                    groupedEvents.AddRange(GroupChangeEvents<IMemberTypeService, IMemberType>(group));
                }
                else if (group.Key == typeof(EventDefinition<IDataTypeService, SaveEventArgs<IDataType>>))
                {
                    groupedEvents.AddRange(GroupSaveEvents<IDataTypeService, IDataType>(group));
                }
                else
                {
                    groupedEvents.AddRange(group);
                }
            }

            return groupedEvents;
        }

        private static IEnumerable<IEventDefinition> GroupSaveEvents<TSender, TEntity>(IEnumerable<IEventDefinition> events)
        {
            foreach (var group in events.GroupBy(x => (x.EventName, x.Sender)))
            {
                yield return new EventDefinition<TSender, SaveEventArgs<TEntity>>(
                    null,
                    (TSender)group.Key.Sender,
                    new SaveEventArgs<TEntity>(group.SelectMany(x => ((SaveEventArgs<TEntity>)x.Args).SavedEntities)),
                    group.Key.EventName);
            }
        }

        private static IEnumerable<IEventDefinition> GroupChangeEvents<TSender, TEntity>(IEnumerable<IEventDefinition> events)
            where TEntity : class, IContentTypeComposition
        {
            foreach (var group in events.GroupBy(x => (x.EventName, x.Sender)))
            {
                yield return new EventDefinition<TSender, ContentTypeChange<TEntity>.EventArgs>(
                    null,
                    (TSender)group.Key.Sender,
                    new ContentTypeChange<TEntity>.EventArgs(group.SelectMany(x => ((ContentTypeChange<TEntity>.EventArgs)x.Args).Changes)),
                    group.Key.EventName);
            }
        }
    }
}
