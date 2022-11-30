using System.Runtime.Serialization;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents the content type that a <see cref="Media" /> object is based on
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class MediaType : ContentTypeCompositionBase, IMediaType
{
    public const bool SupportsPublishingConst = false;

    /// <summary>
    ///     Constuctor for creating a MediaType with the parent's id.
    /// </summary>
    /// <remarks>Only use this for creating MediaTypes at the root (with ParentId -1).</remarks>
    public MediaType(IShortStringHelper shortStringHelper, int parentId)
        : base(shortStringHelper, parentId)
    {
    }

    /// <summary>
    ///     Constuctor for creating a MediaType with the parent as an inherited type.
    /// </summary>
    /// <remarks>Use this to ensure inheritance from parent.</remarks>
    public MediaType(IShortStringHelper shortStringHelper, IMediaType parent)
        : this(shortStringHelper, parent, string.Empty)
    {
    }

    /// <summary>
    ///     Constuctor for creating a MediaType with the parent as an inherited type.
    /// </summary>
    /// <remarks>Use this to ensure inheritance from parent.</remarks>
    public MediaType(IShortStringHelper shortStringHelper, IMediaType parent, string alias)
        : base(shortStringHelper, parent, alias)
    {
    }

    /// <inheritdoc />
    public override bool SupportsPublishing => SupportsPublishingConst;

    /// <inheritdoc />
    public override ISimpleContentType ToSimple() => new SimpleContentType(this);

    /// <inheritdoc />
    IMediaType IMediaType.DeepCloneWithResetIdentities(string newAlias) =>
        (IMediaType)DeepCloneWithResetIdentities(newAlias);
}
