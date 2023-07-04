namespace Umbraco.Cms.Core.Search;

public class UmbracoSearchFieldNames
{
    public const string ParentID ="parentID";
    public const string SystemPrefix = "__";
    /// <summary>
    ///     Used to store the path of a content object
    /// </summary>
    public const string IndexPathFieldName = $"{SystemPrefix}Path";

    public const string NodeKeyFieldName = $"{SystemPrefix}Key";
    public const string UmbracoFileFieldName = "umbracoFileSrc";
    public const string IconFieldName = $"{SystemPrefix}Icon";
    public const string PublishedFieldName = $"{SystemPrefix}Published";

    /// <summary>
    ///     The prefix added to a field when it is duplicated in order to store the original raw value.
    /// </summary>
    public const string RawFieldPrefix = $"{SystemPrefix}Raw_";

    public const string VariesByCultureFieldName = $"{SystemPrefix}VariesByCulture";

    public const string NodeNameFieldName = "nodeName";
    public const string ItemIdFieldName = "__NodeId";
    public const string CategoryFieldName = "__IndexType";
    public const string ItemTypeFieldName = "__NodeTypeAlias";

    /// <summary>
    ///     Field names specifically used in the Delivery API content index
    /// </summary>
    public static class DeliveryApiContentIndex
    {
        /// <summary>
        ///     The content ID
        /// </summary>
        public const string Id = "id";

        /// <summary>
        ///     The content type ID
        /// </summary>
        public const string ContentTypeId = "contentTypeId";

        /// <summary>
        ///     The content culture
        /// </summary>
        public const string Culture = "culture";

        /// <summary>
        ///     Whether or not the content exists in a published state
        /// </summary>
        public const string Published = "published";
    }
}
