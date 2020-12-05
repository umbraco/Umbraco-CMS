using System;
using System.Collections.Generic;
using System.ComponentModel;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Members
{
    /// <summary>
    /// An Umbraco member user type
    /// TODO: use of identity classes in future
    /// </summary>
    public class UmbracoMembersIdentityUser : IRememberBeingDirty
    {
        private int _id;

        private string _passwordHash;

        private string _passwordConfig;

        /// <summary>
        /// Gets or sets the member name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the member email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the member username
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the alias of the member type
        /// </summary>
        public string MemberTypeAlias { get; set; }

        /// <summary>
        ///  Gets or sets a value indicating whether the member is locked out
        /// </summary>
        public bool IsLockedOut { get; set; }

        /// <summary>
        /// Gets a value indicating whether an Id has been set on this object
        /// This will be false if the object is new and not persisted to the database
        /// </summary>
        public bool HasIdentity { get; private set; }

        /// <summary>
        /// Gets or sets the member Id
        /// </summary>
        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                HasIdentity = true;
            }
        }

        /// <summary>
        /// Gets or sets the salted/hashed form of the user password
        /// </summary>
        public string PasswordHash
        {
            get => _passwordHash;
            set => BeingDirty.SetPropertyValueAndDetectChanges(value, ref _passwordHash, nameof(PasswordHash));
        }

        /// <summary>
        /// Gets or sets the password config
        /// </summary>
        public string PasswordConfig
        {
            get => _passwordConfig;
            set => BeingDirty.SetPropertyValueAndDetectChanges(value, ref _passwordConfig, nameof(PasswordConfig));
        }

        /// <summary>
        /// Gets or sets a value indicating whether member Is Approved
        /// </summary>
        public bool IsApproved { get; set; }

        /// <summary>
        /// Gets the <see cref="BeingDirty"/> for change tracking
        /// </summary>
        protected BeingDirty BeingDirty { get; } = new BeingDirty();

        // TODO: implement as per base identity user
        //public bool LoginsChanged;
        //public bool RolesChanged;

        /// <summary>
        /// Create a new identity member
        /// </summary>
        /// <param name="username">The member username</param>
        /// <param name="email">The member email</param>
        /// <param name="memberTypeAlias">The member type alias</param>
        /// <param name="name">The member name</param>
        /// TODO: confirm <param name="password">The password (may be null in some instances)</param>
        /// <exception cref="ArgumentException">Throws is username is null or whitespace</exception>
        /// <returns>The identity member user</returns>
        public static UmbracoMembersIdentityUser CreateNew(
            string username,
            string email,
            string memberTypeAlias,
            string name,
            string password = null)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(username));
            }

            // no groups/roles yet
            var member = new UmbracoMembersIdentityUser
            {
                UserName = username,
                Email = email,
                Name = name,
                MemberTypeAlias = memberTypeAlias,
                Id = 0,  // TODO: is this meant to be 0 in this circumstance?
                // false by default unless specifically set
                HasIdentity = false
            };

            member.EnableChangeTracking();
            return member;
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged
        {
            add => BeingDirty.PropertyChanged += value;

            remove => BeingDirty.PropertyChanged -= value;
        }

        /// <inheritdoc />

        public bool IsDirty() => BeingDirty.IsDirty();

        /// <inheritdoc />
        public bool IsPropertyDirty(string propName) => BeingDirty.IsPropertyDirty(propName);

        /// <inheritdoc />
        public IEnumerable<string> GetDirtyProperties() => BeingDirty.GetDirtyProperties();

        /// <inheritdoc />

        public void ResetDirtyProperties() => BeingDirty.ResetDirtyProperties();

        /// <inheritdoc />

        public void DisableChangeTracking() => BeingDirty.DisableChangeTracking();

        /// <inheritdoc />

        public void EnableChangeTracking() => BeingDirty.EnableChangeTracking();

        /// <inheritdoc />
        public bool WasDirty() => BeingDirty.WasDirty();

        /// <inheritdoc />
        public bool WasPropertyDirty(string propertyName) => BeingDirty.WasPropertyDirty(propertyName);

        /// <inheritdoc />
        public void ResetWereDirtyProperties() => BeingDirty.ResetWereDirtyProperties();

        /// <inheritdoc />
        public void ResetDirtyProperties(bool rememberDirty) => BeingDirty.ResetDirtyProperties(rememberDirty);

        /// <inheritdoc />
        public IEnumerable<string> GetWereDirtyProperties() => BeingDirty.GetWereDirtyProperties();
    }
}
