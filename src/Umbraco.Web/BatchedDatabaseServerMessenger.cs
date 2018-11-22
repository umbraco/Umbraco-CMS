using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using umbraco.interfaces;
using Umbraco.Core;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Sync;
using Umbraco.Web.Routing;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Web.Scheduling;

namespace Umbraco.Web
{
    /// <summary>
    /// An <see cref="IServerMessenger"/> implementation that works by storing messages in the database.
    /// </summary>
    /// <remarks>
    /// This binds to appropriate umbraco events in order to trigger the Boot(), Sync() & FlushBatch() calls
    /// </remarks>
    public class BatchedDatabaseServerMessenger : DatabaseServerMessenger
    {
        private readonly ApplicationContext _appContext;

        public BatchedDatabaseServerMessenger(ApplicationContext appContext, bool enableDistCalls, DatabaseServerMessengerOptions options)
            : base(appContext, enableDistCalls, options)
        {
            _appContext = appContext;
            Scheduler.Initializing += Scheduler_Initializing;
        }

        /// <summary>
        /// Occurs when the scheduler initializes all scheduling activity when the app is ready
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Scheduler_Initializing(object sender, List<IBackgroundTask> e)
        {
            //if the current resolver is 'this' then we will start the scheduling
            var isMessenger = ServerMessengerResolver.HasCurrent && ReferenceEquals(ServerMessengerResolver.Current.Messenger, this);

            if (isMessenger)
            {
                //start the background task runner for processing instructions
                const int delayMilliseconds = 60000;
                var instructionProcessingRunner = new BackgroundTaskRunner<IBackgroundTask>("InstructionProcessing", ApplicationContext.ProfilingLogger.Logger);
                var instructionProcessingTask = new InstructionProcessing(instructionProcessingRunner, this, _appContext.ScopeProvider, ApplicationContext.ProfilingLogger.Logger, delayMilliseconds, Options.ThrottleSeconds * 1000);
                instructionProcessingRunner.TryAdd(instructionProcessingTask);
                e.Add(instructionProcessingTask);
            }
        }


        // invoked by BatchedDatabaseServerMessengerStartup which is an ApplicationEventHandler
        // with default "ShouldExecute", so that method will run if app IsConfigured and database
        // context IsDatabaseConfigured - we still want to check CanConnect though to be safe
        internal void Startup()
        {
            UmbracoModule.EndRequest += UmbracoModule_EndRequest;

            if (ApplicationContext.DatabaseContext.CanConnect == false)
            {
                ApplicationContext.ProfilingLogger.Logger.Warn<BatchedDatabaseServerMessenger>(
                    "Cannot connect to the database, distributed calls will not be enabled for this server.");
            }
            else
            {
                Boot();
            }
        }

        /// <summary>
        /// This will process cache instructions on a background thread and will run every 5 seconds (or whatever is defined in the <see cref="DatabaseServerMessengerOptions.ThrottleSeconds"/>)
        /// </summary>
        private class InstructionProcessing : RecurringTaskBase
        {
            private readonly DatabaseServerMessenger _messenger;
            private readonly IScopeProvider _scopeProvider;
            private readonly ILogger _logger;

            public InstructionProcessing(IBackgroundTaskRunner<RecurringTaskBase> runner,
                DatabaseServerMessenger messenger,
                IScopeProvider scopeProvider,
                ILogger logger,
                int delayMilliseconds, int periodMilliseconds)
                : base(runner, delayMilliseconds, periodMilliseconds)
            {
                _messenger = messenger;
                _scopeProvider = scopeProvider;
                _logger = logger;
            }

            public override bool PerformRun()
            {
                try
                {
                    TryPerformRun();
                }
                catch (Exception e)
                {
                    _logger.Error<InstructionProcessing>("Failed (will repeat).", e);
                }

                //return true to repeat
                return true;
            }

            private void TryPerformRun()
            {
                // beware!
                // DatabaseServerMessenger uses _appContext.DatabaseContext.Database without creating
                // scopes, and since we are running in a background task, there will be no ambient
                // scope (as would be the case within a web request), and so we would end up creating
                // (and leaking) a NoScope instance, which is bad - better make sure we have a true
                // scope here! - see U4-11207
                using (var scope = _scopeProvider.CreateScope())
                {
                    _messenger.Sync();
                    scope.Complete();
                }
            }

