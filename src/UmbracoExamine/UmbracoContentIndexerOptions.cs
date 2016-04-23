namespace UmbracoExamine
{
    /// <summary>
    /// Options used to configure the umbraco content indexer
    /// </summary>
    public class UmbracoContentIndexerOptions
    {
        public bool SupportUnpublishedContent { get; private set; }
        public bool SupportProtectedContent { get; private set; }

        public UmbracoContentIndexerOptions(bool supportUnpublishedContent, bool supportProtectedContent)
        {
            SupportUnpublishedContent = supportUnpublishedContent;
            SupportProtectedContent = supportProtectedContent;
        }
    }
}