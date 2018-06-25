namespace Umbraco.Web.Features
{
    /// <summary>
    /// Represents enabled features.
    /// </summary>
    internal class EnabledFeatures
    {
        /// <summary>
        /// This allows us to inject a razor view into the Umbraco preview view to extend it
        /// </summary>
        /// <remarks>
        /// This is set to a virtual path of a razor view file
        /// </remarks>
        public string PreviewExtendedView { get; set; }
    }
}
