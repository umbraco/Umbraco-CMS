using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Enum for the various types of Macros
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public enum MacroTypes
    {
        Xslt = 1,
        CustomControl = 2,
        UserControl = 3,
        Unknown = 4,
        Python = 5,
        Script = 6
    }
}