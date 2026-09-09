namespace Umbraco.Cms.Search.Core;

/// <summary>
/// Constants used by Umbraco Search.
/// </summary>
public static class Constants
{
    /// <summary>
    /// API-related constants.
    /// </summary>
    public static class Api
    {
        /// <summary>
        /// The API name used to map the Search Management API endpoints.
        /// </summary>
        public const string Name = "search";
    }

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
