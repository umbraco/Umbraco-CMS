using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents the pre-value data for a DataType
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
        private IEnumerable<PreValue> _preValuesAsArray;
        public IEnumerable<PreValue> PreValuesAsArray
        {
            get
            {
                if (_preValuesAsArray == null)
                {
                    throw new InvalidOperationException("The current pre-value collection is dictionary based, use the PreValuesAsDictionary property instead");
                }
                return _preValuesAsArray;
            }
            set { _preValuesAsArray = value; }
        }

        public IDictionary<string, PreValue> PreValuesAsDictionary
        {
            get
            {
                if (_preValuesAsDictionary == null)
                {
                    throw new InvalidOperationException("The current pre-value collection is array based, use the PreValuesAsArray property instead");
                }
                return _preValuesAsDictionary;
            }
            set { _preValuesAsDictionary = value; }
        }

        /// <summary>
        /// Check if it is a dictionary based collection
        /// </summary>
        public bool IsDictionaryBased
        {
            get { return _preValuesAsDictionary != null; }
        }

        public PreValueCollection(IEnumerable<PreValue> preVals)
        {
            _preValuesAsArray = preVals;
        }

        public PreValueCollection(IDictionary<string, PreValue> preVals)
        {
            _preValuesAsDictionary = preVals;
        }

        /// <summary>
        /// Regardless of how the pre-values are stored this will return as a dictionary, it will convert an array based to a dictionary
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, PreValue> FormatAsDictionary()
        {
            if (IsDictionaryBased)
            {
                return PreValuesAsDictionary;
            }

            //it's an array so need to format it, the alias will just be an iteration
            var result = new Dictionary<string, PreValue>();
            var asArray = PreValuesAsArray.ToArray();
            for (var i = 0; i < asArray.Length; i++)
            {
                result.Add(i.ToInvariantString(), asArray[i]);
            }
            return result;
        }

        public object DeepClone()
        {
            var clone = (PreValueCollection) MemberwiseClone();
            if (_preValuesAsArray != null)
            {
                clone._preValuesAsArray = _preValuesAsArray.Select(x => (PreValue)x.DeepClone()).ToArray();    
            }
            if (_preValuesAsDictionary != null)
            {
                clone._preValuesAsDictionary = _preValuesAsDictionary.ToDictionary(x => x.Key, x => (PreValue)x.Value.DeepClone());    
            }
            

            return clone;
        }
    }
}