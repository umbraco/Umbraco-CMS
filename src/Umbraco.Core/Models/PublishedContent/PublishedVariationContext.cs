namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Represents the published variation context.
    /// </summary>
    /// <remarks>
    /// <para>The published variation context indicates which variation is the current default variation.</para>
    /// </remarks>
    public class PublishedVariationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedVariationContext"/> class.
        /// </summary>
        public PublishedVariationContext(string culture = null, string segment = null)
        {
            Culture = culture;
            Segment = segment;
        }

        /// <summary>
        /// Gets the culture.
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// Gets the segment.
        /// </summary>
        public string Segment { get; set; }
    }
}