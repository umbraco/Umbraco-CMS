namespace Umbraco.Core.Models
{
    /// <summary>
    /// Used when determining available compositions for a given content type
    /// </summary>
    internal class ContentTypeAvailableCompositionsResult
    {
        public ContentTypeAvailableCompositionsResult(IContentTypeComposition composition, bool allowed)
        {
            Composition = composition;
            Allowed = allowed;
        }

        public IContentTypeComposition Composition { get; private set; }
        public bool Allowed { get; private set; }
    }
}