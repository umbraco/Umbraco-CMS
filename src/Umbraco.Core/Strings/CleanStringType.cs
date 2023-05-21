namespace Umbraco.Cms.Core.Strings;

/// <summary>
///     Specifies the type of a clean string.
/// </summary>
/// <remarks>
///     <para>Specifies its casing, and its encoding.</para>
/// </remarks>
[Flags]
public enum CleanStringType
{
    // note: you have 32 bits at your disposal
    // 0xffffffff

    // no value

    /// <summary>
    ///     No value.
    /// </summary>
    None = 0x00,

    // casing values

    /// <summary>
    ///     Flag mask for casing.
    /// </summary>
    CaseMask = PascalCase | CamelCase | Unchanged | LowerCase | UpperCase | UmbracoCase,

    /// <summary>
    ///     Pascal casing eg "PascalCase".
    /// </summary>
    PascalCase = 0x01,

    /// <summary>
    ///     Camel casing eg "camelCase".
    /// </summary>
    CamelCase = 0x02,

    /// <summary>
    ///     Unchanged casing eg "UncHanGed".
    /// </summary>
    Unchanged = 0x04,

    /// <summary>
    ///     Lower casing eg "lowercase".
    /// </summary>
    LowerCase = 0x08,

    /// <summary>
    ///     Upper casing eg "UPPERCASE".
    /// </summary>
    UpperCase = 0x10,

    /// <summary>
    ///     Umbraco "safe alias" case.
    /// </summary>
    /// <remarks>
    ///     Uppercases the first char of each term except for the first
    ///     char of the string, everything else including the first char of the
    ///     string is unchanged.
    /// </remarks>
    UmbracoCase = 0x20,

    // encoding values

    /// <summary>
    ///     Flag mask for encoding.
    /// </summary>
    CodeMask = Utf8 | Ascii | TryAscii,

    // Unicode encoding is obsolete, use Utf8
    // Unicode = 0x0100,

    /// <summary>
    ///     Utf8 encoding.
    /// </summary>
    Utf8 = 0x0200,

    /// <summary>
    ///     Ascii encoding.
    /// </summary>
    Ascii = 0x0400,

    /// <summary>
    ///     Ascii encoding, if possible.
    /// </summary>
    TryAscii = 0x0800,

    // role values

    /// <summary>
    ///     Flag mask for role.
    /// </summary>
    RoleMask = UrlSegment | Alias | UnderscoreAlias | FileName | ConvertCase,

    /// <summary>
    ///     Url role.
    /// </summary>
    UrlSegment = 0x010000,

    /// <summary>
    ///     Alias role.
    /// </summary>
    Alias = 0x020000,

    /// <summary>
    ///     FileName role.
    /// </summary>
    FileName = 0x040000,

    /// <summary>
    ///     ConvertCase role.
    /// </summary>
    ConvertCase = 0x080000,

    /// <summary>
    ///     UnderscoreAlias role.
    /// </summary>
    /// <remarks>This is Alias + leading underscore.</remarks>
    UnderscoreAlias = 0x100000,
}
