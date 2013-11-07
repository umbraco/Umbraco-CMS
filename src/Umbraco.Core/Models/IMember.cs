using System;

namespace Umbraco.Core.Models
{
    public interface IMember : IContentBase
    {
        /// <summary>
        /// Gets or sets the Username
        /// </summary>
        string Username { get; set; }

        /// <summary>
        /// Gets or sets the Email
        /// </summary>
        string Email { get; set; }

        /// <summary>
        /// Gets or sets the Password
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// Gets or sets the Password Question
        /// </summary>
        /// <remarks>
        /// Alias: umbracoPasswordRetrievalQuestionPropertyTypeAlias
        /// Part of the standard properties collection.
        /// </remarks>
        string PasswordQuestion { get; set; }

        /// <summary>
        /// Gets or sets the Password Answer
        /// </summary>
        /// <remarks>
        /// Alias: umbracoPasswordRetrievalAnswerPropertyTypeAlias
        /// Part of the standard properties collection.
        /// </remarks>
        string PasswordAnswer { get; set; }

        /// <summary>
        /// Gets or set the comments for the member
        /// </summary>
        /// <remarks>
        /// Alias: umbracoCommentPropertyTypeAlias
        /// Part of the standard properties collection.
        /// </remarks>
        string Comments { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether the Member is approved
        /// </summary>
        /// <remarks>
        /// Alias: umbracoApprovePropertyTypeAlias
        /// Part of the standard properties collection.
        /// </remarks>
        bool IsApproved { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether the Member is locked out
        /// </summary>
        /// <remarks>
        /// Alias: umbracoLockPropertyTypeAlias
        /// Part of the standard properties collection.
        /// </remarks>
        bool IsLockedOut { get; set; }

        /// <summary>
        /// Gets or sets the date for last login
        /// </summary>
        /// <remarks>
        /// Alias: umbracoLastLoginPropertyTypeAlias
        /// Part of the standard properties collection.
        /// </remarks>
        DateTime LastLoginDate { get; set; }

        /// <summary>
        /// Gest or sets the date for last password change
        /// </summary>
        /// <remarks>
        /// Alias: umbracoMemberLastPasswordChange
        /// Part of the standard properties collection.
        /// </remarks>
        DateTime LastPasswordChangeDate { get; set; }

        /// <summary>
        /// Gets or sets the date for when Member was locked out
        /// </summary>
        /// <remarks>
        /// Alias: umbracoMemberLastLockout
        /// Part of the standard properties collection.
        /// </remarks>
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

        /// <summary>
        /// String alias of the default ContentType
        /// </summary>
        string ContentTypeAlias { get; }

        /// <summary>
        /// Gets the ContentType used by this content object
        /// </summary>
        IMemberType ContentType { get; }
    }
}