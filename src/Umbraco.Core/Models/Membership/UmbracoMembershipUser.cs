using System;
using System.Web.Security;

namespace Umbraco.Core.Models.Membership
{
    internal class UmbracoMembershipUser<T> : MembershipUser where T : IMembershipUser
    {
        private T _user;

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoMembershipUser{T}"/> class.
        /// </summary>
        public UmbracoMembershipUser(T user)
        {
            _user = user;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoMembershipUser{T}"/> class.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="name">The name.</param>
        /// <param name="providerUserKey">The provider user key.</param>
        /// <param name="email">The email.</param>
        /// <param name="passwordQuestion">The password question.</param>
        /// <param name="comment">The comment.</param>
        /// <param name="isApproved">if set to <c>true</c> [is approved].</param>
        /// <param name="isLockedOut">if set to <c>true</c> [is locked out].</param>
        /// <param name="creationDate">The creation date.</param>
        /// <param name="lastLoginDate">The last login date.</param>
        /// <param name="lastActivityDate">The last activity date.</param>
        /// <param name="lastPasswordChangedDate">The last password changed date.</param>
        /// <param name="lastLockoutDate">The last lockout date.</param>
        /// <param name="fullName">The full name.</param>
        /// <param name="language">The language.</param>
        /// <param name="user"></param>
        public UmbracoMembershipUser(string providerName, string name, object providerUserKey, string email, 
            string passwordQuestion, string comment, bool isApproved, bool isLockedOut, 
            DateTime creationDate, DateTime lastLoginDate, DateTime lastActivityDate, DateTime lastPasswordChangedDate,
            DateTime lastLockoutDate, string fullName, string language, T user) 
            : base( providerName, name, providerUserKey, email, passwordQuestion, comment, isApproved, isLockedOut, 
                creationDate, lastLoginDate, lastActivityDate, lastPasswordChangedDate, lastLockoutDate)
        {
            _user = user;
        }

        #endregion
    }
}