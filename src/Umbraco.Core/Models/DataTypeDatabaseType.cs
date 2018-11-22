using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Enum of the various DbTypes for which the Property values are stored
    /// </summary>
    [Serializable]
    [DataContract]
    public enum DataTypeDatabaseType
    {
        [EnumMember]
        Ntext,
        [EnumMember]
        Nvarchar,
        [EnumMember]
        Integer,
        [EnumMember]
        Date,
        [EnumMember]
        Decimal
    }
}