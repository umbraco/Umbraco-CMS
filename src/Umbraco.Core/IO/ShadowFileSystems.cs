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

        private static readonly object Locker = new object();
        private static Guid _currentId = Guid.Empty;
        private readonly Guid _id;
        private readonly ShadowWrapper[] _wrappers;
        private bool _completed;

        public ShadowFileSystems(Guid id, ShadowWrapper[] wrappers)
        {
            lock (Locker)
            {
                if (_currentId != Guid.Empty)
                    throw new InvalidOperationException("Already shadowing.");
                _currentId = id;

                LogHelper.Debug<ShadowFileSystems>("Shadow " + id + ".");
                _id = id;
                _wrappers = wrappers;
                foreach (var wrapper in _wrappers)
                    wrapper.Shadow(id);
            }
        }

        public void Complete()
        {
            _completed = true;
        }

        public void Dispose()
        {
            lock (Locker)
            {
                LogHelper.Debug<ShadowFileSystems>("UnShadow " + _id + " (" + (_completed ? "complete" : "abort") + ").");

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
            }
        }

        // for tests
        internal static void ResetId()
        {
            _currentId = Guid.Empty;
        }
    }
}
