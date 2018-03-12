namespace Umbraco.Web.Features
{
    /// <summary>
    /// Represents enabled features.
    /// </summary>
    internal class EnabledFeatures
    {
        /// <summary>
        /// Specifies if rendering pipeline should ignore HasTemplate check when handling a request.
        /// <remarks>This is to allow JSON preview of content with no template set.</remarks>
        /// </summary>
        public bool RenderNoTemplate { get; set; }
    }
}
