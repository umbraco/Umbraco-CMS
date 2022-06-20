using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Validation;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "contentVariant", Namespace = "")]
public class ContentVariantSave : IContentProperties<ContentPropertyBasic>
{
    public ContentVariantSave() => Properties = new List<ContentPropertyBasic>();

    [DataMember(Name = "name", IsRequired = true)]
    [RequiredForPersistence(AllowEmptyStrings = false, ErrorMessage = "Required")]
    [MaxLength(255, ErrorMessage = "Name must be less than 255 characters")]
    public string? Name { get; set; }

    /// <summary>
    ///     The culture of this variant, if this is invariant than this is null or empty
    /// </summary>
    [DataMember(Name = "culture")]
    public string? Culture { get; set; }

    /// <summary>
    ///     The segment of this variant, if this is invariant than this is null or empty
    /// </summary>
    [DataMember(Name = "segment")]
    public string? Segment { get; set; }

    /// <summary>
    ///     Indicates if the variant should be updated
    /// </summary>
    /// <remarks>
    ///     If this is false, this variant data will not be updated at all
    /// </remarks>
    [DataMember(Name = "save")]
    public bool Save { get; set; }

    /// <summary>
    ///     Indicates if the variant should be published
    /// </summary>
    /// <remarks>
    ///     This option will have no affect if <see cref="Save" /> is false.
    ///     This is not used to unpublish.
    /// </remarks>
    [DataMember(Name = "publish")]
    public bool Publish { get; set; }

    [DataMember(Name = "expireDate")]
    public DateTime? ExpireDate { get; set; }

    [DataMember(Name = "releaseDate")]
    public DateTime? ReleaseDate { get; set; }

    /// <summary>
    ///     The property DTO object is used to gather all required property data including data type information etc... for use
    ///     with validation - used during inbound model binding
    /// </summary>
    /// <remarks>
    ///     We basically use this object to hydrate all required data from the database into one object so we can validate
    ///     everything we need
    ///     instead of having to look up all the data individually.
    ///     This is not used for outgoing model information.
    /// </remarks>
    [IgnoreDataMember]
    public ContentPropertyCollectionDto? PropertyCollectionDto { get; set; }

    [DataMember(Name = "properties")]
    public IEnumerable<ContentPropertyBasic> Properties { get; set; }
}
