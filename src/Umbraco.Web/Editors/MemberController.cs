using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Security;
using AutoMapper;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Web.WebApi;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Binders;
using Umbraco.Web.WebApi.Filters;
using umbraco;
using Constants = Umbraco.Core.Constants;
using Examine;

namespace Umbraco.Web.Editors
{
    /// <remarks>
    /// This controller is decorated with the UmbracoApplicationAuthorizeAttribute which means that any user requesting
    /// access to ALL of the methods on this controller will need access to the member application.
    /// </remarks>
    [PluginController("UmbracoApi")]
    [UmbracoApplicationAuthorizeAttribute(Constants.Applications.Members)]
    public class MemberController : ContentControllerBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MemberController()
            : this(UmbracoContext.Current)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        public MemberController(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
        }

        /// <summary>
        /// Gets the content json for the member
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public MemberDisplay GetByKey(Guid key)
        {
            if (Membership.Provider.Name == Constants.Conventions.Member.UmbracoMemberProviderName)
            {
                var foundMember = Services.MemberService.GetByKey(key);
                if (foundMember == null)
                {
                    HandleContentNotFound(key);
                }
                return Mapper.Map<IMember, MemberDisplay>(foundMember);
            }
            else
            {
                //TODO: Support this
                throw new HttpResponseException(Request.CreateValidationErrorResponse("Editing member with a non-umbraco membership provider is currently not supported"));
            }

        }

