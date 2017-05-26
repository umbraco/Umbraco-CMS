using System.Collections.Generic;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// A readonly user group providing basic information
    /// </summary>
    public interface IReadOnlyUserGroup
    {
        int StartContentId { get; }
        int StartMediaId { get; }

        /// <summary>
        /// The alias
        /// </summary>
        string Alias { get; }

        IEnumerable<string> AllowedSections { get; }
    }
}