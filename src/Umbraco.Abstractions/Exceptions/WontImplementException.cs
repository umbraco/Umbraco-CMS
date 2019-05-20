using System;

namespace Umbraco.Core.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a requested method or operation is not, and will not be, implemented.
    /// </summary>
    /// <remarks>The <see cref="NotImplementedException"/> is to be used when some code is not implemented,
    /// but should eventually be implemented (i.e. work in progress) and is reported by tools such as ReSharper.
    /// This exception is to be used when some code is not implemented, and is not meant to be, for whatever
    /// reason.</remarks>
    public class WontImplementException : NotImplementedException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WontImplementException"/> class.
        /// </summary>
        public WontImplementException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WontImplementException"/> class with a specified reason message.
        /// </summary>
        public WontImplementException(string message)
            : base(message)
        { }
    }
}
