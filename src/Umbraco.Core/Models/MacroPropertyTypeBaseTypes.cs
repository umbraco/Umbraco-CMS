using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Enum for the three allowed BaseTypes
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    internal enum MacroPropertyTypeBaseTypes
    {
        [EnumMember]
        Int32,
        [EnumMember]
        Boolean,
        [EnumMember]
        String
    }
}