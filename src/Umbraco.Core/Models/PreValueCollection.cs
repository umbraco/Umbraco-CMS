using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents the preValues for a data type.
    /// </summary>
    /// <remarks>
    /// Due to the legacy nature of the data that can be stored for pre-values, we have this class which encapsulates the 2 different
    /// ways that pre-values are stored: A string array or a Dictionary.
    ///
    /// Most legacy property editors won't support the dictionary format but new property editors should always use the dictionary format.
    /// In order to get overrideable pre-values working we need a dictionary since we'll have to reference a pre-value by a key.
    /// </remarks>
    public class PreValueCollection : IDeepCloneable
    {
        private IDictionary<string, PreValue> _preValuesAsDictionary;
        private PreValue[] _preValuesAsArray;

        /// <summary>
        /// Gets the collection as an array.
        /// </summary>
        public IEnumerable<PreValue> PreValuesAsArray
        {
            get => _preValuesAsArray
                ?? throw new InvalidOperationException("The current preValue collection is dictionary based, use the PreValuesAsDictionary property instead.");
            set => _preValuesAsArray = value.ToArray();
        }

        /// <summary>
        /// Gets the collection as a dictionary.
        /// </summary>
        public IDictionary<string, PreValue> PreValuesAsDictionary
        {
            get => _preValuesAsDictionary
                ?? throw new InvalidOperationException("The current preValue collection is array based, use the PreValuesAsArray property instead.");
            set => _preValuesAsDictionary = value;
        }

        /// <summary>
        /// Gets a value indicating whether the collection is dictionary-based.
        /// </summary>
        public bool IsDictionaryBased => _preValuesAsDictionary != null;

        /// <summary>
        /// Initializes a new array-based instance of the <seealso cref="PreValueCollection"/> class.
        /// </summary>
        public PreValueCollection(IEnumerable<PreValue> preVals)
        {
            _preValuesAsArray = preVals.ToArray();
        }

        /// <summary>
        /// Initializes a new dictionary-based instance of the <seealso cref="PreValueCollection"/> class.
        /// </summary>
        public PreValueCollection(IDictionary<string, PreValue> preVals)
        {
            _preValuesAsDictionary = preVals;
        }

        /// <summary>
        /// Gets the collection as a dictionary, even if it is array-based.
        /// </summary>
        public IDictionary<string, PreValue> FormatAsDictionary()
        {
            if (IsDictionaryBased)
                return PreValuesAsDictionary;

            var dictionary = new Dictionary<string, PreValue>();
            for (var i = 0; i < _preValuesAsArray.Length; i++)
                dictionary[i.ToInvariantString()] = _preValuesAsArray[i];
            return dictionary;
        }

        public object DeepClone()
        {
            var clone = (PreValueCollection) MemberwiseClone();

            if (_preValuesAsArray != null)
                clone._preValuesAsArray = _preValuesAsArray.Select(x => (PreValue) x.DeepClone()).ToArray();

            if (_preValuesAsDictionary != null)
                clone._preValuesAsDictionary = _preValuesAsDictionary.ToDictionary(x => x.Key, x => (PreValue) x.Value.DeepClone());

            return clone;
        }
    }
}
