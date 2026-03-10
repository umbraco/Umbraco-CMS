using System.Runtime.Serialization;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents the content type that a <see cref="Member" /> object is based on
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class MemberType : ContentTypeCompositionBase, IMemberType
{
    /// <summary>
    ///     Constant indicating that member types do not support publishing.
    /// </summary>
    public const bool SupportsPublishingConst = false;
    private readonly IShortStringHelper _shortStringHelper;

    /// <summary>
    ///     Gets or Sets a Dictionary of Tuples (MemberCanEdit, VisibleOnProfile, IsSensitive) by the PropertyTypes' alias.
    /// </summary>
    private readonly IDictionary<string, MemberTypePropertyProfileAccess> _memberTypePropertyTypes;

    // Dictionary is divided into string: PropertyTypeAlias, Tuple: MemberCanEdit, VisibleOnProfile, PropertyTypeId
    private string _alias = string.Empty;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberType" /> class with the specified parent ID.
    /// </summary>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="parentId">The identifier of the parent content type.</param>
    public MemberType(IShortStringHelper shortStringHelper, int parentId)
        : base(shortStringHelper, parentId)
    {
        _shortStringHelper = shortStringHelper;
        _memberTypePropertyTypes = new Dictionary<string, MemberTypePropertyProfileAccess>();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberType" /> class with a parent content type.
    /// </summary>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="parent">The parent content type composition.</param>
    public MemberType(IShortStringHelper shortStringHelper, IContentTypeComposition parent)
        : this(
        shortStringHelper,
        parent,
        string.Empty)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberType" /> class with a parent content type and alias.
    /// </summary>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="parent">The parent content type composition.</param>
    /// <param name="alias">The alias for the member type.</param>
    public MemberType(IShortStringHelper shortStringHelper, IContentTypeComposition parent, string alias)
        : base(shortStringHelper, parent, alias)
    {
        _shortStringHelper = shortStringHelper;
        _memberTypePropertyTypes = new Dictionary<string, MemberTypePropertyProfileAccess>();
    }

    /// <inheritdoc />
    public override bool SupportsPublishing => SupportsPublishingConst;

    /// <summary>
    ///     Gets or sets the content variation mode. Always returns <see cref="ContentVariation.Nothing"/> for members.
    /// </summary>
    /// <remarks>
    ///     Although technically possible, variations on members don't make much sense
    ///     and therefore are disabled. They are fully supported at service level, though,
    ///     but not at published snapshot level.
    /// </remarks>
    public override ContentVariation Variations
    {
        // note: although technically possible, variations on members don't make much sense
        // and therefore are disabled - they are fully supported at service level, though,
        // but not at published snapshot level.
        get => base.Variations;
        set
        {
            if (value is not ContentVariation.Nothing)
            {
                throw new NotSupportedException("Variations are not supported on members.");
            }

            base.Variations = value;
        }
    }

    /// <inheritdoc />
    public override ISimpleContentType ToSimple() => new SimpleContentType(this);

    /// <summary>
    ///     The Alias of the ContentType
    /// </summary>
    [DataMember]
    public override string Alias
    {
        get => _alias;
        set
        {
            // NOTE: WE are overriding this because we don't want to do a ToSafeAlias when the alias is the special case of
            // "_umbracoSystemDefaultProtectType" which is used internally, currently there is an issue with the safe alias as it strips
            // leading underscores which we don't want in this case.
            // see : http://issues.umbraco.org/issue/U4-3968

            // TODO: BUT, I'm pretty sure we could do this with regards to underscores now:
            // .ToCleanString(CleanStringType.Alias | CleanStringType.UmbracoCase)
            // Need to ask Stephen
            var newVal = value == "_umbracoSystemDefaultProtectType"
                ? value
                : value == null
                    ? string.Empty
                    : value.ToSafeAlias(_shortStringHelper);

            SetPropertyValueAndDetectChanges(newVal, ref _alias!, nameof(Alias));
        }
    }

    /// <summary>
    ///     Gets a boolean indicating whether a Property is editable by the Member.
    /// </summary>
    /// <param name="propertyTypeAlias">PropertyType Alias of the Property to check</param>
    /// <returns></returns>
    public bool MemberCanEditProperty(string? propertyTypeAlias) => propertyTypeAlias is not null &&
                                                                    _memberTypePropertyTypes.TryGetValue(
                                                                        propertyTypeAlias,
                                                                        out MemberTypePropertyProfileAccess? propertyProfile) &&
                                                                    propertyProfile.IsEditable;

    /// <summary>
    ///     Gets a boolean indicating whether a Property is visible on the Members profile.
    /// </summary>
    /// <param name="propertyTypeAlias">PropertyType Alias of the Property to check</param>
    /// <returns></returns>
    public bool MemberCanViewProperty(string propertyTypeAlias) =>
        _memberTypePropertyTypes.TryGetValue(propertyTypeAlias, out MemberTypePropertyProfileAccess? propertyProfile) &&
        propertyProfile.IsVisible;

    /// <summary>
    ///     Gets a boolean indicating whether a Property is marked as storing sensitive values on the Members profile.
    /// </summary>
    /// <param name="propertyTypeAlias">PropertyType Alias of the Property to check</param>
    /// <returns></returns>
    public bool IsSensitiveProperty(string propertyTypeAlias) =>
        _memberTypePropertyTypes.TryGetValue(propertyTypeAlias, out MemberTypePropertyProfileAccess? propertyProfile) &&
        propertyProfile.IsSensitive;

    /// <summary>
    ///     Sets a boolean indicating whether a Property is editable by the Member.
    /// </summary>
    /// <param name="propertyTypeAlias">PropertyType Alias of the Property to set</param>
    /// <param name="value">Boolean value, true or false</param>
    public void SetMemberCanEditProperty(string propertyTypeAlias, bool value)
    {
        if (_memberTypePropertyTypes.TryGetValue(propertyTypeAlias, out MemberTypePropertyProfileAccess? propertyProfile))
        {
            propertyProfile.IsEditable = value;
        }
        else
        {
            var tuple = new MemberTypePropertyProfileAccess(false, value, false);
            _memberTypePropertyTypes.Add(propertyTypeAlias, tuple);
        }
    }

    /// <summary>
    ///     Sets a boolean indicating whether a Property is visible on the Members profile.
    /// </summary>
    /// <param name="propertyTypeAlias">PropertyType Alias of the Property to set</param>
    /// <param name="value">Boolean value, true or false</param>
    public void SetMemberCanViewProperty(string propertyTypeAlias, bool value)
    {
        if (_memberTypePropertyTypes.TryGetValue(propertyTypeAlias, out MemberTypePropertyProfileAccess? propertyProfile))
        {
            propertyProfile.IsVisible = value;
        }
        else
        {
            var tuple = new MemberTypePropertyProfileAccess(value, false, false);
            _memberTypePropertyTypes.Add(propertyTypeAlias, tuple);
        }
    }

    /// <summary>
    ///     Sets a boolean indicating whether a Property is a sensitive value on the Members profile.
    /// </summary>
    /// <param name="propertyTypeAlias">PropertyType Alias of the Property to set</param>
    /// <param name="value">Boolean value, true or false</param>
    public void SetIsSensitiveProperty(string propertyTypeAlias, bool value)
    {
        if (_memberTypePropertyTypes.TryGetValue(
            propertyTypeAlias, out MemberTypePropertyProfileAccess? propertyProfile))
        {
            propertyProfile.IsSensitive = value;
        }
        else
        {
            var tuple = new MemberTypePropertyProfileAccess(false, false, value);
            _memberTypePropertyTypes.Add(propertyTypeAlias, tuple);
        }
    }
}
