using System.Runtime.Serialization;
using System.Text;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Exceptions;

/// <summary>
///     The exception that is thrown when a composition is invalid.
/// </summary>
/// <seealso cref="System.Exception" />
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
