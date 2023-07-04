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

    public class DeliveryApiContentIndex
    {
        public const string Id ="parentID";
        public const string ContentTypeId ="parentID";
        public const string Culture ="parentID";

    }
}
