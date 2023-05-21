using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents the supported database types for storing a value.
/// </summary>
[Serializable]
[DataContract]
public enum ValueStorageType
{
    // note: these values are written out in the database in some places,
    // and then parsed back in a case-sensitive way - think about it before
    // changing the casing of values.

    /// <summary>
    ///     Store property value as NText.
    /// </summary>
    [EnumMember]
    Ntext,

    /// <summary>
    ///     Store property value as NVarChar.
    /// </summary>
    [EnumMember]
    Nvarchar,

    /// <summary>
    ///     Store property value as Integer.
    /// </summary>
    [EnumMember]
    Integer,

    /// <summary>
    ///     Store property value as Date.
    /// </summary>
    [EnumMember]
    Date,

    /// <summary>
    ///     Store property value as Decimal.
    /// </summary>
    [EnumMember]
    Decimal,
}
