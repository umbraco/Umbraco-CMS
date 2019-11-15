using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// An exception used to indicate that an Umbraco entity could not be found.
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Obsolete("Instead of throwing an exception, return null or an HTTP 404 status code instead.")]
    [Serializable]
    public class EntityNotFoundException : Exception
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        /// <remarks>
        /// This object should be serializable to prevent a <see cref="SerializationException" /> to be thrown.
        /// </remarks>
        public object Id { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException" /> class.
        /// </summary>
        public EntityNotFoundException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        public EntityNotFoundException(object id, string message)
            : base(message)
        {
            Id = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public EntityNotFoundException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException" /> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public EntityNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException" /> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected EntityNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Id = info.GetValue(nameof(Id), typeof(object));
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">info</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(Id), Id);

            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var result = base.ToString();

            if (Id != null)
            {
                return "Umbraco entity (id: " + Id + ") not found. " + result;
            }

            return result;
        }
    }
}
