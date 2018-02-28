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

        /// <summary>
        /// This allows us to inject html into the preview function extending the view with custom data.
        /// </summary>
        public string ExtendPreviewHtml { get; set; }
    }
}
