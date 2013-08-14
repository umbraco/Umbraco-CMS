using System;
using System.Collections.Generic;

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
    public class PreValueCollection
    {
        private IDictionary<string, object> _preValuesAsDictionary;
        private IEnumerable<string> _preValuesAsArray;
        public IEnumerable<string> PreValuesAsArray
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

        public IDictionary<string, object> PreValuesAsDictionary
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

        public PreValueCollection(IEnumerable<string> preVals)
        {
            _preValuesAsArray = preVals;
        }

        public PreValueCollection(IDictionary<string, object> preVals)
        {
            _preValuesAsDictionary = preVals;
        }
    }
}