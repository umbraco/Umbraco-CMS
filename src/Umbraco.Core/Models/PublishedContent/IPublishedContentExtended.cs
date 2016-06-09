namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides methods to handle extended content.
    /// </summary>
    internal interface IPublishedContentExtended : IPublishedContent
    {
        /// <summary>
        /// Adds a property to the extended content.
        /// </summary>
        /// <param name="property">The property to add.</param>
        void AddProperty(IPublishedProperty property);

        /// <summary>
        /// Gets a value indicating whether properties were added to the extended content.
        /// </summary>
        bool HasAddedProperties { get; }
    }
}
