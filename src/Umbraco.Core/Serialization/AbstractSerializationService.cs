using System;
using System.Collections.Generic;
using System.IO;

namespace Umbraco.Core.Serialization
{
    public abstract class AbstractSerializationService
    {
        /// <summary>
        ///   A sequence of <see cref="IFormatter" /> registered with this serialization service.
        /// </summary>
        public IEnumerable<IFormatter> Formatters { get; set; }

        /// <summary>
        ///   Finds an <see cref="IFormatter" /> with a matching <paramref name="intent" /> , and deserializes the <see cref="Stream" /> <paramref
        ///    name="input" /> to an object graph.
        /// </summary>
        /// <param name="input"> </param>
        /// <param name="outputType"> </param>
        /// <param name="intent"> </param>
        /// <returns> </returns>
        public abstract object FromStream(Stream input, Type outputType, string intent = null);

        /// <summary>
        ///   Finds an <see cref="IFormatter" /> with a matching <paramref name="intent" /> , and serializes the <paramref
        ///    name="input" /> object graph to an <see cref="IStreamedResult" /> .
        /// </summary>
        /// <param name="input"> </param>
        /// <param name="intent"> </param>
        /// <returns> </returns>
        public abstract IStreamedResult ToStream(object input, string intent = null);
    }
}
