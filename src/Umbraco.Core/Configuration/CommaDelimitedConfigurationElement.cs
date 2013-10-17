using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;

namespace Umbraco.Core.Configuration
{
    /// <summary>
    /// Defines a configuration section that contains inner text that is comma delimited
    /// </summary>
    internal class CommaDelimitedConfigurationElement : InnerTextConfigurationElement<CommaDelimitedStringCollection>, IEnumerable<string>
    {
        public override CommaDelimitedStringCollection Value
        {
            get 
            { 
                var converter = new CommaDelimitedStringCollectionConverter();
                return (CommaDelimitedStringCollection) converter.ConvertFrom(RawValue);
            }
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            return new InnerEnumerator(Value.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new InnerEnumerator(Value.GetEnumerator());
        }

        /// <summary>
        /// A wrapper for StringEnumerator since it doesn't explicitly implement IEnumerable
        /// </summary>
        private class InnerEnumerator : IEnumerator<string>
        {
            private readonly StringEnumerator _stringEnumerator;

            public InnerEnumerator(StringEnumerator stringEnumerator)
            {
                _stringEnumerator = stringEnumerator;
            }

            public bool MoveNext()
            {
                return _stringEnumerator.MoveNext();
            }

            public void Reset()
            {
                _stringEnumerator.Reset();
            }

            string IEnumerator<string>.Current
            {
                get { return _stringEnumerator.Current; }
            }

            public object Current
            {
                get { return _stringEnumerator.Current; }
            }

            public void Dispose()
            {
                ObjectExtensions.DisposeIfDisposable(_stringEnumerator);
            }
        }
    }
}