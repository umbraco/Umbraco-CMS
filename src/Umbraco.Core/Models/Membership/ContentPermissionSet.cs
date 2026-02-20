using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Represents an <see cref="IContent" /> -> user group &amp; permission key value pair collection
/// </summary>
/// <remarks>
///     This implements <see cref="IEntity" /> purely so it can be used with the repository layer which is why it's
///     explicitly implemented.
/// </remarks>
public class ContentPermissionSet : EntityPermissionSet, IEntity
{
    private readonly IContent _content;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPermissionSet" /> class.
    /// </summary>
    /// <param name="content">The content item associated with the permission set.</param>
    /// <param name="permissionsSet">The collection of entity permissions.</param>
    public ContentPermissionSet(IContent content, EntityPermissionCollection permissionsSet)
        : base(content.Id, permissionsSet) =>
        _content = content;

    /// <inheritdoc />
    public override int EntityId => _content.Id;

    /// <inheritdoc />
    int IEntity.Id
    {
        get => EntityId;
        set => throw new NotImplementedException();
    }

    /// <inheritdoc />
    bool IEntity.HasIdentity => EntityId > 0;

    /// <inheritdoc />
    Guid IEntity.Key { get; set; }

    /// <inheritdoc />
    void IEntity.ResetIdentity() =>
        throw new InvalidOperationException($"Resetting identity on {nameof(ContentPermissionSet)} is invalid");

    /// <inheritdoc />
    DateTime IEntity.CreateDate { get; set; }

    /// <inheritdoc />
    DateTime IEntity.UpdateDate { get; set; }

    /// <inheritdoc />
    DateTime? IEntity.DeleteDate { get; set; }

    /// <inheritdoc />
    object IDeepCloneable.DeepClone() => throw new NotImplementedException();
}
