using System;
using System.Collections.Generic;
using System.ComponentModel;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Models
{
    public class MemberUserAdapter : IMemberUserAdapter
    {
        private readonly IMember _member;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberUserAdapter"/> class.
        /// Member adaptor to use existing user change password functionality and other shared functions
        /// </summary>
        /// <param name="member">The member to adapt</param>
        public MemberUserAdapter(IMember member)
        {
            _member = member;
        }

        // This is the only reason we currently need this adaptor
        public IEnumerable<string> AllowedSections => new List<string>()
        {
            Constants.Applications.Users
        };


        public UserState UserState { get; }
        public string Name { get; set; }
        public int SessionTimeout { get; set; }
        public int[] StartContentIds { get; set; }
        public int[] StartMediaIds { get; set; }
        public string Language { get; set; }
        public DateTime? InvitedDate { get; set; }
        public IEnumerable<IReadOnlyUserGroup> Groups { get; }
        public void RemoveGroup(string @group) => throw new NotImplementedException();

        public void ClearGroups() => throw new NotImplementedException();

        public void AddGroup(IReadOnlyUserGroup @group) => throw new NotImplementedException();

        public IProfile ProfileData { get; }
        public string Avatar { get; set; }
        public string TourData { get; set; }
        public T FromUserCache<T>(string cacheKey) where T : class => throw new NotImplementedException();

        public void ToUserCache<T>(string cacheKey, T vals) where T : class => throw new NotImplementedException();

        public object DeepClone() => throw new NotImplementedException();

        public int Id { get; set; }
        public Guid Key { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public bool HasIdentity { get; }
        public void ResetIdentity() => throw new NotImplementedException();

        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime? EmailConfirmedDate { get; set; }
        public string RawPasswordValue { get; set; }
        public string PasswordConfiguration { get; set; }
        public string Comments { get; set; }
        public bool IsApproved { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime LastPasswordChangeDate { get; set; }
        public DateTime LastLockoutDate { get; set; }
        public int FailedPasswordAttempts { get; set; }
        public string SecurityStamp { get; set; }
        public bool IsDirty() => throw new NotImplementedException();

        public bool IsPropertyDirty(string propName) => throw new NotImplementedException();

        public IEnumerable<string> GetDirtyProperties() => throw new NotImplementedException();

        public void ResetDirtyProperties() => throw new NotImplementedException();

        public void DisableChangeTracking() => throw new NotImplementedException();

        public void EnableChangeTracking() => throw new NotImplementedException();

        public event PropertyChangedEventHandler PropertyChanged;
        public bool WasDirty() => throw new NotImplementedException();

        public bool WasPropertyDirty(string propertyName) => throw new NotImplementedException();

        public void ResetWereDirtyProperties() => throw new NotImplementedException();

        public void ResetDirtyProperties(bool rememberDirty) => throw new NotImplementedException();

        public IEnumerable<string> GetWereDirtyProperties() => throw new NotImplementedException();
    }
}
