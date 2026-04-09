namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
/// Provides validation logic for member value sets during the Examine indexing process in Umbraco.
/// </summary>
public class MemberValueSetValidator : ValueSetValidator
{
    /// <summary>
    ///     By default these are the member fields we index
    /// </summary>
    public static readonly string[] DefaultMemberIndexFields =
    {
        "id", UmbracoExamineFieldNames.NodeNameFieldName, "updateDate", "loginName", "email",
        UmbracoExamineFieldNames.NodeKeyFieldName,
    };

    private static readonly IEnumerable<string> _validCategories = new[] { IndexTypes.Member };

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberValueSetValidator"/> class, which is used to validate value sets for members in Examine.
    /// </summary>
    public MemberValueSetValidator()
        : base(null, null, DefaultMemberIndexFields, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Examine.MemberValueSetValidator"/> class.
    /// </summary>
    /// <param name="includeItemTypes">An optional collection of item types to include for validation. If <c>null</c>, all item types are included unless excluded by <paramref name="excludeItemTypes"/>.</param>
    /// <param name="excludeItemTypes">An optional collection of item types to exclude from validation. If <c>null</c>, no item types are excluded.</param>
    public MemberValueSetValidator(IEnumerable<string>? includeItemTypes, IEnumerable<string>? excludeItemTypes)
        : base(includeItemTypes, excludeItemTypes, DefaultMemberIndexFields, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Examine.MemberValueSetValidator"/> class.
    /// </summary>
    /// <param name="includeItemTypes">Item types to include in validation, or <c>null</c> to include all.</param>
    /// <param name="excludeItemTypes">Item types to exclude from validation, or <c>null</c> to exclude none.</param>
    /// <param name="includeFields">Fields to include in validation, or <c>null</c> to include all.</param>
    /// <param name="excludeFields">Fields to exclude from validation, or <c>null</c> to exclude none.</param>
    public MemberValueSetValidator(IEnumerable<string>? includeItemTypes, IEnumerable<string>? excludeItemTypes, IEnumerable<string>? includeFields, IEnumerable<string>? excludeFields)
        : base(includeItemTypes, excludeItemTypes, includeFields, excludeFields)
    {
    }

    protected override IEnumerable<string> ValidIndexCategories => _validCategories;
}
