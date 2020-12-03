using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models.Identity
{
    public interface IIdentityUser<TKey>
    {
        int AccessFailedCount { get; set; }
        //ICollection<TClaim> Claims { get; }
        string Email { get; set; }
        bool EmailConfirmed { get; set; }
        TKey Id { get; set; }
        DateTime? LastLoginDateUtc { get; set; }
        DateTime? LastPasswordChangeDateUtc { get; set; }
        bool LockoutEnabled { get; set; }
        DateTime? LockoutEndDateUtc { get; set; }
        //ICollection<TLogin> Logins { get; }
        string PasswordHash { get; set; }
        string PhoneNumber { get; set; }
        bool PhoneNumberConfirmed { get; set; }
        //ICollection<TRole> Roles { get; }
        string SecurityStamp { get; set; }
        bool TwoFactorEnabled { get; set; }
        string UserName { get; set; }
    }
}
