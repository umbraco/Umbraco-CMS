using CSharpTest.Net.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.PublishedCache.NuCache
{

    /// <summary>
    /// Adapts ITransactableDictionarySerializer<T> to ISerializer<T>
    /// </summary>
    /// <typeparam name="T">Type to serialize/deserialize</typeparam>
    public class TransactableDictionaryBPlusTreeSerializerAdapter<T> : ISerializer<T>
    {
        private readonly ITransactableDictionarySerializer<T> _serializer;

        public TransactableDictionaryBPlusTreeSerializerAdapter(ITransactableDictionarySerializer<T> serializer)
        {
            _serializer = serializer;
        }

        public T ReadFrom(Stream stream)
        {
            return _serializer.ReadFrom(stream);
        }

        public void WriteTo(T value, Stream stream)
        {
            _serializer.WriteTo(value, stream);
        }
    }
}
