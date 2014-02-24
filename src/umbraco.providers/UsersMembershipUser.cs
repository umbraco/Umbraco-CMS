#region namespace
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Security;
using umbraco.BusinessLogic;
using System.Data;
#endregion

namespace umbraco.providers
{
    /// <summary>
    /// Wrapper for the umbraco.BusinessLogic.User class.
    /// </summary>
    [Obsolete("This class is used by the legacy user's membership provider which is also obsolete, this shouldn't be referenced directly in code, the standard .Net MembershipUser base class object should be referenced instead.")]
    public class UsersMembershipUser : MembershipUser
    {
        #region Fields and Properties
        private string _FullName;

        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        /// <value>The full name.</value>
        public string FullName
        {
            get { return _FullName; }
            set { _FullName = value; }
        }
        private string _Language;

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        /// <value>The language.</value>
        public string Language
        {
            get { return _Language; }
            set { _Language = value; }
        }
        private UserType _UserType;

        /// <summary>
        /// Gets or sets the type of the user.
        /// </summary>
        /// <value>The type of the user.</value>
        public UserType UserType
        {
            get { return _UserType; }
            set { _UserType = value; }
        }         
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoMembershipUser"/> class.
        /// </summary>
        protected UsersMembershipUser()
        {
            
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoMembershipUser"/> class.
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
        /// <param name="userType">Type of the user.</param>
        public UsersMembershipUser(string providerName, string name, object providerUserKey, string email, 
            string passwordQuestion, string comment, bool isApproved, bool isLockedOut, 
            DateTime creationDate, DateTime lastLoginDate, DateTime lastActivityDate, DateTime lastPasswordChangedDate, 
            DateTime lastLockoutDate, string fullName, string language, UserType userType ) 
            : base( providerName, name, providerUserKey, email, passwordQuestion, comment, isApproved, isLockedOut, 
                creationDate, lastLoginDate, lastActivityDate, lastPasswordChangedDate, lastLockoutDate)
        {
            _FullName = fullName;
            _UserType = userType;
            _Language = language;
        }
        #endregion       
    }
}
