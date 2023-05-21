using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Cms.Infrastructure.Search;

public class UmbracoTreeSearcherFields : IUmbracoTreeSearcherFields
{
    private readonly ISet<string> _backOfficeDocumentFieldsToLoad =
        new HashSet<string> { UmbracoExamineFieldNames.VariesByCultureFieldName };

    private readonly ISet<string> _backOfficeFieldsToLoad = new HashSet<string>
    {
        "id",
        UmbracoExamineFieldNames.ItemIdFieldName,
        UmbracoExamineFieldNames.NodeKeyFieldName,
        "nodeName",
        UmbracoExamineFieldNames.IconFieldName,
        UmbracoExamineFieldNames.CategoryFieldName,
        "parentID",
        UmbracoExamineFieldNames.ItemTypeFieldName,
    };

    private readonly ISet<string> _backOfficeMediaFieldsToLoad =
        new HashSet<string> { UmbracoExamineFieldNames.UmbracoFileFieldName };

    private readonly ISet<string> _backOfficeMembersFieldsToLoad = new HashSet<string> { "email", "loginName" };
    private readonly ILocalizationService _localizationService;

    private readonly IReadOnlyList<string> _backOfficeFields = new List<string>
    {
        "id", UmbracoExamineFieldNames.ItemIdFieldName, UmbracoExamineFieldNames.NodeKeyFieldName,
    };

    private readonly IReadOnlyList<string> _backOfficeMediaFields =
        new List<string> { UmbracoExamineFieldNames.UmbracoFileFieldName };

    private readonly IReadOnlyList<string> _backOfficeMembersFields = new List<string> { "email", "loginName" };

    public UmbracoTreeSearcherFields(ILocalizationService localizationService) =>
        _localizationService = localizationService;

    /// <inheritdoc />
    public virtual IEnumerable<string> GetBackOfficeFields() => _backOfficeFields;

    /// <inheritdoc />
    public virtual IEnumerable<string> GetBackOfficeMembersFields() => _backOfficeMembersFields;

    /// <inheritdoc />
    public virtual IEnumerable<string> GetBackOfficeMediaFields() => _backOfficeMediaFields;

    /// <inheritdoc />
    public virtual IEnumerable<string> GetBackOfficeDocumentFields() => Enumerable.Empty<string>();

    /// <inheritdoc />
    public virtual ISet<string> GetBackOfficeFieldsToLoad() => _backOfficeFieldsToLoad;

    /// <inheritdoc />
    public virtual ISet<string> GetBackOfficeMembersFieldsToLoad() => _backOfficeMembersFieldsToLoad;

    /// <inheritdoc />
    public virtual ISet<string> GetBackOfficeMediaFieldsToLoad() => _backOfficeMediaFieldsToLoad;

    /// <inheritdoc />
    public virtual ISet<string> GetBackOfficeDocumentFieldsToLoad()
    {
        ISet<string> fields = _backOfficeDocumentFieldsToLoad;

        // We need to load all nodeName_* fields but we won't know those up front so need to get
        // all langs (this is cached)
        foreach (var field in _localizationService.GetAllLanguages()
                     .Select(x => "nodeName_" + x.IsoCode.ToLowerInvariant()))
        {
            fields.Add(field);
        }

        return fields;
    }
}
