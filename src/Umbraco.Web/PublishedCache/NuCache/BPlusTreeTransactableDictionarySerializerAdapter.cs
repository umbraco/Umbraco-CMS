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
    /// Adapts ISerializer<T> to ITransactableDictionarySerializer<T>
    /// </summary>
    /// <typeparam name="T">Type to serialize/deserialize</typeparam>
    public class BPlusTreeTransactableDictionarySerializerAdapter<T> : ITransactableDictionarySerializer<T>
    {
        private readonly ISerializer<T> _serializer;

        public BPlusTreeTransactableDictionarySerializerAdapter(ISerializer<T> serializer)
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
