using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models.Membership
{
    public interface IUserGroup : IAggregateRoot
    {
        /// <summary>
        /// The alias
        /// </summary>
        string Alias { get; set; }

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
    }
}