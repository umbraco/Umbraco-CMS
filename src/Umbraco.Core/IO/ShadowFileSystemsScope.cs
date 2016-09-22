using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Umbraco.Core.Logging;

namespace Umbraco.Core.IO
{
    public class ShadowFileSystemsScope : IDisposable
    {
        // note: taking a reference to the _manager instead of using manager.Current
        // to avoid using Current everywhere but really, we support only 1 scope at
        // a time, not multiple scopes in case of multiple managers (not supported)

        private const string ItemKey = "Umbraco.Core.IO.ShadowFileSystemsScope";
        private static readonly object Locker = new object();
        private readonly Guid _id;
        private readonly ShadowWrapper[] _wrappers;

        static ShadowFileSystemsScope()
        {
            SafeCallContext.Register(
                () =>
                {
                    var scope = CallContext.LogicalGetData(ItemKey);
                    CallContext.FreeNamedDataSlot(ItemKey);
                    return scope;
                },
                o =>
                {
                    if (CallContext.LogicalGetData(ItemKey) != null) throw new InvalidOperationException();
                    if (o != null) CallContext.LogicalSetData(ItemKey, o);
                });
        }

        private ShadowFileSystemsScope(Guid id, ShadowWrapper[] wrappers)
        {
            LogHelper.Debug<ShadowFileSystemsScope>("Shadow " + id + ".");
            _id = id;
            _wrappers = wrappers;
            foreach (var wrapper in _wrappers)
                wrapper.Shadow(id);
        }

        // internal for tests + FileSystemProviderManager
        // do NOT use otherwise
        internal static ShadowFileSystemsScope CreateScope(Guid id, ShadowWrapper[] wrappers)
        {
            lock (Locker)
            {
                if (CallContext.LogicalGetData(ItemKey) != null) throw new InvalidOperationException("Already shadowing.");
                CallContext.LogicalSetData(ItemKey, ItemKey); // value does not matter
            }
            return new ShadowFileSystemsScope(id, wrappers);
        }

        internal static bool InScope
        {
            get { return NoScope == false; }
        }

        internal static bool NoScope
        {
            get { return CallContext.LogicalGetData(ItemKey) == null; }
        }

        public void Complete()
        {
            lock (Locker)
            {
                LogHelper.Debug<ShadowFileSystemsScope>("UnShadow " + _id + " (complete).");

                var exceptions = new List<Exception>();
                foreach (var wrapper in _wrappers)
                {
                    try
                    {
                        // this may throw an AggregateException if some of the changes could not be applied
                        wrapper.UnShadow(true);
                    }
                    catch (AggregateException ae)
                    {
                        exceptions.Add(ae);
                    }
                }

                if (exceptions.Count > 0)
                    throw new AggregateException("Failed to apply all changes (see exceptions).", exceptions);

                // last, & *only* if successful (otherwise we'll unshadow & cleanup as best as we can)
                CallContext.FreeNamedDataSlot(ItemKey);
            }
        }

        public void Dispose()
        {
            lock (Locker)
            {
                if (CallContext.LogicalGetData(ItemKey) == null) return;

                try
                {
                    LogHelper.Debug<ShadowFileSystemsScope>("UnShadow " + _id + " (abort)");
                    foreach (var wrapper in _wrappers)
                        wrapper.UnShadow(false); // should not throw
                }
                finally
                {
                    // last, & always
                    CallContext.FreeNamedDataSlot(ItemKey);
                }
            }
        }
    }
}
