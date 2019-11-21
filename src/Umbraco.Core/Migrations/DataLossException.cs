using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Migrations
{
    /// <summary>
    /// The exception that is thrown if a migration has executed, but the whole process has failed and cannot be rolled back.
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    internal class DataLossException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataLossException" /> class.
        /// </summary>
        public DataLossException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataLossException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DataLossException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataLossException" /> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The inner exception.</param>
        public DataLossException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataLossException" /> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected DataLossException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
