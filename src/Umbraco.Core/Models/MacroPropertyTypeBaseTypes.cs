using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Enum for the three allowed BaseTypes
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public enum MacroPropertyTypeBaseTypes
    {
        Int32,
        Boolean,
        String
    }
}