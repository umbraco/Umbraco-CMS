using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Models.Entities;

public class UserAccessEntity
{
    public UserAccessEntity(IEntitySlim entity, bool hasAccess)
    {
        Entity = entity;
        HasAccess = hasAccess;
    }

    public IEntitySlim Entity { get; }

    public bool HasAccess { get; }
}
