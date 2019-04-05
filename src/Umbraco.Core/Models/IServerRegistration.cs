using System;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Sync;

namespace Umbraco.Core.Models
{
    public interface IServerRegistration : IServerAddress, IEntity, IRememberBeingDirty
    {
        /// <summary>
        /// Gets or sets the server unique identity.
        /// </summary>
        string ServerIdentity { get; set; }

        new string ServerAddress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the server is active.
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the server is master.
        /// </summary>
        bool IsMaster { get; set; }

        /// <summary>
        /// Gets the date and time the registration was created.
        /// </summary>
        DateTime Registered { get; set; }

        /// <summary>
        /// Gets the date and time the registration was last accessed.
        /// </summary>
        DateTime Accessed { get; set; }

        /// <summary>
        /// Gets or sets the value of the last executed cache instruction.
        /// </summary>
        int LastCacheInstructionId { get; set; }
    }
}
