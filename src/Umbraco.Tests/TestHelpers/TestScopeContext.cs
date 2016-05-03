using System.Collections.Generic;
using Umbraco.Core;

namespace Umbraco.Tests.TestHelpers
{
    internal class TestScopeContext : IScopeContext
    {
        private readonly Dictionary<string, object> _d = new Dictionary<string, object>();

        public object GetData(string key)
        {
            if (_d.ContainsKey(key)) return _d[key];
            return null;
        }

        public void SetData(string key, object data)
        {
            _d[key] = data;
        }

        public void ClearData(string key)
        {
            if (_d.ContainsKey(key))
                _d.Remove(key);
        }
    }
}