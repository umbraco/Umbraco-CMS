using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Models;

public interface IServerRegistration : IServerAddress, IEntity, IRememberBeingDirty
{
    /// <summary>
    ///     Gets or sets the server unique identity.
    /// </summary>
    string? ServerIdentity { get; set; }

    new string? ServerAddress { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the server is active.
    /// </summary>
    bool IsActive { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the server is has the SchedulingPublisher role.
    /// </summary>
    bool IsSchedulingPublisher { get; set; }

    /// <summary>
    ///     Gets the date and time the registration was created.
    /// </summary>
    DateTime Registered { get; set; }

    /// <summary>
    ///     Gets the date and time the registration was last accessed.
    /// </summary>
    DateTime Accessed { get; set; }
}
