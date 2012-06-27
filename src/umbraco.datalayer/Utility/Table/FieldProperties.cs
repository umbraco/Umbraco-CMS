using System;
using System.Collections.Generic;
using System.Text;

namespace umbraco.DataLayer.Utility.Table
{
    /// <summary>
    /// Properties for a table field.
    /// </summary>
    [Flags]
    public enum FieldProperties
    {
        /// <summary>
        /// No special properties.
        /// </summary>
        None        = 0x00,

        /// <summary>
        /// The field cannot be null.
        /// </summary>
        NotNull     = 0x01 << 1,

        /// <summary>
        /// The field is an identity field.
        /// </summary>
        Identity    = 0x01 << 2 | NotNull,
    }
}
