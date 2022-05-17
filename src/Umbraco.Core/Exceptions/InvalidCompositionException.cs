using System.Runtime.Serialization;
using System.Text;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Exceptions;

/// <summary>
///     The exception that is thrown when a composition is invalid.
/// </summary>
/// <seealso cref="System.Exception" />
[Serializable]
public class InvalidCompositionException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidCompositionException" /> class.
    /// </summary>
    public InvalidCompositionException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidCompositionException" /> class.
    /// </summary>
    /// <param name="contentTypeAlias">The content type alias.</param>
    /// <param name="propertyTypeAliases">The property type aliases.</param>
    public InvalidCompositionException(string contentTypeAlias, string[] propertyTypeAliases)
        : this(contentTypeAlias, null, propertyTypeAliases)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidCompositionException" /> class.
    /// </summary>
    /// <param name="contentTypeAlias">The content type alias.</param>
    /// <param name="addedCompositionAlias">The added composition alias.</param>
    /// <param name="propertyTypeAliases">The property type aliases.</param>
    public InvalidCompositionException(string contentTypeAlias, string? addedCompositionAlias, string[] propertyTypeAliases)
        : this(contentTypeAlias, addedCompositionAlias, propertyTypeAliases, new string[0])
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidCompositionException" /> class.
    /// </summary>
    /// <param name="contentTypeAlias">The content type alias.</param>
    /// <param name="addedCompositionAlias">The added composition alias.</param>
    /// <param name="propertyTypeAliases">The property type aliases.</param>
    /// <param name="propertyGroupAliases">The property group aliases.</param>
    public InvalidCompositionException(string contentTypeAlias, string? addedCompositionAlias, string[] propertyTypeAliases, string[] propertyGroupAliases)
        : this(FormatMessage(contentTypeAlias, addedCompositionAlias, propertyTypeAliases, propertyGroupAliases))
    {
        ContentTypeAlias = contentTypeAlias;
        AddedCompositionAlias = addedCompositionAlias;
        PropertyTypeAliases = propertyTypeAliases;
        PropertyGroupAliases = propertyGroupAliases;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidCompositionException" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public InvalidCompositionException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidCompositionException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a null reference (
    ///     <see langword="Nothing" /> in Visual Basic) if no inner exception is specified.
    /// </param>
    public InvalidCompositionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidCompositionException" /> class.
    /// </summary>
    /// <param name="info">
    ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
    ///     data about the exception being thrown.
    /// </param>
    /// <param name="context">
    ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
    ///     information about the source or destination.
    /// </param>
    protected InvalidCompositionException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        ContentTypeAlias = info.GetString(nameof(ContentTypeAlias));
        AddedCompositionAlias = info.GetString(nameof(AddedCompositionAlias));
        PropertyTypeAliases = (string[]?)info.GetValue(nameof(PropertyTypeAliases), typeof(string[]));
        PropertyGroupAliases = (string[]?)info.GetValue(nameof(PropertyGroupAliases), typeof(string[]));
    }

    /// <summary>
    ///     Gets the content type alias.
    /// </summary>
    /// <value>
    ///     The content type alias.
    /// </value>
    public string? ContentTypeAlias { get; }

    /// <summary>
    ///     Gets the added composition alias.
    /// </summary>
    /// <value>
    ///     The added composition alias.
    /// </value>
    public string? AddedCompositionAlias { get; }

    /// <summary>
    ///     Gets the property type aliases.
    /// </summary>
    /// <value>
    ///     The property type aliases.
    /// </value>
    public string[]? PropertyTypeAliases { get; }

    /// <summary>
    ///     Gets the property group aliases.
    /// </summary>
    /// <value>
    ///     The property group aliases.
    /// </value>
    public string[]? PropertyGroupAliases { get; }

    /// <summary>
    ///     When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with
    ///     information about the exception.
    /// </summary>
    /// <param name="info">
    ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
    ///     data about the exception being thrown.
    /// </param>
    /// <param name="context">
    ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
    ///     information about the source or destination.
    /// </param>
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
        info.AddValue(nameof(PropertyGroupAliases), PropertyGroupAliases);

        base.GetObjectData(info, context);
    }

    private static string FormatMessage(string contentTypeAlias, string? addedCompositionAlias, string[] propertyTypeAliases, string[] propertyGroupAliases)
    {
        var sb = new StringBuilder();

        if (addedCompositionAlias.IsNullOrWhiteSpace())
        {
            sb.AppendFormat("Content type with alias '{0}' has an invalid composition.", contentTypeAlias);
        }
        else
        {
            sb.AppendFormat(
                "Content type with alias '{0}' was added as a composition to content type with alias '{1}', but there was a conflict.",
                addedCompositionAlias,
                contentTypeAlias);
        }

        if (propertyTypeAliases.Length > 0)
        {
            sb.AppendFormat(
                " Property types must have a unique alias across all compositions, these aliases are duplicate: {0}.",
                string.Join(", ", propertyTypeAliases));
        }

        if (propertyGroupAliases.Length > 0)
        {
            sb.AppendFormat(
                " Property groups with the same alias must also have the same type across all compositions, these aliases have different types: {0}.",
                string.Join(", ", propertyGroupAliases));
        }

        return sb.ToString();
    }
}
