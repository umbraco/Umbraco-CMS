using System.ComponentModel;

namespace Umbraco.Cms.Core;

/// <summary>
///     Represents a string-based entity identifier.
/// </summary>
[TypeConverter(typeof(UdiTypeConverter))]
public class StringUdi : Udi
{
    /// <summary>
    ///     Initializes a new instance of the StringUdi class with an entity type and a string id.
    /// </summary>
    /// <param name="entityType">The entity type part of the udi.</param>
    /// <param name="id">The string id part of the udi.</param>
    public StringUdi(string entityType, string id)
        : base(entityType, Constants.Conventions.Udi.Prefix + entityType + "/" + EscapeUriString(id)) =>
        Id = id;

    /// <summary>
    ///     Initializes a new instance of the StringUdi class with a uri value.
    /// </summary>
    /// <param name="uriValue">The uri value of the udi.</param>
    public StringUdi(Uri uriValue)
        : base(uriValue) =>
        Id = Uri.UnescapeDataString(uriValue.AbsolutePath.TrimStart(Constants.CharArrays.ForwardSlash));

    /// <summary>
    ///     The string part of the identifier.
    /// </summary>
    public string Id { get; }

    /// <inheritdoc />
    public override bool IsRoot => Id == string.Empty;

    public StringUdi EnsureClosed()
    {
        EnsureNotRoot();
        return this;
    }

    private static string EscapeUriString(string s) =>

        // Uri.EscapeUriString preserves / but also [ and ] which is bad
        // Uri.EscapeDataString does not preserve / which is bad
        // reserved = : / ? # [ ] @ ! $ & ' ( ) * + , ; =
        // unreserved = alpha digit - . _ ~
        // we want to preserve the / and the unreserved
        // so...
        string.Join("/", s.Split(Constants.CharArrays.ForwardSlash).Select(Uri.EscapeDataString));
}
