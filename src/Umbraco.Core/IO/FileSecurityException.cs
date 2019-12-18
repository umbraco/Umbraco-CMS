using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.IO
{
    /// <summary>
    /// The exception that is thrown when the caller does not have the required permission to access a file.
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Obsolete("Throw an UnauthorizedAccessException instead.")]
    [Serializable]
    public class FileSecurityException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSecurityException" /> class.
        /// </summary>
        public FileSecurityException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSecurityException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public FileSecurityException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSecurityException" /> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public FileSecurityException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSecurityException" /> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected FileSecurityException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
