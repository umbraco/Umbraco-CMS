using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Provides a base class for configurations that support additional data.
    /// </summary>
    public abstract class ConfigurationWithAdditionalData
    {
        private Dictionary<string, object> _values;

        // note: this class should NOT define ANY property, so that configuration
        // classes that inherit from it, can do what they want with properties

        /// <summary>
        /// Gets an additional value.
        /// </summary>
        /// <exception cref="KeyNotFoundException">No value exists for the key.</exception>
        public object GetAdditionalValue(string key)
        {
            if (_values != null && _values.TryGetValue(key, out var obj))
                return obj;
            throw new KeyNotFoundException($"No value exists for key \"{key}\".");
        }

        /// <summary>
        /// Sets an additional value.
        /// </summary>
        public void SetAdditionalValue(string key, object value)
        {
            if (value == null)
            {
                if (_values == null) return;
                _values.Remove(key);
            }
            else
            {
                if (_values == null) _values = new Dictionary<string, object>();
                _values[key] = value;
            }
        }

        /// <summary>
        /// Removes an additional value.
        /// </summary>
        public void RemoveAdditionalValue(string key)
        {
            _values?.Remove(key);
        }

        /// <summary>
        /// Determines whether the configuration contains a additional value.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsAdditionalKey(string key)
            => _values != null && _values.ContainsKey(key);

        /// <summary>
        /// Tries to get an additional value.
        /// </summary>
        public bool TryGetAdditionalValue(string key, out object obj)
        {
            obj = null;
            return _values != null && _values.TryGetValue(key, out obj);
        }

        /// <summary>
        /// Determines whether the configuration has additional values.
        /// </summary>
        /// <returns></returns>
        public bool HasAdditionalValues()
            => _values != null && _values.Count > 0;

        /// <summary>
        /// Gets additional values.
        /// </summary>
        public IEnumerable<KeyValuePair<string, object>> GetAdditionalValues()
            => _values ?? Enumerable.Empty<KeyValuePair<string, object>>();
    }
}