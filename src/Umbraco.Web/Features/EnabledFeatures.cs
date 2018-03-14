namespace Umbraco.Web.Features
{
    /// <summary>
    /// Represents enabled features.
    /// </summary>
    internal class EnabledFeatures
    {
        
        /// <summary>
        /// This allows us to inject html into the preview function extending the view with custom data.
        /// </summary>
        public string ExtendPreviewHtml { get; set; }
    }
}
