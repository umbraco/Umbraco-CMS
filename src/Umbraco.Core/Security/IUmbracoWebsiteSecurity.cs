using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models.Security;

namespace Umbraco.Cms.Core.Security
{
    // TODO: I think we can kill this whole thing, the logic can just be in the controllers
    public interface IUmbracoWebsiteSecurity
    {
        /// <summary>
        /// Creates a model to use for registering new members with custom member properties
        /// </summary>
        /// <param name="memberTypeAlias">Alias of member type for created member (default used if not provided).</param>
        /// <returns>Instance of <see cref="RegisterModel"/></returns>
        RegisterModel CreateRegistrationModel(string memberTypeAlias = null);

        /// <summary>
        /// Registers a new member.
        /// </summary>
        /// <param name="model">Register member model.</param>
        /// <param name="logMemberIn">Flag for whether to log the member in upon successful registration.</param>
        /// <returns>Result of registration operation.</returns>
        Task<RegisterMemberStatus> RegisterMemberAsync(RegisterModel model, bool logMemberIn = true);

        /// <summary>
        /// Creates a new profile model filled in with the current members details if they are logged in which allows for editing
        /// profile properties.
        /// </summary>
        /// <returns>Instance of <see cref="ProfileModel"/></returns>
        Task<ProfileModel> GetCurrentMemberProfileModelAsync();

        /// <summary>
        /// Updates the currently logged in member's profile.
        /// </summary>
        /// <param name="model">Update member profile model.</param>
        /// <returns>Result of update profile operation.</returns>
        Task<UpdateMemberProfileResult> UpdateMemberProfileAsync(ProfileModel model);

        // TODO: Kill this, we will just use the MemberManager / MemberSignInManager
        Task<bool> LoginAsync(string username, string password);

        // TODO: Kill this, we will just use the MemberManager
        bool IsLoggedIn();

        /// <summary>
        /// Returns the login status model of the currently logged in member.
        /// </summary>
        /// <returns>Instance of <see cref="LoginStatusModel"/></returns>
        Task<LoginStatusModel> GetCurrentLoginStatusAsync();

        // TODO: Kill this, we will just use the MemberManager
        Task LogOutAsync();

        /// <summary>
        /// Checks if the current member is authorized based on the parameters provided.
        /// </summary>
        /// <param name="allowTypes">Allowed types.</param>
        /// <param name="allowGroups">Allowed groups.</param>
        /// <param name="allowMembers">Allowed individual members.</param>
        /// <returns>True or false if the currently logged in member is authorized</returns>
        bool IsMemberAuthorized(
            IEnumerable<string> allowTypes = null,
            IEnumerable<string> allowGroups = null,
            IEnumerable<int> allowMembers = null);
    }
}
