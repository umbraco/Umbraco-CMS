using System;

namespace UmbracoExamine
{
    /// <summary>
    /// Options used to configure the umbraco content indexer
    /// </summary>
    public class UmbracoContentIndexerOptions
    {
        public bool SupportUnpublishedContent { get; private set; }
        public bool SupportProtectedContent { get; private set; }
        //TODO: We should make this a GUID! But to do that we sort of need to store the 'Path' as a comma list of GUIDs instead of int
        public int? ParentId { get; private set; }

        public UmbracoContentIndexerOptions(bool supportUnpublishedContent, bool supportProtectedContent, int? parentId)
        {
            SupportUnpublishedContent = supportUnpublishedContent;
            SupportProtectedContent = supportProtectedContent;
            ParentId = parentId;
        }
    }
}