namespace Umbraco.Cms.Search.Core;

/// <summary>
/// Constants used by Umbraco Search.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Persistence-related constants.
    /// </summary>
    public static class Persistence
    {
        /// <summary>
        /// The name of the database table storing persisted index documents.
        /// </summary>
        public const string IndexDocumentTableName = Umbraco.Cms.Core.Constants.DatabaseSchema.Tables.IndexDocument;
    }
}
