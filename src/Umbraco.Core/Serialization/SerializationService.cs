using System;
using System.IO;

namespace Umbraco.Core.Serialization
{
    public class SerializationService : AbstractSerializationService
    {
        private readonly ISerializer _serializer;

        public SerializationService(ISerializer serializer)
        {
            _serializer = serializer;
        }

        #region Overrides of AbstractSerializationService

        /// <summary>
        ///   Finds an <see cref="IFormatter" /> with a matching <paramref name="intent" /> , and deserializes the <see cref="Stream" /> <paramref
        ///    name="input" /> to an object graph.
        /// </summary>
        /// <param name="input"> </param>
        /// <param name="outputType"> </param>
        /// <param name="intent"> </param>
        /// <returns> </returns>
        public override object FromStream(Stream input, Type outputType, string intent = null)
        {
            if (input.CanSeek && input.Position > 0) input.Seek(0, SeekOrigin.Begin);
            return _serializer.FromStream(input, outputType);
        }

        /// <summary>
        ///   Finds an <see cref="IFormatter" /> with a matching <paramref name="intent" /> , and serializes the <paramref
        ///    name="input" /> object graph to an <see cref="IStreamedResult" /> .
        /// </summary>
        /// <param name="input"> </param>
        /// <param name="intent"> </param>
        /// <returns> </returns>
        public override IStreamedResult ToStream(object input, string intent = null)
        {
            return _serializer.ToStream(input);
        }

        #endregion
    }
}