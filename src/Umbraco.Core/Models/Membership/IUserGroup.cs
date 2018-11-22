using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models.Membership
{
    public interface IUserGroup : IAggregateRoot, IRememberBeingDirty, ICanBeDirty
    {
        string Alias { get; set; }

        int? StartContentId { get; set; }
        int? StartMediaId { get; set; }

        /// <summary>
        /// The icon
        /// </summary>
        string Icon { get; set; }
        
        /// <summary>
        /// The name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The set of default permissions
        /// </summary>
        /// <remarks>
        /// By default each permission is simply a single char but we've made this an enumerable{string} to support a more flexible permissions structure in the future.
        /// </remarks>
        IEnumerable<string> Permissions { get; set; }

        IEnumerable<string> AllowedSections { get; }

        void RemoveAllowedSection(string sectionAlias);

        void AddAllowedSection(string sectionAlias);

        void ClearAllowedSections();

        /// <summary>
        /// Specifies the number of users assigned to this group
        /// </summary>
        int UserCount { get; }
    }
}
