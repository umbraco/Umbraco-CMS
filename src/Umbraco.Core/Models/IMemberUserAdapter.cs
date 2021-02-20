using System.Collections.Generic;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Models
{
    public interface IMemberUserAdapter : IUser
    {
        IEnumerable<string> AllowedSections { get; }
    }
}
