using System.Collections.Generic;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// A readonly user group providing basic information
    /// </summary>
    public interface IReadOnlyUserGroup
    {
        string Name { get; }
        string Icon { get; }
        int Id { get; }
        int? StartContentId { get; }
        int? StartMediaId { get; }

        /// <summary>
        /// The alias
        /// </summary>
        string Alias { get; }

        /// <summary>
        /// The set of default permissions
        /// </summary>
        /// <remarks>
        /// By default each permission is simply a single char but we've made this an enumerable{string} to support a more flexible permissions structure in the future.
        /// </remarks>
        IEnumerable<string> Permissions { get; set; }

        IEnumerable<string> AllowedSections { get; }
    }
}
