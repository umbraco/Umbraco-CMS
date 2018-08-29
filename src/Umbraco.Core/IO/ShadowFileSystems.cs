using System;
using System.Collections.Generic;
using Umbraco.Core.Logging;

namespace Umbraco.Core.IO
{
    internal class ShadowFileSystems : ICompletable
    {
        // note: taking a reference to the _manager instead of using manager.Current
        // to avoid using Current everywhere but really, we support only 1 scope at
        // a time, not multiple scopes in case of multiple managers (not supported)

        // fixme - why are we managing logical call context here? should be bound
        // to the current scope, always => REFACTOR! but there should be something in
        // place (static?) to ensure we only have one concurrent shadow FS?
        //
        // => yes, that's _currentId - need to cleanup this entirely
        // and, we probably need a way to stop shadowing entirely without cycling the app

        private const string ItemKey = "Umbraco.Core.IO.ShadowFileSystems";

        private static readonly object Locker = new object();
        private static Guid _currentId = Guid.Empty;

        private readonly Guid _id;
        private readonly ShadowWrapper[] _wrappers;
        private readonly ILogger _logger;

        private bool _completed;

        //static ShadowFileSystems()
        //{
        //    SafeCallContext.Register(
        //        () =>
        //        {
        //            var scope = CallContext.LogicalGetData(ItemKey);
        //            CallContext.FreeNamedDataSlot(ItemKey);
        //            return scope;
        //        },
        //        o =>
        //        {
        //            if (CallContext.LogicalGetData(ItemKey) != null) throw new InvalidOperationException();
        //            if (o != null) CallContext.LogicalSetData(ItemKey, o);
        //        });
        //}

        public ShadowFileSystems(Guid id, ShadowWrapper[] wrappers, ILogger logger)
        {
            lock (Locker)
            {
                if (_currentId != Guid.Empty)
                    throw new InvalidOperationException("Already shadowing.");
                _currentId = id;
            }

            _logger = logger;
            _logger.Debug<ShadowFileSystems>("Shadow '{ShadowId}'", id);
            _id = id;

            _wrappers = wrappers;
            foreach (var wrapper in _wrappers)
                wrapper.Shadow(id);
        }

        // fixme - remove
        //// internal for tests + FileSystems
        //// do NOT use otherwise
        //internal static ShadowFileSystems CreateScope(Guid id, ShadowWrapper[] wrappers, ILogger logger)
        //{
        //    lock (Locker)
        //    {
        //        if (CallContext.LogicalGetData(ItemKey) != null) throw new InvalidOperationException("Already shadowing.");
        //        CallContext.LogicalSetData(ItemKey, ItemKey); // value does not matter
        //    }
        //    return new ShadowFileSystems(id, wrappers, logger);
        //}

        //internal static bool InScope => NoScope == false;

        //internal static bool NoScope => CallContext.LogicalGetData(ItemKey) == null;

        public void Complete()
        {
            _completed = true;
            //lock (Locker)
            //{
            //    _logger.Debug<ShadowFileSystems>("UnShadow " + _id + " (complete).");

            //    var exceptions = new List<Exception>();
            //    foreach (var wrapper in _wrappers)
            //    {
            //        try
            //        {
            //            // this may throw an AggregateException if some of the changes could not be applied
            //            wrapper.UnShadow(true);
            //        }
            //        catch (AggregateException ae)
            //        {
            //            exceptions.Add(ae);
            //        }
            //    }

            //    if (exceptions.Count > 0)
            //        throw new AggregateException("Failed to apply all changes (see exceptions).", exceptions);

            //    // last, & *only* if successful (otherwise we'll unshadow & cleanup as best as we can)
            //    CallContext.FreeNamedDataSlot(ItemKey);
            //}
        }

        public void Dispose()
        {
            lock (Locker)
            {
                _logger.Debug<ShadowFileSystems>("UnShadow '{ShadowId}' {Status}", _id, _completed ? "complete" : "abort");

                var exceptions = new List<Exception>();
                foreach (var wrapper in _wrappers)
                {
                    try
                    {
                        // this may throw an AggregateException if some of the changes could not be applied
                        wrapper.UnShadow(_completed);
                    }
                    catch (AggregateException ae)
                    {
                        exceptions.Add(ae);
                    }
                }

                _currentId = Guid.Empty;

                if (exceptions.Count > 0)
                    throw new AggregateException(_completed ? "Failed to apply all changes (see exceptions)." : "Failed to abort (see exceptions).", exceptions);

                //if (CallContext.LogicalGetData(ItemKey) == null) return;

                //try
                //{
                //    _logger.Debug<ShadowFileSystems>("UnShadow " + _id + " (abort)");
                //    foreach (var wrapper in _wrappers)
                //        wrapper.UnShadow(false); // should not throw
                //}
                //finally
                //{
                //    // last, & always
                //    CallContext.FreeNamedDataSlot(ItemKey);
                //}
            }
        }

        // for tests
        internal static void ResetId()
        {
            _currentId = Guid.Empty;
        }
    }
}
