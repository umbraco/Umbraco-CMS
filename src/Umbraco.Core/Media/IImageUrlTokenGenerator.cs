namespace Umbraco.Cms.Core.Media
{
    /// <summary>
    /// Exposes a method that generates an image URL token for request authentication.
    /// </summary>
    public interface IImageUrlTokenGenerator
    {
        /// <summary>
        /// Gets the image URL token for request authentication.
        /// </summary>
        /// <param name="imageUrl">The image URL.</param>
        /// <param name="commands">The commands to include.</param>
        /// <returns>
        /// The generated image URL token.
        /// </returns>
        string? GetImageUrlToken(string imageUrl, IEnumerable<KeyValuePair<string, string?>> commands);
    }
}
