using System.Collections.Generic;

namespace Umbraco.Core.Models.Membership
{
    public interface IUserGroup : IUserTypeGroupBase
    {
        IEnumerable<string> AllowedSections { get; }

        void RemoveAllowedSection(string sectionAlias);

        void AddAllowedSection(string sectionAlias);
    }
}