namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents an uploaded file for a particular property
    /// </summary>
    public class ContentItemFile
    {
        /// <summary>
        /// The property alias associated with the file
        /// </summary>
        public string PropertyAlias { get; set; }

        /// <summary>
        /// The original file name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The file path for the uploaded file for where the MultipartFormDataStreamProvider has saved the temp file
        /// </summary>
        public string TempFilePath { get; set; }
    }
}