        /// <summary>
        /// Gets an empty content item for the 
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        /// <returns></returns>
        public MemberDisplay GetEmpty(string contentTypeAlias)
        {
            var contentType = Services.MemberTypeService.GetMemberType(contentTypeAlias);
            if (contentType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            IMember emptyContent = new Member("", contentType);
            emptyContent.AdditionalData["NewPassword"] = Membership.GeneratePassword(Membership.MinRequiredPasswordLength, Membership.MinRequiredNonAlphanumericCharacters);
            return Mapper.Map<IMember, MemberDisplay>(emptyContent);
        }

        /// <summary>
        /// Saves member
        /// </summary>
        /// <returns></returns>        
        [FileUploadCleanupFilter]
        [MembershipProviderValidationFilter]
        public MemberDisplay PostSave(
            [ModelBinder(typeof(MemberBinder))]
                MemberSave contentItem)
        {
            //TODO : Support this!
            if (Membership.Provider.Name != Constants.Conventions.Member.UmbracoMemberProviderName)
            {
                throw new NotSupportedException("Currently the member editor does not support providers that are not the default Umbraco membership provider ");
            }

            //If we've reached here it means:
            // * Our model has been bound
            // * and validated
            // * any file attachments have been saved to their temporary location for us to use
            // * we have a reference to the DTO object and the persisted object
            // * Permissions are valid
            
            //map the properties to the persisted entity
            MapPropertyValues(contentItem);

            //Unlike content/media - if there are errors for a member, we do NOT proceed to save them, we cannot so return the errors
            if (ModelState.IsValid == false)
            {
                var forDisplay = Mapper.Map<IMember, MemberDisplay>(contentItem.PersistedContent);
                forDisplay.Errors = ModelState.ToErrorDictionary();
                throw new HttpResponseException(Request.CreateValidationErrorResponse(forDisplay));
            }

            //TODO: WE need to support this! - requires UI updates, etc...
            if (Membership.Provider.RequiresQuestionAndAnswer)
            {
                throw new NotSupportedException("Currently the member editor does not support providers that have RequiresQuestionAndAnswer specified");
            }

            string generatedPassword = null;
            //Depending on the action we need to first do a create or update using the membership provider
            // this ensures that passwords are formatted correclty and also performs the validation on the provider itself.
            switch (contentItem.Action)
            {
                case ContentSaveAction.Save:
                    generatedPassword = UpdateWithMembershipProvider(contentItem);
                    break;
                case ContentSaveAction.SaveNew:
                    MembershipCreateStatus status;
                    CreateWithUmbracoProvider(contentItem, out status);
                    break;
                default:
                    //we don't support anything else for members
                    throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            //If we've had problems creating/updating the user with the provider then return the error
            if (ModelState.IsValid == false)
            {
                var forDisplay = Mapper.Map<IMember, MemberDisplay>(contentItem.PersistedContent);
                forDisplay.Errors = ModelState.ToErrorDictionary();
                throw new HttpResponseException(Request.CreateValidationErrorResponse(forDisplay));
            }

            //save the item
            //NOTE: We are setting the password to NULL - this indicates to the system to not actually save the password
            // so it will not get overwritten!
            contentItem.PersistedContent.Password = null;
            
            //create/save the IMember
            Services.MemberService.Save(contentItem.PersistedContent);

            //Now let's do the role provider stuff - now that we've saved the content item (that is important since
            // if we are changing the username, it must be persisted before looking up the member roles).
            var currGroups = Roles.GetRolesForUser(contentItem.PersistedContent.Username);
            //find the ones to remove and remove them
            var toRemove = currGroups.Except(contentItem.Groups).ToArray();            
            if (toRemove.Any())
            {
                Roles.RemoveUserFromRoles(contentItem.PersistedContent.Username, toRemove);
            }
            //find the ones to add and add them
            var toAdd = contentItem.Groups.Except(currGroups).ToArray();
            if (toAdd.Any())
            {
                //add the ones submitted
                Roles.AddUserToRoles(contentItem.PersistedContent.Username, toAdd);
            }

            //set the generated password (if there was one) - in order to do this we'll chuck the gen'd password into the
            // additional data of the IUmbracoEntity of the persisted item - then we can retreive this in the model mapper and set 
            // the value to be given to the UI. Hooray for AdditionalData :)
            contentItem.PersistedContent.AdditionalData["GeneratedPassword"] = generatedPassword;

            //return the updated model
            var display = Mapper.Map<IMember, MemberDisplay>(contentItem.PersistedContent);
            
            //lasty, if it is not valid, add the modelstate to the outgoing object and throw a 403
            HandleInvalidModelState(display);

            //put the correct msgs in 
            switch (contentItem.Action)
            {
                case ContentSaveAction.Save:
                case ContentSaveAction.SaveNew:
                    display.AddSuccessNotification(ui.Text("speechBubbles", "editMemberSaved"), ui.Text("speechBubbles", "editMemberSaved"));
                    break;
            }

            return display;
        }

        /// <summary>
        /// Maps the property values to the persisted entity
        /// </summary>
        /// <param name="contentItem"></param>
        private void MapPropertyValues(MemberSave contentItem)
        {
            UpdateName(contentItem);

            //map the custom properties - this will already be set for new entities in our member binder
            contentItem.PersistedContent.Email = contentItem.Email;
            contentItem.PersistedContent.Username = contentItem.Username;

            //use the base method to map the rest of the properties
            base.MapPropertyValues(contentItem);
        }

        /// <summary>
        /// Update the membership user using the membership provider (for things like email, etc...)
        /// If a password change is detected then we'll try that too.
        /// </summary>
        /// <param name="contentItem"></param>
        /// <returns>
        /// If the password has been reset then this method will return the reset/generated password, otherwise will return null.
        /// </returns>
        private string UpdateWithMembershipProvider(MemberSave contentItem)
        {
            //Get the member from the provider

            var membershipUser = Membership.Provider.GetUser(contentItem.PersistedContent.Key, false);
            if (membershipUser == null)
            {
                //This should never happen! so we'll let it YSOD if it does.
                throw new InvalidOperationException("Could not get member from membership provider " + Membership.Provider.Name + " with key " + contentItem.PersistedContent.Key);
            }

            var shouldReFetchMember = false;

            //Update the membership user if it has changed
            if (HasMembershipUserChanged(membershipUser, contentItem))
            {
                membershipUser.Email = contentItem.Email.Trim();
                membershipUser.IsApproved = contentItem.IsApproved;
                membershipUser.Comment = contentItem.Comments;
                try
                {
                    Membership.Provider.UpdateUser(membershipUser);
                    shouldReFetchMember = true;
                }
                catch (Exception ex)
                {
                    LogHelper.WarnWithException<MemberController>("Could not update member, the provider returned an error", ex);
                    ModelState.AddPropertyError(
                        //specify 'default' just so that it shows up as a notification - is not assigned to a property
                        new ValidationResult("Could not update member, the provider returned an error: " + ex.Message + " (see log for full details)"), "default");
                }
            }

            //if they were locked but now they are trying to be unlocked
            if (membershipUser.IsLockedOut && contentItem.IsLockedOut == false)
            {
                try
                {
                    var result = Membership.Provider.UnlockUser(membershipUser.UserName);
                    if (result == false)
                    {
                        //it wasn't successful - but it won't really tell us why.
                        ModelState.AddModelError("custom", "Could not unlock the user");
                    }
                    else
                    {
                        shouldReFetchMember = true;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("custom", ex);
                }
            }
            else if (membershipUser.IsLockedOut == false && contentItem.IsLockedOut)
            {
                //NOTE: This should not ever happen unless someone is mucking around with the request data.
                //An admin cannot simply lock a user, they get locked out by password attempts, but an admin can un-approve them
                ModelState.AddModelError("custom", "An admin cannot lock a user");
            }
           
            //password changes ?           
            if (contentItem.Password == null) return null;

            var passwordChangeResult = Security.ChangePassword(membershipUser.UserName, contentItem.Password, Membership.Provider);
            if (passwordChangeResult.Success)
            {
                //If the provider has changed some values, these values need to be reflected in the member object 
                //that will get mapped to the display object
                if (shouldReFetchMember)
                {
                    //Go and re-fetch the persisted item
                    contentItem.PersistedContent = Services.MemberService.GetByUsername(contentItem.Username.Trim());
                    //remap the values to save
                    MapPropertyValues(contentItem);
                }

                //even if we weren't resetting this, it is the correct value (null), otherwise if we were resetting then it will contain the new pword
                return passwordChangeResult.Result.ResetPassword;
            }

            //it wasn't successful, so add the change error to the model state
            ModelState.AddPropertyError(
                passwordChangeResult.Result.ChangeError,
                string.Format("{0}password", Constants.PropertyEditors.InternalGenericPropertiesPrefix));


            return null;
        }

        /// <summary>
        /// Quick check to see if the 'normal' settable properties for the membership provider have changed
        /// </summary>
        /// <param name="membershipUser"></param>
        /// <param name="contentItem"></param>
        /// <returns></returns>
        /// <remarks>
        /// By 'normal' we mean that they can simply be set on the membership user and don't require method calls like ChangePassword or UnlockUser
        /// </remarks>
        private bool HasMembershipUserChanged(MembershipUser membershipUser, MemberSave contentItem)
        {
            if (contentItem.Email.Trim().InvariantEquals(membershipUser.Email) == false
                || contentItem.IsApproved != membershipUser.IsApproved
                || contentItem.Comments != membershipUser.Comment)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// This is going to create the user with the membership provider and check for validation
        /// </summary>
        /// <param name="contentItem"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        /// <remarks>
        /// If this is successful, it will go and re-fetch the IMember from the db because it will now have an ID because the Umbraco provider 
        /// uses the umbraco data store - then of course we need to re-map it to the saved property values.
        /// </remarks>
        private MembershipUser CreateWithUmbracoProvider(MemberSave contentItem, out MembershipCreateStatus status)
        {
            //if we are creating a new one, create the member using the membership provider first

            //TODO: I think we should detect if the Umbraco membership provider is active, if so then we'll create the member first  and the provider key doesn't matter
            // but if we are using a 3rd party membership provider - then we should create our IMember first and use it's key as their provider user key!

            //NOTE: We are casting directly to the umbraco membership provider so we can specify the member type that we want to use!

            var umbracoMembershipProvider = (global::umbraco.providers.members.UmbracoMembershipProvider)Membership.Provider;
            
            //TODO: We are not supporting q/a - passing in empty here
            var membershipUser = umbracoMembershipProvider.CreateUser(
                contentItem.ContentTypeAlias, contentItem.Username,
                contentItem.Password.NewPassword,
                contentItem.Email, "", "", true, Guid.NewGuid(), out status);

            //TODO: Localize these!
            switch (status)
            {
                case MembershipCreateStatus.Success:

                    //if the comments are there then we need to save them
                    if (contentItem.Comments.IsNullOrWhiteSpace() == false)
                    {
                        membershipUser.Comment = contentItem.Comments;
                        umbracoMembershipProvider.UpdateUser(membershipUser);
                    }

                    //Go and re-fetch the persisted item
                    contentItem.PersistedContent = Services.MemberService.GetByUsername(contentItem.Username.Trim());
                    //remap the values to save
                    MapPropertyValues(contentItem);

                    break;
                case MembershipCreateStatus.InvalidUserName:
                    ModelState.AddPropertyError(
                        new ValidationResult("Invalid user name", new[] { "value" }),
                        string.Format("{0}login", Constants.PropertyEditors.InternalGenericPropertiesPrefix));
                    break;
                case MembershipCreateStatus.InvalidPassword:
                    ModelState.AddPropertyError(
                        new ValidationResult("Invalid password", new[] { "value" }),
                        string.Format("{0}password", Constants.PropertyEditors.InternalGenericPropertiesPrefix));
                    break;
                case MembershipCreateStatus.InvalidQuestion:
                case MembershipCreateStatus.InvalidAnswer:
                    throw new NotSupportedException("Currently the member editor does not support providers that have RequiresQuestionAndAnswer specified");
                case MembershipCreateStatus.InvalidEmail:
                    ModelState.AddPropertyError(
                        new ValidationResult("Invalid email", new[] { "value" }),
                        string.Format("{0}email", Constants.PropertyEditors.InternalGenericPropertiesPrefix));
                    break;
                case MembershipCreateStatus.DuplicateUserName:
                    ModelState.AddPropertyError(
                        new ValidationResult("Username is already in use", new[] { "value" }),
                        string.Format("{0}login", Constants.PropertyEditors.InternalGenericPropertiesPrefix));
                    break;
                case MembershipCreateStatus.DuplicateEmail:
                    ModelState.AddPropertyError(
                        new ValidationResult("Email address is already in use", new[] { "value" }),
                        string.Format("{0}email", Constants.PropertyEditors.InternalGenericPropertiesPrefix));
                    break;
                case MembershipCreateStatus.InvalidProviderUserKey:
                    ModelState.AddPropertyError(
                        //specify 'default' just so that it shows up as a notification - is not assigned to a property
                       new ValidationResult("Invalid provider user key"), "default");
                    break;
                case MembershipCreateStatus.DuplicateProviderUserKey:
                    ModelState.AddPropertyError(
                        //specify 'default' just so that it shows up as a notification - is not assigned to a property
                       new ValidationResult("Duplicate provider user key"), "default");
                    break;
                case MembershipCreateStatus.ProviderError:
                case MembershipCreateStatus.UserRejected:
                    ModelState.AddPropertyError(
                        //specify 'default' just so that it shows up as a notification - is not assigned to a property
                        new ValidationResult("User could not be created (rejected by provider)"), "default");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return membershipUser;
        }

        /// <summary>
        /// Permanently deletes a member
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>        
        public HttpResponseMessage DeleteByKey(Guid key)
        {
            var foundMember = Services.MemberService.GetByKey(key);
            if (foundMember == null)
            {
                return HandleContentNotFound(key, false);
            }

            Services.MemberService.Delete(foundMember);

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
