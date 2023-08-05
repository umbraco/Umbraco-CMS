using Umbraco.Cms.Core.Search;

namespace Umbraco.Search.ValueSet.Validators;

public class MemberValueSetValidator : UmbracoValueSetValidator
{
    /// <summary>
    ///     By default these are the member fields we index
    /// </summary>
    public static readonly string[] DefaultMemberIndexFields =
    {
        "id", UmbracoSearchFieldNames.NodeNameFieldName, "updateDate", "loginName", "email",
        UmbracoSearchFieldNames.NodeKeyFieldName,
    };

    private static readonly IEnumerable<string> _validCategories = new[] { IndexTypes.Member };

    public MemberValueSetValidator()
        : base(null, null, DefaultMemberIndexFields, null)
    {
    }

    public MemberValueSetValidator(IEnumerable<string>? includeItemTypes, IEnumerable<string>? excludeItemTypes)
        : base(includeItemTypes, excludeItemTypes, DefaultMemberIndexFields, null)
    {
    }

    public MemberValueSetValidator(IEnumerable<string>? includeItemTypes, IEnumerable<string>? excludeItemTypes, IEnumerable<string>? includeFields, IEnumerable<string>? excludeFields)
        : base(includeItemTypes, excludeItemTypes, includeFields, excludeFields)
    {
    }

    protected override IEnumerable<string> ValidIndexCategories => _validCategories;
}
