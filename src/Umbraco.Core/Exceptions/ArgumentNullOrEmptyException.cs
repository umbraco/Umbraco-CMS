using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a null reference, or an empty argument, is passed to a method that does not accept it as a valid argument.
    /// </summary>
    /// <seealso cref="System.ArgumentNullException" />
    [Obsolete("Throw an ArgumentNullException when the parameter is null or an ArgumentException when its empty instead.")]
    [Serializable]
    public class ArgumentNullOrEmptyException : ArgumentNullException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentNullOrEmptyException" /> class.
        /// </summary>
        public ArgumentNullOrEmptyException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentNullOrEmptyException" /> class with the name of the parameter that caused this exception.
        /// </summary>
        /// <param name="paramName">The named of the parameter that caused the exception.</param>
        public ArgumentNullOrEmptyException(string paramName)
            : base(paramName)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentNullOrEmptyException" /> class with a specified error message and the name of the parameter that caused this exception.
        /// </summary>
        /// <param name="paramName">The named of the parameter that caused the exception.</param>
        /// <param name="message">A message that describes the error.</param>
        public ArgumentNullOrEmptyException(string paramName, string message)
            : base(paramName, message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentNullOrEmptyException" /> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for this exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public ArgumentNullOrEmptyException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentNullOrEmptyException" /> class.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">An object that describes the source or destination of the serialized data.</param>
        protected ArgumentNullOrEmptyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
