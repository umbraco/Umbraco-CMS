namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Building
{
    /// <summary>
    /// Defines the interface for a generator that creates strongly-typed models for Umbraco content types within the CMS infrastructure.
    /// </summary>
    public interface IModelsGenerator
    {
        /// <summary>
        /// Generates strongly-typed models based on the current content types or schema.
        /// Implementations should create or update code files representing the data models used by Umbraco.
        /// </summary>
        void GenerateModels();
    }
}
