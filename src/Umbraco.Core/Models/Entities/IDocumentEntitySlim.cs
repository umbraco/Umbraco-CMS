namespace Umbraco.Core.Models.Entities
{
    /// <summary>
    /// Represents a lightweight document entity, managed by the entity service.
    /// </summary>
    public interface IDocumentEntitySlim : IContentEntitySlim
    {
        /// <summary>
        /// Gets a value indicating whether the document is published.
        /// </summary>
        bool Published { get; }

        /// <summary>
        /// Gets a value indicating whether the document has edited properties.
        /// </summary>
        bool Edited { get; }
    }
}