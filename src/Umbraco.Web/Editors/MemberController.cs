using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

            var emptyContent = new Core.Models.Member("", contentType);
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

            //Depending on the action we need to first do a create or update using the membership provider
            // this ensures that passwords are formatted correclty and also performs the validation on the provider itself.
            switch (contentItem.Action)
            {
                case ContentSaveAction.Save:
                    UpdateWithMembershipProvider(contentItem);
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
        /// </summary>
        /// <param name="contentItem"></param>
        private void UpdateWithMembershipProvider(MemberSave contentItem)
        {
            //Get the member from the provider
            var membershipUser = Membership.Provider.GetUser(contentItem.PersistedContent.Username, false);
            if (membershipUser == null)
            {
                //This should never happen! so we'll let it YSOD if it does.
                throw new InvalidOperationException("Could not get member from membership provider " + Membership.Provider.Name + " with username " + contentItem.PersistedContent.Username);
            }

            //ok, first thing to do is check if they've changed their email 
            //TODO: When we support the other membership provider data then we'll check if any of that's changed too.
            if (contentItem.Email.Trim().InvariantEquals(membershipUser.Email))
            {
                membershipUser.Email = contentItem.Email.Trim();

                try
                {
                    Membership.Provider.UpdateUser(membershipUser);
                }
                catch (Exception ex)
                {
                    LogHelper.WarnWithException<MemberController>("Could not update member, the provider returned an error", ex);
                    ModelState.AddPropertyError(
                        //specify 'default' just so that it shows up as a notification - is not assigned to a property
                        new ValidationResult("Could not update member, the provider returned an error: " + ex.Message + " (see log for full details)"), "default");
                }
            }
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
            var membershipUser = umbracoMembershipProvider.CreateUser(
                contentItem.ContentTypeAlias, contentItem.Username, contentItem.Password, contentItem.Email, "", "", true, Guid.NewGuid(), out status);

            //TODO: Localize these!
            switch (status)
            {
                case MembershipCreateStatus.Success:
                    
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
