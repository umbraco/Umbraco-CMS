using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Represents an <see cref="IContent" /> -> user group & permission key value pair collection
/// </summary>
/// <remarks>
///     This implements <see cref="IEntity" /> purely so it can be used with the repository layer which is why it's
///     explicitly implemented.
/// </remarks>
public class ContentPermissionSet : EntityPermissionSet, IEntity
{
    private readonly IContent _content;

    public ContentPermissionSet(IContent content, EntityPermissionCollection permissionsSet)
        : base(content.Id, permissionsSet) =>
        _content = content;

    public override int EntityId => _content.Id;

    int IEntity.Id
    {
        get => EntityId;
        set => throw new NotImplementedException();
    }

    bool IEntity.HasIdentity => EntityId > 0;

    Guid IEntity.Key { get; set; }

    void IEntity.ResetIdentity() =>
        throw new InvalidOperationException($"Resetting identity on {nameof(ContentPermissionSet)} is invalid");

    DateTime IEntity.CreateDate { get; set; }

    DateTime IEntity.UpdateDate { get; set; }

    DateTime? IEntity.DeleteDate { get; set; }

    object IDeepCloneable.DeepClone() => throw new NotImplementedException();
}
