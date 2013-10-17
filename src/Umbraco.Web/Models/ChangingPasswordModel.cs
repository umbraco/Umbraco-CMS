using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// A model representing an attempt at changing a password
    /// </summary>
    public class PasswordChangedModel
    {
        /// <summary>
        /// The error affiliated with the failing password changes, null if changing was successful
        /// </summary>
        public ValidationResult ChangeError { get; set; }

        /// <summary>
        /// If the password was reset, this is the value it has been changed to
        /// </summary>
        public string ResetPassword { get; set; }
    }

    /// <summary>
    /// A model representing the data required to set a member/user password depending on the provider installed.
    /// </summary>
    public class ChangingPasswordModel
    {
        /// <summary>
        /// The password value
        /// </summary>
        /// <remarks>
        /// This 
        /// </remarks>
        [DataMember(Name = "newPassword")]
        public string NewPassword { get; set; }

        /// <summary>
        /// The old password - used to change a password when: EnablePasswordRetrieval = false 
        /// </summary>
        [DataMember(Name = "oldPassword")]
        public string OldPassword { get; set; }

        /// <summary>
        /// Set to true if the password is to be reset - only valid when: EnablePasswordReset = true
        /// </summary>
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
    }
}