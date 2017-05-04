using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Defines the interface for a <see cref="User"/>
    /// </summary>
    /// <remarks>Will be left internal until a proper Membership implementation is part of the roadmap</remarks>
    public interface IUser : IMembershipUser, IRememberBeingDirty, ICanBeDirty
    {
        string Name { get; set; }
        int SessionTimeout { get; set; }
        int StartContentId { get; set; }
        int StartMediaId { get; set; }
        string Language { get; set; }

        /// <summary>
        /// Gets the groups that user is part of
        /// </summary>
        IEnumerable<IUserGroup> Groups { get; }

        /// <summary>
        /// Indicates if the groups for a user have been loaded
        /// </summary>
        bool GroupsLoaded { get; }

        void RemoveGroup(IUserGroup group);

        void AddGroup(IUserGroup group);

        void SetGroupsLoaded();

        IEnumerable<string> AllowedSections { get; }

        /// <summary>
        /// Exposes the basic profile data
        /// </summary>
        IProfile ProfileData { get; }

        /// <summary>
        /// The security stamp used by ASP.Net identity
        /// </summary>
        string SecurityStamp { get; set; }
    }
}