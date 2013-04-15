using System;

namespace Umbraco.Core.Strings
{
    /// <summary>
    /// Specifies the type of a clean string.
    /// </summary>
    /// <remarks>
    /// <para>Specifies its casing, and its encoding.</para>
    /// </remarks>
    [Flags]
    public enum CleanStringType
    {
        // note: you have 32 bits at your disposal
        // 0xffffffff

        /// <summary>
        /// Flag mask for casing.
        /// </summary>
        CaseMask = 0x3f, // 0xff - 8 possible values

        /// <summary>
        /// Flag mask for encoding.
        /// </summary>
        CodeMask = 0x700, // 0xff00 - 8 possible values

        /// <summary>
        /// Flag mask for role.
        /// </summary>
        RoleMask = 0x030000, // 0xff0000 - 8 possible values

        /// <summary>
        /// No value.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Pascal casing eg "PascalCase".
        /// </summary>
        PascalCase = 0x01,

        /// <summary>
        /// Camel casing eg "camelCase".
        /// </summary>
        CamelCase = 0x02,

        /// <summary>
        /// Unchanged casing eg "UncHanGed".
        /// </summary>
        Unchanged = 0x04,

        /// <summary>
        /// Lower casing eg "lowercase".
        /// </summary>
        LowerCase = 0x08,

        /// <summary>
        /// Upper casing eg "UPPERCASE".
        /// </summary>
        UpperCase = 0x10,

        /// <summary>
        /// Umbraco "safe alias" case.
        /// </summary>
        /// <remarks>This is for backward compatibility. Casing is unchanged within terms,
        /// and is pascal otherwise.</remarks>
        UmbracoCase = 0x20,

        /// <summary>
        /// Unicode encoding.
        /// </summary>
        Unicode = 0x0100,

        /// <summary>
        /// Utf8 encoding.
        /// </summary>
        Utf8 = 0x0200,

        /// <summary>
        /// Ascii encoding.
        /// </summary>
        Ascii = 0x0400,

        /// <summary>
        ///  Url role.
        /// </summary>
        Url = 0x010000,

        /// <summary>
        /// Alias role.
        /// </summary>
        Alias = 0x020000
    }
}
