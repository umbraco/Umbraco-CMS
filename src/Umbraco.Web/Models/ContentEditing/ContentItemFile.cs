namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents an uploaded file for a particular property
    /// </summary>
    public class ContentItemFile
    {
        /// <summary>
        /// The property id associated with the file
        /// </summary>
        public int PropertyId { get; set; }

        /// <summary>
        /// The file path for the uploaded file for where the MultipartFormDataStreamProvider has saved the temp file
        /// </summary>
        public string FilePath { get; set; }
    }
}