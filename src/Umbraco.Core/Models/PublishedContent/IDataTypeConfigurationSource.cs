namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides data type configurations.
    /// </summary>
    public interface IDataTypeConfigurationSource
    {
        /// <summary>
        /// Gets a data type configuration.
        /// </summary>
        /// <param name="editorAlias">The property editor alias.</param>
        /// <param name="id">The data type identifier.</param>
        /// <returns>The data type configuration, as a strongly typed object if the property editor
        /// supports it, otherwise as a Dictionary{string, string}.</returns>
        object GetDataTypeConfiguration(string editorAlias, int id);
    }
}
