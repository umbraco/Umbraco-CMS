namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Gives access to the current <see cref="PublishedContent.CurrentVariation"/>.
    /// </summary>
    public interface ICurrentVariationAccessor
    {
        /// <summary>
        /// Gets or sets the current <see cref="PublishedContent.CurrentVariation"/>.
        /// </summary>
        CurrentVariation CurrentVariation { get; set; }
    }
}
