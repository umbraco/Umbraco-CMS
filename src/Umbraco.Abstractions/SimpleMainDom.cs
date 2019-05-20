using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides a simple implementation of <see cref="IMainDom"/>.
    /// </summary>
    public class SimpleMainDom : IMainDom
    {
        private readonly object _locko = new object();
        private readonly List<KeyValuePair<int, Action>> _callbacks = new List<KeyValuePair<int, Action>>();
        private bool _isStopping;

        /// <inheritdoc />
        public bool IsMainDom { get; private set; } = true;

        /// <inheritdoc />
        public bool Register(Action release, int weight = 100)
            => Register(null, release, weight);

        /// <inheritdoc />
        public bool Register(Action install, Action release, int weight = 100)
        {
            lock (_locko)
            {
                if (_isStopping) return false;
                install?.Invoke();
                if (release != null)
                    _callbacks.Add(new KeyValuePair<int, Action>(weight, release));
                return true;
            }
        }

        public void Stop()
        {
            lock (_locko)
            {
                if (_isStopping) return;
                if (IsMainDom == false) return; // probably not needed
                _isStopping = true;
            }

            try
            {
                foreach (var callback in _callbacks.OrderBy(x => x.Key).Select(x => x.Value))
                {
                    callback(); // no timeout on callbacks
                }
            }
            finally
            {
                // in any case...
                IsMainDom = false;
            }
        }
    }
}
