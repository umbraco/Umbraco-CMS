using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents an Umbraco Member
    /// </summary>
    /// <remarks>
    /// Should be internal until a proper user/membership implementation
    /// is part of the roadmap.
    /// </remarks>
    [Serializable]
    [DataContract(IsReference = true)]
    internal class Member : MemberProfile, IMembershipUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordQuestion { get; set; }
        public string PasswordAnswer { get; set; }
        public string Comments { get; set; }
        public bool IsApproved { get; set; }
        public bool IsOnline { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime LastPasswordChangeDate { get; set; }
        public DateTime LastLockoutDate { get; set; }
        public object ProfileId { get; set; }
        public IEnumerable<object> Groups { get; set; }
        public Guid Key { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public bool HasIdentity { get; private set; }
    }
}