using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Core.Models.Membership
{

    public interface IUserType : IAggregateRoot
    {
        /// <summary>
        /// The user type alias
        /// </summary>
        string Alias { get; set; }

        /// <summary>
        /// The user type name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The set of default permissions for the user type
        /// </summary>
        /// <remarks>
        /// By default each permission is simply a single char but we've made this an enumerable{string} to support a more flexible permissions structure in the future.
        /// </remarks>
        IEnumerable<string> Permissions { get; set; }
    }
}