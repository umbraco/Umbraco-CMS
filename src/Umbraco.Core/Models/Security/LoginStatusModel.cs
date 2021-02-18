using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.Models.Security
{
    /// <summary>
    /// The model representing the status of a logged in member.
    /// </summary>
    public class LoginStatusModel
    {
        /// <summary>
        /// Creates a new empty LoginStatusModel.
        /// </summary>
        /// <returns></returns>
        public static LoginStatusModel CreateModel()
        {
            return new LoginStatusModel();
        }

        /// <summary>
        /// The name of the member
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// The username of the member
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The email of the member
        /// </summary>
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// True, if the member is currently logged in
        /// </summary>
        public bool IsLoggedIn { get; set; }
    }
}
