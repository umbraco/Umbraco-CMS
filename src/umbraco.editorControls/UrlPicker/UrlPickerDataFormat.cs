namespace umbraco.editorControls.UrlPicker
{
    /// <summary>
    /// Determines in which serialized format the the data is saved to the database
    /// </summary>
    public enum UrlPickerDataFormat
    {
        /// <summary>
        /// Store as XML
        /// </summary>
        Xml,

        /// <summary>
        /// Store as comma delimited (CSV, single line)
        /// </summary>
        Csv,

        /// <summary>
        /// Store as a JSON object, which can be deserialized by .NET or JavaScript
        /// </summary>
        Json
    }
}
