namespace Umbraco.Cms.Core.Search;

public class UmbracoSearchFieldNames
{
    public const string ParentID ="parentID";

    /// <summary>
    ///     Used to store the path of a content object
    /// </summary>
    public const string IndexPathFieldName = "{prefixTemplate}Path";

    public const string NodeKeyFieldName = "{prefixTemplate}Key";
    public const string UmbracoFileFieldName = "umbracoFileSrc";
    public const string IconFieldName = "{prefixTemplate}Icon";
    public const string PublishedFieldName = "{prefixTemplate}Published";

    /// <summary>
    ///     The prefix added to a field when it is duplicated in order to store the original raw value.
    /// </summary>
    public const string RawFieldPrefix = "{prefixTemplate}Raw_";

    public const string VariesByCultureFieldName = "{prefixTemplate}VariesByCulture";

    public const string NodeNameFieldName = "nodeName";
    public const string ItemIdFieldName = "__NodeId";
    public const string CategoryFieldName = "__IndexType";
    public const string ItemTypeFieldName = "__NodeTypeAlias";
}
