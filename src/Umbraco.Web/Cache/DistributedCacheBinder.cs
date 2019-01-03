using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Default <see cref="IDistributedCacheBinder"/> implementation.
    /// </summary>
    public partial class DistributedCacheBinder : IDistributedCacheBinder
    {
        private static readonly ConcurrentDictionary<string, MethodInfo> FoundHandlers = new ConcurrentDictionary<string, MethodInfo>();
        private readonly DistributedCache _distributedCache;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedCacheBinder"/> class.
        /// </summary>
        /// <param name="distributedCache"></param>
        /// <param name="logger"></param>
        public DistributedCacheBinder(DistributedCache distributedCache, ILogger logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;
        }

        // internal for tests
        internal static MethodInfo FindHandler(IEventDefinition eventDefinition)
        {
            var name = eventDefinition.Sender.GetType().Name + "_" + eventDefinition.EventName;

            return FoundHandlers.GetOrAdd(name, n => CandidateHandlers.Value.FirstOrDefault(x => x.Name == n));
        }

        private static readonly Lazy<MethodInfo[]> CandidateHandlers = new Lazy<MethodInfo[]>(() =>
        {
            var underscore = new[] { '_' };

            return typeof(DistributedCacheBinder)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(x =>
                {
                    if (x.Name.Contains("_") == false) return null;

                    var parts = x.Name.Split(underscore, StringSplitOptions.RemoveEmptyEntries).Length;
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
            // ensure we run with an UmbracoContext, because this may run in a background task,
            // yet developers may be using the 'current' UmbracoContext in the event handlers
            using (UmbracoContext.EnsureContext())
            {
                foreach (var e in events)
                {
                    var handler = FindHandler(e);
                    if (handler == null)
                    {
                        // fixme - should this be fatal (ie, an exception)?
                        var name = e.Sender.GetType().Name + "_" + e.EventName;
                        _logger.Warn<DistributedCacheBinder>("Dropping event {EventName} because no corresponding handler was found.", name);
                        continue;
                    }

                    handler.Invoke(this, new[] { e.Sender, e.Args });
                }
            }
        }
    }
}
