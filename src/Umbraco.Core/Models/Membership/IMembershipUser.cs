using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models.Membership
{
    internal interface IMembershipUser : IMembershipUserId
    {
        object Id { get; set; }
        string Username { get; set; }
        string Email { get; set; }
        string Password { get; set; }
        string PasswordQuestion { get; set; }
        string PasswordAnswer { get; set; }
        string Comments { get; set; }
        bool IsApproved { get; set; }
        bool IsOnline { get; set; }
        bool IsLockedOut { get; set; }
        DateTime CreationDate { get; set; }
        DateTime LastLoginDate { get; set; }
        DateTime LastActivityDate { get; set; }
        DateTime LastPasswordChangeDate { get; set; }
        DateTime LastLockoutDate { get; set; }

        object ProfileId { get; set; }
        IEnumerable<object> Groups { get; set; }
    }
}