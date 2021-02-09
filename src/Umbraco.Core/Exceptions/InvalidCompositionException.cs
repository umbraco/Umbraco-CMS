using System;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a composition is invalid.
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class InvalidCompositionException : Exception
    {
        /// <summary>
        /// Gets the content type alias.
        /// </summary>
        /// <value>
        /// The content type alias.
        /// </value>
        public string ContentTypeAlias { get; }

        /// <summary>
        /// Gets the added composition alias.
        /// </summary>
        /// <value>
        /// The added composition alias.
        /// </value>
        public string AddedCompositionAlias { get; }

        /// <summary>
        /// Gets the property type aliases.
        /// </summary>
        /// <value>
        /// The property type aliases.
        /// </value>
        public string[] PropertyTypeAliases { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCompositionException" /> class.
        /// </summary>
        public InvalidCompositionException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCompositionException" /> class.
        /// </summary>
        /// <param name="contentTypeAlias">The content type alias.</param>
        /// <param name="propertyTypeAliases">The property type aliases.</param>
        public InvalidCompositionException(string contentTypeAlias, string[] propertyTypeAliases)
            : this(contentTypeAlias, null, propertyTypeAliases)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCompositionException" /> class.
        /// </summary>
        /// <param name="contentTypeAlias">The content type alias.</param>
        /// <param name="addedCompositionAlias">The added composition alias.</param>
        /// <param name="propertyTypeAliases">The property type aliases.</param>
        public InvalidCompositionException(string contentTypeAlias, string addedCompositionAlias, string[] propertyTypeAliases)
            : this(addedCompositionAlias.IsNullOrWhiteSpace()
                    ? string.Format(
                        "ContentType with alias '{0}' has an invalid composition " +
                        "and there was a conflict on the following PropertyTypes: '{1}'. " +
                        "PropertyTypes must have a unique alias across all Compositions in order to compose a valid ContentType Composition.",
                        contentTypeAlias, string.Join(", ", propertyTypeAliases))
                    : string.Format(
                        "ContentType with alias '{0}' was added as a Composition to ContentType with alias '{1}', " +
                        "but there was a conflict on the following PropertyTypes: '{2}'. " +
                        "PropertyTypes must have a unique alias across all Compositions in order to compose a valid ContentType Composition.",
                        addedCompositionAlias, contentTypeAlias, string.Join(", ", propertyTypeAliases)))
        {
            ContentTypeAlias = contentTypeAlias;
            AddedCompositionAlias = addedCompositionAlias;
            PropertyTypeAliases = propertyTypeAliases;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCompositionException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InvalidCompositionException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCompositionException" /> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public InvalidCompositionException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCompositionException" /> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected InvalidCompositionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ContentTypeAlias = info.GetString(nameof(ContentTypeAlias));
            AddedCompositionAlias = info.GetString(nameof(AddedCompositionAlias));
            PropertyTypeAliases = (string[])info.GetValue(nameof(PropertyTypeAliases), typeof(string[]));
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

            info.AddValue(nameof(ContentTypeAlias), ContentTypeAlias);
            info.AddValue(nameof(AddedCompositionAlias), AddedCompositionAlias);
            info.AddValue(nameof(PropertyTypeAliases), PropertyTypeAliases);

            base.GetObjectData(info, context);
        }
    }
}
