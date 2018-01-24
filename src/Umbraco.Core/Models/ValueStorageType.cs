using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents the supported database types for storing a value.
    /// </summary>
    [Serializable]
    [DataContract]
    public enum ValueStorageType
    {
        /// <summary>
        /// Store property value as NText.
        /// </summary>
        [EnumMember]
        Ntext,

        /// <summary>
        /// Store property value as NVarChar.
        /// </summary>
        [EnumMember]
        NVarChar,

        /// <summary>
        /// Store property value as Integer.
        /// </summary>
        [EnumMember]
        Integer,

        /// <summary>
        /// Store property value as Date.
        /// </summary>
        [EnumMember]
        Date,

        /// <summary>
        /// Store property value as Decimal.
        /// </summary>
        [EnumMember]
        Decimal
    }
}
