using System.Collections.Generic;
using Umbraco.Core;

namespace Umbraco.Tests.TestHelpers
{
    internal class TestScopeContextAdapter : IScopeContextAdapter
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        public object Get(string key)
        {
            object value;
            return _values.TryGetValue(key, out value) ? value : null;
        }

        public void Set(string key, object value)
        {
            _values[key] = value;
        }

        public void Clear(string key)
        {
            _values.Remove(key);
        }
    }
}
