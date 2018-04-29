namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Represents the current variation.
    /// </summary>
    public class CurrentVariation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentVariation"/> class.
        /// </summary>
        public CurrentVariation(string culture = null, string segment = null)
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
