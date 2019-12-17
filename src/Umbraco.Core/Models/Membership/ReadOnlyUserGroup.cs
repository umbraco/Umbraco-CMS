using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Models.Membership
{
    public class ReadOnlyUserGroup : IReadOnlyUserGroup, IEquatable<ReadOnlyUserGroup>
    {
        public ReadOnlyUserGroup(int id, string name, string icon, int? startContentId, int? startMediaId, string @alias,
            IEnumerable<string> allowedSections, IEnumerable<string> permissions)
        {
            Name = name;
            Icon = icon;
            Id = id;
            Alias = alias;
            AllowedSections = allowedSections.ToArray();
            Permissions = permissions.ToArray();

            //Zero is invalid and will be treated as Null
            StartContentId = startContentId == 0 ? null : startContentId;
            StartMediaId = startMediaId == 0 ? null : startMediaId;
        }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Icon { get; private set; }
        public int? StartContentId { get; private set; }
        public int? StartMediaId { get; private set; }
        public string Alias { get; private set; }

        /// <summary>
        /// The set of default permissions
        /// </summary>
        /// <remarks>
        /// By default each permission is simply a single char but we've made this an enumerable{string} to support a more flexible permissions structure in the future.
        /// </remarks>
        public IEnumerable<string> Permissions { get; set; }
        public IEnumerable<string> AllowedSections { get; private set; }

        public bool Equals(ReadOnlyUserGroup other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Alias, other.Alias);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ReadOnlyUserGroup) obj);
        }

        public override int GetHashCode()
        {
            return Alias.GetHashCode();
        }

        public static bool operator ==(ReadOnlyUserGroup left, ReadOnlyUserGroup right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ReadOnlyUserGroup left, ReadOnlyUserGroup right)
        {
            return !Equals(left, right);
        }
    }
}
