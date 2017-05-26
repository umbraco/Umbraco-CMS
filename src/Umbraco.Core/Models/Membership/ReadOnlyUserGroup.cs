using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models.Membership
{
    public class ReadOnlyUserGroup : IReadOnlyUserGroup, IEquatable<ReadOnlyUserGroup>
    {
        public ReadOnlyUserGroup(int startContentId, int startMediaId, string @alias, IEnumerable<string> allowedSections)
        {
            StartContentId = startContentId;
            StartMediaId = startMediaId;
            Alias = alias;
            AllowedSections = allowedSections;
        }

        public int StartContentId { get; private set; }
        public int StartMediaId { get; private set; }
        public string Alias { get; private set; }
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