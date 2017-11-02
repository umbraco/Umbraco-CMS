using System;
using System.Collections.Generic;
using System.ComponentModel;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Defines the interface for a <see cref="User"/>
    /// </summary>
    /// <remarks>Will be left internal until a proper Membership implementation is part of the roadmap</remarks>
    public interface IUser : IMembershipUser, IRememberBeingDirty, ICanBeDirty
    {
        UserState UserState { get; }

        string Name { get; set; }
        int SessionTimeout { get; set; }

        [Obsolete("This should not be used it exists for legacy reasons only, use user groups instead, it will be removed in future versions")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        int StartContentId { get; set; }

        int[] StartContentIds { get; set; }

        [Obsolete("This should not be used it exists for legacy reasons only, use user groups instead, it will be removed in future versions")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        int StartMediaId { get; set; }

        int[] StartMediaIds { get; set; }

        string Language { get; set; }

        [Obsolete("This should not be used it exists for legacy reasons only, use user groups instead, it will be removed in future versions")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IUserType UserType { get; set; }

        DateTime? EmailConfirmedDate { get; set; }
        DateTime? InvitedDate { get; set; }

        /// <summary>
        /// Gets the groups that user is part of
        /// </summary>
        IEnumerable<IReadOnlyUserGroup> Groups { get; }        

        void RemoveGroup(string group);
        void ClearGroups();
        void AddGroup(IReadOnlyUserGroup group);
        
        IEnumerable<string> AllowedSections { get; }

        [Obsolete("This should not be used it exists for legacy reasons only, use user groups instead, it will be removed in future versions")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void RemoveAllowedSection(string sectionAlias);

        [Obsolete("This should not be used it exists for legacy reasons only, use user groups instead, it will be removed in future versions")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void AddAllowedSection(string sectionAlias);

        /// <summary>
        /// Exposes the basic profile data
        /// </summary>
        IProfile ProfileData { get; }

        /// <summary>
        /// The security stamp used by ASP.Net identity
        /// </summary>
        string SecurityStamp { get; set; }

        /// <summary>
        /// Will hold the media file system relative path of the users custom avatar if they uploaded one
        /// </summary>
        string Avatar { get; set; }

        /// <summary>
        /// A Json blob stored for recording tour data for a user
        /// </summary>
        string TourData { get; set; }
    }
}