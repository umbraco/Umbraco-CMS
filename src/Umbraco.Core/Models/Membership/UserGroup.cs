using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents a Group for a Backoffice User
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    internal class UserGroup : UserTypeGroupBase, IUserGroup
    {
        private List<string> _sectionCollection;

        public UserGroup()
        {
            _sectionCollection = new List<string>();
        }

        public IEnumerable<string> AllowedSections
        {
            get { return _sectionCollection; }
        }

        public void RemoveAllowedSection(string sectionAlias)
        {
            if (_sectionCollection.Contains(sectionAlias))
            {
                _sectionCollection.Remove(sectionAlias);
            }
        }

        public void AddAllowedSection(string sectionAlias)
        {
            if (_sectionCollection.Contains(sectionAlias) == false)
            {
                _sectionCollection.Add(sectionAlias);
            }
        }
    }
}