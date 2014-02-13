﻿using System;
using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models.Membership
{
    public interface IMembershipUser : IAggregateRoot
    {        
        object ProviderUserKey { get; set; }
        string Username { get; set; }
        string Email { get; set; }
        string Password { get; set; }
        string PasswordQuestion { get; set; }
        string PasswordAnswer { get; set; }
        string Comments { get; set; }
        bool IsApproved { get; set; }
        bool IsLockedOut { get; set; }
        DateTime LastLoginDate { get; set; }
        DateTime LastPasswordChangeDate { get; set; }
        DateTime LastLockoutDate { get; set; }

        /// <summary>
        /// Gets or sets the number of failed password attempts.
        /// This is the number of times the password was entered incorrectly upon login.
        /// </summary>
        /// <remarks>
        /// Alias: umbracoFailedPasswordAttemptsPropertyTypeAlias
        /// Part of the standard properties collection.
        /// </remarks>
        int FailedPasswordAttempts { get; set; }

        //object ProfileId { get; set; }
        //IEnumerable<object> Groups { get; set; }
    }
}