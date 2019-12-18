using System;

namespace Umbraco.Core.Models.Membership
{
    internal class UserProfile : IProfile, IEquatable<UserProfile>
    {
        public UserProfile(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; private set; }
        public string Name { get; private set; }

        public bool Equals(UserProfile other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((UserProfile) obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public static bool operator ==(UserProfile left, UserProfile right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(UserProfile left, UserProfile right)
        {
            return Equals(left, right) == false;
        }
    }
}
