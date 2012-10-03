namespace Umbraco.Core.Models
{
    /// <summary>
    /// Enum of the various DbTypes for which the Property values are stored
    /// </summary>
    /// <remarks>
    /// Object is added to support complex values from PropertyEditors, 
    /// but will be saved under the Ntext column.
    /// </remarks>
    public enum DataTypeDatabaseType
    {
        Integer,
        Date,
        Nvarchar,
        Ntext /*, Object*/
    }
}