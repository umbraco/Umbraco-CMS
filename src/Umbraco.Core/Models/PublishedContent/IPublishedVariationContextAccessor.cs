namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Gives access to the current <see cref="PublishedVariationContext"/>.
    /// </summary>
    public interface IPublishedVariationContextAccessor
    {
        /// <summary>
        /// Gets or sets the current <see cref="PublishedVariationContext"/>.
        /// </summary>
        PublishedVariationContext Context { get; set; }
    }
}
