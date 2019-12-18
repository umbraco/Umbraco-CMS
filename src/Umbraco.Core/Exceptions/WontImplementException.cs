using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a requested method or operation is not, and will not be, implemented.
    /// </summary>
    /// <remarks>
    /// The <see cref="NotImplementedException" /> is to be used when some code is not implemented,
    /// but should eventually be implemented (i.e. work in progress) and is reported by tools such as ReSharper.
    /// This exception is to be used when some code is not implemented, and is not meant to be, for whatever
    /// reason.
    /// </remarks>
    /// <seealso cref="System.NotImplementedException" />
    [Serializable]
    [Obsolete("If a method or operation is not, and will not be, implemented, it is invalid or not supported, so we should throw either an InvalidOperationException or NotSupportedException instead.")]
    public class WontImplementException : NotImplementedException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WontImplementException" /> class.
        /// </summary>
        public WontImplementException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WontImplementException" /> class with a specified reason message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public WontImplementException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WontImplementException" /> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner" /> parameter is not <see langword="null" />, the current exception is raised in a <see langword="catch" /> block that handles the inner exception.</param>
        public WontImplementException(string message, Exception inner)
            : base(message, inner)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WontImplementException" /> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected WontImplementException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
