namespace Umbraco.Core.Models.Editors
{
    /// <summary>
    /// Represents an uploaded file for a property.
    /// </summary>
    public class ContentPropertyFile
    {
        /// <summary>
        /// Gets or sets the property alias.
        /// </summary>
        public string PropertyAlias { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the temporary path where the file has been uploaded.
        /// </summary>
        public string TempFilePath { get; set; }
    }
}
