using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Enum for the various types of Macros
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    internal enum MacroTypes
    {
        [EnumMember]
        Xslt = 1,
        [EnumMember]
        CustomControl = 2,
        [EnumMember]
        UserControl = 3,
        [EnumMember]
        Unknown = 4,
        [EnumMember]
        Python = 5,
        [EnumMember]
        Script = 6,
        [EnumMember]
        PartialView = 7
    }
}