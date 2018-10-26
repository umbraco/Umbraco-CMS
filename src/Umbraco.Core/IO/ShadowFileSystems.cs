using System;
using System.Collections.Generic;
using Umbraco.Core.Logging;

namespace Umbraco.Core.IO
{
    internal class ShadowFileSystems : ICompletable
    {
        private static readonly object Locker = new object();
        private static Guid _currentId = Guid.Empty;

        private readonly Guid _id;
        private readonly ShadowWrapper[] _wrappers;
        private readonly ILogger _logger;

        private bool _completed;

        // invoked by the filesystems when shadowing
        // can only be 1 shadow at a time (static)
        public ShadowFileSystems(Guid id, ShadowWrapper[] wrappers, ILogger logger)
        {
            lock (Locker)
            {
                // if we throw here, it means that something very wrong happened.
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

        // invoked by the scope when exiting, if completed
        public void Complete()
        {
            _completed = true;
        }

        // invoked by the scope when exiting
        public void Dispose()
        {
            lock (Locker)
            {
                // if we throw here, it means that something very wrong happened.
                if (_currentId == Guid.Empty)
                    throw new InvalidOperationException("Not shadowing.");
                if (_id != _currentId)
                    throw new InvalidOperationException("Not the current shadow.");

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
            }
        }

        // for tests
        internal static void ResetId()
        {
            _currentId = Guid.Empty;
        }
    }
}
