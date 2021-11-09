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

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedCacheBinder"/> class.
        /// </summary>
        public DistributedCacheBinder(DistributedCache distributedCache, IUmbracoContextFactory umbracoContextFactory, ILogger logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;
            _umbracoContextFactory = umbracoContextFactory;
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
            using (_umbracoContextFactory.EnsureUmbracoContext())
            {
                // When it comes to content types types, a change to any single one will trigger a reload of the content and media caches.
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
            }
        }

        // Internal for tests
        internal static IEnumerable<IEventDefinition> GetGroupedEventList(IEnumerable<IEventDefinition> events)
        {
            var groupedEvents = new List<IEventDefinition>();

            var grouped = events.GroupBy(x => x.GetType());

            foreach (var group in grouped)
            {
                if (group.Key == typeof(EventDefinition<IContentTypeService, SaveEventArgs<IContentType>>))
                {
                    GroupSaveEvents<IContentTypeService, IContentType>(groupedEvents, group);
                }
                else if (group.Key == typeof(EventDefinition<IContentTypeService, ContentTypeChange<IContentType>.EventArgs>))
                {
                    GroupChangeEvents<IContentTypeService, IContentType>(groupedEvents, group);
                }
                else if (group.Key == typeof(EventDefinition<IMediaTypeService, SaveEventArgs<IMediaType>>))
                {
                    GroupSaveEvents<IMediaTypeService, IMediaType>(groupedEvents, group);
                }
                else if (group.Key == typeof(EventDefinition<IMediaTypeService, ContentTypeChange<IMediaType>.EventArgs>))
                {
                    GroupChangeEvents<IMediaTypeService, IMediaType>(groupedEvents, group);
                }
                else if (group.Key == typeof(EventDefinition<IMemberTypeService, SaveEventArgs<IMemberType>>))
                {
                    GroupSaveEvents<IMemberTypeService, IMemberType>(groupedEvents, group);
                }
                else if (group.Key == typeof(EventDefinition<IMemberTypeService, ContentTypeChange<IMemberType>.EventArgs>))
                {
                    GroupChangeEvents<IMemberTypeService, IMemberType>(groupedEvents, group);
                }
                else
                {
                    groupedEvents.AddRange(group);
                }
            }

            return groupedEvents;
        }

        private static void GroupSaveEvents<TService, TType>(List<IEventDefinition> groupedEvents, IGrouping<Type, IEventDefinition> group)
            where TService : IContentTypeBaseService
            where TType : IContentTypeBase
        {
            var groupedGroups = group.GroupBy(x => (x.EventName, x.Sender));

            foreach (var groupedGroup in groupedGroups)
            {
                groupedEvents.Add(new EventDefinition<TService, SaveEventArgs<TType>>(
                    null,
                    (TService)groupedGroup.Key.Sender,
                    new SaveEventArgs<TType>(groupedGroup.SelectMany(x => ((SaveEventArgs<TType>)x.Args).SavedEntities)),
                    groupedGroup.Key.EventName));
            }
        }

        private static void GroupChangeEvents<TService, TType>(List<IEventDefinition> groupedEvents, IGrouping<Type, IEventDefinition> group)
            where TService : IContentTypeBaseService
            where TType : class, IContentTypeComposition
        {
            var groupedGroups = group.GroupBy(x => (x.EventName, x.Sender));

            foreach (var groupedGroup in groupedGroups)
            {
                groupedEvents.Add(new EventDefinition<TService, ContentTypeChange<TType>.EventArgs>(
                    null,
                    (TService)groupedGroup.Key.Sender,
                    new ContentTypeChange<TType>.EventArgs(groupedGroup.SelectMany(x => ((ContentTypeChange<TType>.EventArgs)x.Args).Changes)),
                    groupedGroup.Key.EventName));
            }
        }
    }
}
