using System.Collections.Generic;
using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Defines the interface for a <see cref="User"/>
    /// </summary>
    /// <remarks>Will be left internal until a proper Membership implementation is part of the roadmap</remarks>
    internal interface IUser : IMembershipUser, IProfile
    {
        new object Id { get; set; }

        int SessionTimeout { get; set; }
        int StartContentId { get; set; }
        int StartMediaId { get; set; }
        string Language { get; set; }
        
        //NOTE: I have removed this because it is obsolete in v7 and we are basically removing live editing for now
        //bool DefaultToLiveEditing { get; set; }

        IUserType UserType { get; }
        /// <summary>
        /// The default permissions for the user
        /// </summary>
        /// <remarks>
        /// The default permissions are assigned to the user object based on the user type's default permissions
        /// </remarks> 
        string DefaultPermissions { get; }

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
    }
}