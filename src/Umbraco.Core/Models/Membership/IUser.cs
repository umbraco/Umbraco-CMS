using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Mappers;

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
        /// Gets/sets the user type for the user
        /// </summary>
        IUserType UserType { get; set; }

        //TODO: This should be a private set 
        /// <summary>
        /// The default permission set for the user
        /// </summary>
        /// <remarks>
        /// Currently in umbraco each permission is a single char but with an Enumerable{string} collection this allows for flexible changes to this in the future
        /// </remarks>
        IEnumerable<string> DefaultPermissions { get; set; }

        IEnumerable<string> AllowedSections { get; }
        void RemoveAllowedSection(string sectionAlias);
        void AddAllowedSection(string sectionAlias);

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