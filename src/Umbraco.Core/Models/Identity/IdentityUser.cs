using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models.Identity
{
    /// <summary>
    /// Abstract class for use in Umbraco Identity
    /// </summary>
    /// <typeparam name="TLogin">The type of user login</typeparam>
    /// <typeparam name="TRole">The type of user role</typeparam>
    /// <typeparam name="TClaim">The type of user claims</typeparam>
    /// <remarks>
    /// This class was originally borrowed from the EF implementation in Identity prior to netcore.
    /// The new IdentityUser in netcore does not have properties such as Claims, Roles and Logins and those are instead
    /// by default managed with their default user store backed by EF which utilizes EF's change tracking to track these values
    /// to a user. We will continue using this approach since it works fine for what we need which does the change tracking of
    /// claims, roles and logins directly on the user model.
    /// </remarks>
    public abstract class IdentityUser<TLogin, TRole, TClaim>
        where TLogin : IIdentityUserLogin
        where TRole : IdentityUserRole
        where TClaim : IdentityUserClaim
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityUser{TLogin, TRole, TClaim}"/> class.
        /// </summary>
        protected IdentityUser()
        {
            Claims = new List<TClaim>();
            Roles = new List<TRole>();
            Logins = new List<TLogin>();
        }

        /// <summary>
        /// Gets or sets last login date
        /// </summary>
        public virtual DateTime? LastLoginDateUtc { get; set; }

        /// <summary>
        /// Gets or sets email
        /// </summary>
        public virtual string Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the email is confirmed, default is false
        /// </summary>
        public virtual bool EmailConfirmed { get; set; }

        /// <summary>
        /// Gets or sets the salted/hashed form of the user password
        /// </summary>
        public virtual string PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets a random value that should change whenever a users credentials have changed (password changed, login removed)
        /// </summary>
        public virtual string SecurityStamp { get; set; }

        /// <summary>
        /// Gets or sets a phone Number for the user
        /// </summary>
        /// <remarks>
        /// This is unused until we or an end-user requires this value for 2FA
        /// </remarks>
        public virtual string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether true if the phone number is confirmed, default is false
        /// </summary>
        /// <remarks>
        /// This is unused until we or an end-user requires this value for 2FA
        /// </remarks>
        public virtual bool PhoneNumberConfirmed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is two factor enabled for the user
        /// </summary>
        /// <remarks>
        /// This is unused until we or an end-user requires this value for 2FA
        /// </remarks>
        public virtual bool TwoFactorEnabled { get; set; }

        /// <summary>
        /// Gets or sets dateTime in UTC when lockout ends, any time in the past is considered not locked out.
        /// </summary>
        public virtual DateTime? LockoutEndDateUtc { get; set; }

        /// <summary>
        /// Gets or sets dateTime in UTC when the password was last changed.
        /// </summary>
        public virtual DateTime? LastPasswordChangeDateUtc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is lockout enabled for this user
        /// </summary>
        public virtual bool LockoutEnabled { get; set; }

        /// <summary>
        /// Gets or sets the value to record failures for the purposes of lockout
        /// </summary>
        public virtual int AccessFailedCount { get; set; }

        /// <summary>
        /// Gets the user roles collection
        /// </summary>
        public virtual ICollection<TRole> Roles { get; }

        /// <summary>
        /// Gets navigation the user claims collection
        /// </summary>
        public virtual ICollection<TClaim> Claims { get; }

        /// <summary>
        /// Gets the user logins collection
        /// </summary>
        public virtual ICollection<TLogin> Logins { get; }

        /// <summary>
        /// Gets or sets user ID (Primary Key)
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// Gets or sets user name
        /// </summary>
        public virtual string UserName { get; set; }
    }
}