            public override bool IsAsync
            {
                get { return false; }
            }
        }

        private void UmbracoModule_EndRequest(object sender, UmbracoRequestEventArgs e)
        {
            // will clear the batch - will remain in HttpContext though - that's ok
            FlushBatch();
        }

        protected override void DeliverRemote(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, MessageType messageType, IEnumerable<object> ids = null, string json = null)
        {
            var idsA = ids == null ? null : ids.ToArray();

            Type arrayType;
            if (GetArrayType(idsA, out arrayType) == false)
                throw new ArgumentException("All items must be of the same type, either int or Guid.", "ids");

            BatchMessage(servers, refresher, messageType, idsA, arrayType, json);
        }

        public void FlushBatch()
        {
            var batch = GetBatch(false);
            if (batch == null) return;

            var instructions = batch.SelectMany(x => x.Instructions).ToArray();
            batch.Clear();

            //Write the instructions but only create JSON blobs with a max instruction count equal to MaxProcessingInstructionCount
            using (var scope = _appContext.ScopeProvider.CreateScope())
            {
                foreach (var instructionsBatch in instructions.InGroupsOf(Options.MaxProcessingInstructionCount))
                {
                    WriteInstructions(scope, instructionsBatch);
                }
                scope.Complete();
            }
        }

        private void WriteInstructions(IScope scope, IEnumerable<RefreshInstruction> instructions)
        {
            var dto = new CacheInstructionDto
            {
                UtcStamp = DateTime.UtcNow,
                Instructions = JsonConvert.SerializeObject(instructions, Formatting.None),
                OriginIdentity = LocalIdentity,
                InstructionCount = instructions.Sum(x => x.JsonIdCount)
            };
            scope.Database.Insert(dto);
        }

        protected ICollection<RefreshInstructionEnvelope> GetBatch(bool create)
        {
            // try get the http context from the UmbracoContext, we do this because in the case we are launching an async
            // thread and we know that the cache refreshers will execute, we will ensure the UmbracoContext and therefore we
            // can get the http context from it
            var httpContext = (UmbracoContext.Current == null ? null : UmbracoContext.Current.HttpContext)
                // if this is null, it could be that an async thread is calling this method that we weren't aware of and the UmbracoContext
                // wasn't ensured at the beginning of the thread. We can try to see if the HttpContext.Current is available which might be
                // the case if the asp.net synchronization context has kicked in
                ?? (HttpContext.Current == null ? null : new HttpContextWrapper(HttpContext.Current));

            // if no context was found, return null - we cannot not batch
            if (httpContext == null) return null;

            var key = typeof (BatchedDatabaseServerMessenger).Name;

            // no thread-safety here because it'll run in only 1 thread (request) at a time
            var batch = (ICollection<RefreshInstructionEnvelope>)httpContext.Items[key];
            if (batch == null && create)
                httpContext.Items[key] = batch = new List<RefreshInstructionEnvelope>();
            return batch;
        }

        protected void BatchMessage(
            IEnumerable<IServerAddress> servers,
            ICacheRefresher refresher,
            MessageType messageType,
            IEnumerable<object> ids = null,
            Type idType = null,
            string json = null)
        {
            var batch = GetBatch(true);
            var instructions = RefreshInstruction.GetInstructions(refresher, messageType, ids, idType, json);

            // batch if we can, else write to DB immediately
            if (batch == null)
            {
                //only write the json blob with a maximum count of the MaxProcessingInstructionCount
                using (var scope = _appContext.ScopeProvider.CreateScope())
                {
                    foreach (var maxBatch in instructions.InGroupsOf(Options.MaxProcessingInstructionCount))
                    {
                        WriteInstructions(scope, maxBatch);
                    }
                    scope.Complete();
                }
            }
            else
            {
                batch.Add(new RefreshInstructionEnvelope(servers, refresher, instructions));
            }
        }
    }
}
