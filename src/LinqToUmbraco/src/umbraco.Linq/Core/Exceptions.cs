using System;

namespace umbraco.Linq.Core
{
    /// <summary>
    /// Exception for when the provided class does not meet the expected class
    /// </summary>
    [global::System.Serializable]
    public class DocTypeMissMatchException : ArgumentException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocTypeMissMatchException"/> class.
        /// </summary>
        /// <param name="actual">The actual doc type alias.</param>
        /// <param name="expected">The expcected doc type alias.</param>
        public DocTypeMissMatchException(string actual, string expected) : this(actual, expected, string.Empty) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="DocTypeMissMatchException"/> class.
        /// </summary>
        /// <param name="actual">The actual doc type alias.</param>
        /// <param name="expected">The expcected doc type alias.</param>
        /// <param name="message">Additional message information.</param>
        public DocTypeMissMatchException(string actual, string expected, string message)
            : base(string.Format("DocTypeAlias provided did not match what was expected (provided: {0}, expected: {1}){2}{3}", actual, expected, Environment.NewLine, message))
        {
            Expected = expected;
            Actual = actual;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DocTypeMissMatchException"/> class.
        /// </summary>
        /// <param name="actual">The actual doc type alias.</param>
        /// <param name="expected">The expcected doc type alias.</param>
        /// <param name="message">Additional message information.</param>
        /// <param name="innerException">The inner exception.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object[])")]
        public DocTypeMissMatchException(string actual, string expected, string message, Exception innerException)
            : base(string.Format("DocTypeAlias provided did not match what was expected (provided: {0}, expected: {1}){2}{3}", actual, expected, Environment.NewLine, message), innerException)
        {
            Expected = expected;
            Actual = actual;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MandatoryFailureException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="info"/> parameter is null.
        /// </exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">
        /// The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0).
        /// </exception>
        protected DocTypeMissMatchException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        /// <summary>
        /// Gets or sets the expected DocTypeAlias
        /// </summary>
        /// <value>The expected DocTypeAlias.</value>
        public string Expected { get; set; }
        /// <summary>
        /// Gets or sets the actual DocTypeAlias
        /// </summary>
        /// <value>The actual DocTypeAlias.</value>
        public string Actual { get; set; }
    }

    /// <summary>
    /// Exception raised when a field isn't meeting its mandatory requirement
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors"), global::System.Serializable]
    public class MandatoryFailureException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MandatoryFailureException"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property that failed the mandatory check.</param>
        public MandatoryFailureException(string propertyName) : this(propertyName, string.Empty) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MandatoryFailureException"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property that failed the mandatory check.</param>
        /// <param name="message">Additional message information.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object,System.Object)")]
        public MandatoryFailureException(string propertyName, string message)
            : base(string.Format("The mandatory property \"{0}\" did not have a value.{1}{2}", propertyName, Environment.NewLine, message))
        {
            PropertyName = propertyName;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MandatoryFailureException"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property that failed the mandatory check.</param>
        /// <param name="message">Additional message information.</param>
        /// <param name="innerException">The inner exception.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object,System.Object)")]
        public MandatoryFailureException(string propertyName, string message, Exception innerException)
            : base(string.Format("The mandatory property \"{0}\" did not have a value.{1}{2}", propertyName, Environment.NewLine, message), innerException)
        {
            PropertyName = propertyName;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MandatoryFailureException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="info"/> parameter is null.
        /// </exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">
        /// The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0).
        /// </exception>
        protected MandatoryFailureException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        /// <summary>
        /// Gets or sets the name of the property the exception was raised for.
        /// </summary>
        /// <value>The name of the property.</value>
        public string PropertyName { get; set; }
    }
}
