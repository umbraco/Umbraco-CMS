using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Core.Models.Membership
{
    public interface IUserTypeGroupBase : IAggregateRoot
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
    }
}