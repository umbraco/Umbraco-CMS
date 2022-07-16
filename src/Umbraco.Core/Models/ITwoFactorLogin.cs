using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

public interface ITwoFactorLogin : IEntity, IRememberBeingDirty
{
    string ProviderName { get; }

    string Secret { get; }

    Guid UserOrMemberKey { get; }
}
