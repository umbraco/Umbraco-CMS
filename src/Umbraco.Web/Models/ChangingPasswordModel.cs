using System.Runtime.Serialization;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// A model representing the data required to set a member/user password depending on the provider installed.
    /// </summary>
    public class ChangingPasswordModel
    {
        /// <summary>
        /// The password value
        /// </summary>
        [DataMember(Name = "newPassword")]
        public string NewPassword { get; set; }

        /// <summary>
        /// The old password - used to change a password when: EnablePasswordRetrieval = false
        /// </summary>
        [DataMember(Name = "oldPassword")]
        public string OldPassword { get; set; }

        /// <summary>
        /// Set to true if the password is to be reset
        /// </summary>
        /// <remarks>
        /// <para>
        /// This operator is different between using ASP.NET Identity APIs and Membership APIs.
        /// </para>
        /// <para>
        /// When using Membership APIs, this is only valid when: EnablePasswordReset = true and it will reset the password to something auto generated.
        /// </para>
        /// <para>
        /// When using ASP.NET Identity APIs this needs to be set if an administrator user that has access to the Users section is changing another users
        /// password. This flag is required to indicate that the oldPassword value is not required and that we are in fact performing a password reset and
        /// then a password change if the executing user has access to do so.
        /// </para>
        /// </remarks>
        [DataMember(Name = "reset")]
        public bool? Reset { get; set; }

        /// <summary>
        /// The password answer - required for reset when: RequiresQuestionAndAnswer = true
        /// </summary>
        [DataMember(Name = "answer")]
        public string Answer { get; set; }

        /// <summary>
        /// This is filled in on the server side if the password has been reset/generated
        /// </summary>
        [DataMember(Name = "generatedPassword")]
        public string GeneratedPassword { get; set; }

        /// <summary>
        /// The id of the user - required to allow changing password without the entire UserSave model
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }
    }
}
