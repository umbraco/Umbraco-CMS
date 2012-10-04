using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Enum of the various DbTypes for which the Property values are stored
    /// </summary>
    /// <remarks>
    /// Object is added to support complex values from PropertyEditors, 
    /// but will be saved under the Ntext column.
    /// </remarks>
    [Serializable]
    [DataContract(IsReference = true)]
    public enum DataTypeDatabaseType
    {
        Integer,
        Date,
        Nvarchar,
        Ntext /*, Object*/
    }
}