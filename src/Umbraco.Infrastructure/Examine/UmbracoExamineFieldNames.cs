using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

public static class UmbracoExamineFieldNames
{
    /// <summary>
    ///     Used to store the path of a content object
    /// </summary>
    public const string IndexPathFieldName = ExamineFieldNames.SpecialFieldPrefix + "Path";

    public const string NodeKeyFieldName = ExamineFieldNames.SpecialFieldPrefix + "Key";
    public const string UmbracoFileFieldName = "umbracoFileSrc";
    public const string IconFieldName = ExamineFieldNames.SpecialFieldPrefix + "Icon";
    public const string PublishedFieldName = ExamineFieldNames.SpecialFieldPrefix + "Published";

    /// <summary>
    ///     The prefix added to a field when it is duplicated in order to store the original raw value.
    /// </summary>
    public const string RawFieldPrefix = ExamineFieldNames.SpecialFieldPrefix + "Raw_";

    public const string VariesByCultureFieldName = ExamineFieldNames.SpecialFieldPrefix + "VariesByCulture";

    public const string NodeNameFieldName = "nodeName";
    public const string ItemIdFieldName = "__NodeId";
    public const string CategoryFieldName = "__IndexType";
    public const string ItemTypeFieldName = "__NodeTypeAlias";
}
