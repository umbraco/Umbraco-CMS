using System.Runtime.Serialization;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents the content type that a <see cref="Media" /> object is based on
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class MediaType : ContentTypeCompositionBase, IMediaType
{
    public const bool SupportsPublishingConst = false;
    private bool _isCloning;

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
    public override string Alias
    {
        get => base.Alias;
        set
        {
            if (this.IsSystemMediaType() && value != Alias && _isCloning is false)
            {
                throw new InvalidOperationException("Cannot change the alias of a system media type");
            }

            base.Alias = value;
        }
    }

    /// <inheritdoc />
    public new IMediaType DeepCloneWithResetIdentities(string newAlias)
    {
        _isCloning = true;
        var clone = (IMediaType)base.DeepCloneWithResetIdentities(newAlias);
        _isCloning = false;
        return clone;
    }

}
