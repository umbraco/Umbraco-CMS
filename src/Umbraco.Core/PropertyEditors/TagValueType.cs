namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Determines how the value for tags is extracted from the property
    /// </summary>
    public enum TagValueType
    {
        /// <summary>
        /// The list of tags will be extracted from the string value by a delimiter
        /// </summary>
        FromDelimitedValue,

        /// <summary>
        /// The list of tags will be supplied by the property editor's ConvertEditorToDb method result which will need to return an IEnumerable{string} value
        /// </summary>        
        /// <remarks>
        /// if the ConvertEditorToDb doesn't return an IEnumerable{string} then an exception will be thrown.
        /// </remarks>
        CustomTagList
    }
}