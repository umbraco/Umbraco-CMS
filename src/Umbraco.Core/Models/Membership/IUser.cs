using System.Collections.Generic;
using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Defines the interface for a <see cref="User"/>
    /// </summary>
    /// <remarks>Will be left internal until a proper Membership implementation is part of the roadmap</remarks>
    internal interface IUser : IMembershipUser
    {
        string Name { get; set; }
        int SessionTimeout { get; set; }
        int StartContentId { get; set; }
        int StartMediaId { get; set; }
        string Language { get; set; }
        bool DefaultToLiveEditing { get; set; }
        IEnumerable<string> Applications { get; set; }
        bool NoConsole { get; set; }
        IUserType UserType { get; }
        string Permissions { get; set; }

        IEnumerable<UserSection> UserSections { get; }
        void RemoveUserSection(string sectionAlias);
        void AddUserSection(string sectionAlias);
    }
}