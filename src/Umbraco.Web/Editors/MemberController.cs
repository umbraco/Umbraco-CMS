using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Mapping;
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
    [OutgoingNoHyphenGuidFormat]
    public class MemberController : ContentControllerBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MemberController()
            : this(UmbracoContext.Current)
        {
            _provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        public MemberController(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
            _provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();
        }

        private readonly MembershipProvider _provider;

        /// <summary>
        /// Returns the currently configured membership scenario for members in umbraco
        /// </summary>
        /// <value></value>
        protected MembershipScenario MembershipScenario
        {
            get { return Services.MemberService.GetMembershipScenario(); }
        }

        public PagedResult<MemberBasic> GetPagedResults(
            int pageNumber = 1,
            int pageSize = 100,
            string orderBy = "username",
            Direction orderDirection = Direction.Ascending,
            bool orderBySystemField = true,
            string filter = "",
            string memberTypeAlias = null)
        {

            if (pageNumber <= 0 || pageSize <= 0)
            {
                throw new NotSupportedException("Both pageNumber and pageSize must be greater than zero");
            }

            if (MembershipScenario == MembershipScenario.NativeUmbraco)
            {
                long totalRecords;
                var members = Services.MemberService
                    .GetAll((pageNumber - 1), pageSize, out totalRecords, orderBy, orderDirection, orderBySystemField, memberTypeAlias, filter).ToArray();
                if (totalRecords == 0)
                {
                    return new PagedResult<MemberBasic>(0, 0, 0);
                }
                var pagedResult = new PagedResult<MemberBasic>(totalRecords, pageNumber, pageSize);
                pagedResult.Items = members
                    .Select(Mapper.Map<IMember, MemberBasic>);
                return pagedResult;
            }
            else
            {
                int totalRecords;

                MembershipUserCollection members;
                if (filter.IsNullOrWhiteSpace())
                {
                    members = _provider.GetAllUsers((pageNumber - 1), pageSize, out totalRecords);
                }
                else
                {
                    //we need to search!

                    //try by name first
                    members = _provider.FindUsersByName(filter, (pageNumber - 1), pageSize, out totalRecords);
                    if (totalRecords == 0)
                    {
                        //try by email then
                        members = _provider.FindUsersByEmail(filter, (pageNumber - 1), pageSize, out totalRecords);
                    }
                }
                if (totalRecords == 0)
                {
                    return new PagedResult<MemberBasic>(0, 0, 0);
                }
                var pagedResult = new PagedResult<MemberBasic>(totalRecords, pageNumber, pageSize);
                pagedResult.Items = members
                    .Cast<MembershipUser>()
                    .Select(Mapper.Map<MembershipUser, MemberBasic>);
                return pagedResult;
            }

        }

        /// <summary>
        /// Returns a display node with a list view to render members
        /// </summary>
        /// <param name="listName"></param>
        /// <returns></returns>
        public MemberListDisplay GetListNodeDisplay(string listName)
        {
            var display = new MemberListDisplay
            {
                ContentTypeAlias = listName,
                ContentTypeName = listName,
                Id = listName,
                IsContainer = true,
                Name = listName == Constants.Conventions.MemberTypes.AllMembersListId ? "All Members" : listName,
                Path = "-1," + listName,
                ParentId = -1
            };

            TabsAndPropertiesResolver.AddListView(display, "member", Services.DataTypeService, Services.TextService);

            return display;
        }

        /// <summary>
        /// Gets the content json for the member
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [OutgoingEditorModelEvent]
        public MemberDisplay GetByKey(Guid key)
        {
            MembershipUser foundMembershipMember;
            MemberDisplay display;
            IMember foundMember;
            switch (MembershipScenario)
            {
                case MembershipScenario.NativeUmbraco:
                    foundMember = Services.MemberService.GetByKey(key);
                    if (foundMember == null)
                    {
                        HandleContentNotFound(key);
                    }
                    return Mapper.Map<IMember, MemberDisplay>(foundMember);
                case MembershipScenario.CustomProviderWithUmbracoLink:

                //TODO: Support editing custom properties for members with a custom membership provider here.

                //foundMember = Services.MemberService.GetByKey(key);
                //if (foundMember == null)
                //{
                //    HandleContentNotFound(key);
                //}
                //foundMembershipMember = Membership.GetUser(key, false);
                //if (foundMembershipMember == null)
                //{
                //    HandleContentNotFound(key);
                //}

                //display = Mapper.Map<MembershipUser, MemberDisplay>(foundMembershipMember);
                ////map the name over
                //display.Name = foundMember.Name;
                //return display;

                case MembershipScenario.StandaloneCustomProvider:
                default:
                    foundMembershipMember = _provider.GetUser(key, false);
                    if (foundMembershipMember == null)
                    {
                        HandleContentNotFound(key);
                    }
                    display = Mapper.Map<MembershipUser, MemberDisplay>(foundMembershipMember);
                    return display;
            }
        }

        /// <summary>
        /// Gets an empty content item for the 
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        /// <returns></returns>
        [OutgoingEditorModelEvent]
        public MemberDisplay GetEmpty(string contentTypeAlias = null)
        {
            IMember emptyContent;
            switch (MembershipScenario)
            {
                case MembershipScenario.NativeUmbraco:
                    if (contentTypeAlias == null)
                    {
                        throw new HttpResponseException(HttpStatusCode.NotFound);
                    }

                    var contentType = Services.MemberTypeService.Get(contentTypeAlias);
                    if (contentType == null)
                    {
                        throw new HttpResponseException(HttpStatusCode.NotFound);
                    }

                    var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();

                    emptyContent = new Member(contentType);
                    emptyContent.AdditionalData["NewPassword"] = Membership.GeneratePassword(provider.MinRequiredPasswordLength, provider.MinRequiredNonAlphanumericCharacters);
                    return Mapper.Map<IMember, MemberDisplay>(emptyContent);
                case MembershipScenario.CustomProviderWithUmbracoLink:
                //TODO: Support editing custom properties for members with a custom membership provider here.

                case MembershipScenario.StandaloneCustomProvider:
                default:
                    //we need to return a scaffold of a 'simple' member - basically just what a membership provider can edit
                    emptyContent = MemberService.CreateGenericMembershipProviderMember("", "", "", "");
                    emptyContent.AdditionalData["NewPassword"] = Membership.GeneratePassword(Membership.MinRequiredPasswordLength, Membership.MinRequiredNonAlphanumericCharacters);
                    return Mapper.Map<IMember, MemberDisplay>(emptyContent);
            }
        }

        /// <summary>
        /// Saves member
        /// </summary>
        /// <returns></returns>        
        [FileUploadCleanupFilter]
        public MemberDisplay PostSave(
            [ModelBinder(typeof(MemberBinder))]
                MemberSave contentItem)
        {

            //If we've reached here it means:
            // * Our model has been bound
            // * and validated
            // * any file attachments have been saved to their temporary location for us to use
            // * we have a reference to the DTO object and the persisted object
            // * Permissions are valid

            //This is a special case for when we're not using the umbraco membership provider - when this is the case
            // we will not have a ContentTypeAlias set which means the model state will be invalid but we don't care about that
            // so we'll remove that model state value
            if (MembershipScenario != MembershipScenario.NativeUmbraco)
            {
                ModelState.Remove("ContentTypeAlias");

                //TODO: We're removing this because we are not displaying it but when we support the CustomProviderWithUmbracoLink scenario
                // we will be able to have a real name associated so do not remove this state once that is implemented!
                ModelState.Remove("Name");
            }

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
            if (_provider.RequiresQuestionAndAnswer)
            {
                throw new NotSupportedException("Currently the member editor does not support providers that have RequiresQuestionAndAnswer specified");
            }

            //We're gonna look up the current roles now because the below code can cause
            // events to be raised and developers could be manually adding roles to members in 
            // their handlers. If we don't look this up now there's a chance we'll just end up
            // removing the roles they've assigned.
            var currRoles = Roles.GetRolesForUser(contentItem.PersistedContent.Username);
            //find the ones to remove and remove them
            var rolesToRemove = currRoles.Except(contentItem.Groups).ToArray();

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
                    CreateWithMembershipProvider(contentItem, out status);

                    // save the ID of the creator
                    contentItem.PersistedContent.CreatorId = Security.CurrentUser.Id;
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

            //save the IMember - 
            //TODO: When we support the CustomProviderWithUmbracoLink scenario, we'll need to save the custom properties for that here too
            if (MembershipScenario == MembershipScenario.NativeUmbraco)
            {
                //save the item
                //NOTE: We are setting the password to NULL - this indicates to the system to not actually save the password
                // so it will not get overwritten!
                contentItem.PersistedContent.RawPasswordValue = null;

                //create/save the IMember
                Services.MemberService.Save(contentItem.PersistedContent);
            }

            //Now let's do the role provider stuff - now that we've saved the content item (that is important since
            // if we are changing the username, it must be persisted before looking up the member roles).
            if (rolesToRemove.Any())
            {
                Roles.RemoveUserFromRoles(contentItem.PersistedContent.Username, rolesToRemove);
            }
            //find the ones to add and add them
            var toAdd = contentItem.Groups.Except(currRoles).ToArray();
            if (toAdd.Any())
            {
                //add the ones submitted
                Roles.AddUserToRoles(contentItem.PersistedContent.Username, toAdd);
            }

            //set the generated password (if there was one) - in order to do this we'll chuck the gen'd password into the
            // additional data of the IUmbracoEntity of the persisted item - then we can retrieve this in the model mapper and set 
            // the value to be given to the UI. Hooray for AdditionalData :)
            contentItem.PersistedContent.AdditionalData["GeneratedPassword"] = generatedPassword;

            //return the updated model
            var display = Mapper.Map<IMember, MemberDisplay>(contentItem.PersistedContent);

            //lasty, if it is not valid, add the modelstate to the outgoing object and throw a 403
            HandleInvalidModelState(display);

            var localizedTextService = Services.TextService;
            //put the correct msgs in 
            switch (contentItem.Action)
            {
                case ContentSaveAction.Save:
                case ContentSaveAction.SaveNew:
                    display.AddSuccessNotification(localizedTextService.Localize("speechBubbles/editMemberSaved"), localizedTextService.Localize("speechBubbles/editMemberSaved"));
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

            var membershipUser = _provider.GetUser(contentItem.PersistedContent.Key, false);
            if (membershipUser == null)
            {
                //This should never happen! so we'll let it YSOD if it does.
                throw new InvalidOperationException("Could not get member from membership provider " + _provider.Name + " with key " + contentItem.PersistedContent.Key);
            }

            var shouldReFetchMember = false;
            var providedUserName = contentItem.PersistedContent.Username;

            //Update the membership user if it has changed
            try
            {
                var requiredUpdating = Members.UpdateMember(membershipUser, _provider,
                    contentItem.Email.Trim(),
                    contentItem.IsApproved,
                    comment: contentItem.Comments);

                if (requiredUpdating.Success)
                {
                    //re-map these values 
                    shouldReFetchMember = true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WarnWithException<MemberController>("Could not update member, the provider returned an error", ex);
                ModelState.AddPropertyError(
                    //specify 'default' just so that it shows up as a notification - is not assigned to a property
                    new ValidationResult("Could not update member, the provider returned an error: " + ex.Message + " (see log for full details)"), "default");
            }

            //if they were locked but now they are trying to be unlocked
            if (membershipUser.IsLockedOut && contentItem.IsLockedOut == false)
            {
                try
                {
                    var result = _provider.UnlockUser(membershipUser.UserName);
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
            if (contentItem.Password == null)
            {
                //If the provider has changed some values, these values need to be reflected in the member object 
                //that will get mapped to the display object
                if (shouldReFetchMember)
                {
                    RefetchMemberData(contentItem, LookupType.ByKey);
                    RestoreProvidedUserName(contentItem, providedUserName);
                }

                return null;
            }

            var passwordChangeResult = Members.ChangePassword(membershipUser.UserName, contentItem.Password, _provider);
            if (passwordChangeResult.Success)
            {
                //If the provider has changed some values, these values need to be reflected in the member object 
                //that will get mapped to the display object
                if (shouldReFetchMember)
                {
                    RefetchMemberData(contentItem, LookupType.ByKey);
                    RestoreProvidedUserName(contentItem, providedUserName);
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

        private enum LookupType
        {
            ByKey,
            ByUserName
        }

        /// <summary>
        /// Re-fetches the database data to map to the PersistedContent object and re-assigns the already mapped the posted properties so that the display object is up-to-date
        /// </summary>
        /// <param name="contentItem"></param>
        /// <remarks>
        /// This is done during an update if the membership provider has changed some underlying data - we need to ensure that our model is consistent with that data
        /// </remarks>
        private void RefetchMemberData(MemberSave contentItem, LookupType lookup)
        {
            var currProps = contentItem.PersistedContent.Properties.ToArray();

            switch (MembershipScenario)
            {
                case MembershipScenario.NativeUmbraco:
                    switch (lookup)
                    {
                        case LookupType.ByKey:
                            //Go and re-fetch the persisted item
                            contentItem.PersistedContent = Services.MemberService.GetByKey(contentItem.Key);
                            break;
                        case LookupType.ByUserName:
                            contentItem.PersistedContent = Services.MemberService.GetByUsername(contentItem.Username.Trim());
                            break;
                    }
                    break;
                case MembershipScenario.CustomProviderWithUmbracoLink:
                case MembershipScenario.StandaloneCustomProvider:
                default:
                    var membershipUser = _provider.GetUser(contentItem.Key, false);
                    //Go and re-fetch the persisted item
                    contentItem.PersistedContent = Mapper.Map<MembershipUser, IMember>(membershipUser);
                    break;
            }

            UpdateName(contentItem);

            //re-assign the mapped values that are not part of the membership provider properties.
            var builtInAliases = Constants.Conventions.Member.GetStandardPropertyTypeStubs().Select(x => x.Key).ToArray();
            foreach (var p in contentItem.PersistedContent.Properties)
            {
                var valueMapped = currProps.SingleOrDefault(x => x.Alias == p.Alias);
                if (builtInAliases.Contains(p.Alias) == false && valueMapped != null)
                {
                    p.Value = valueMapped.Value;
                    p.TagSupport.Behavior = valueMapped.TagSupport.Behavior;
                    p.TagSupport.Enable = valueMapped.TagSupport.Enable;
                    p.TagSupport.Tags = valueMapped.TagSupport.Tags;
                }
            }
        }

        /// <summary>
        /// Following a refresh of member data called during an update if the membership provider has changed some underlying data, 
        /// we don't want to lose the provided, and potentiallly changed, username
        /// </summary>
        /// <param name="contentItem"></param>
        /// <param name="providedUserName"></param>
        private static void RestoreProvidedUserName(MemberSave contentItem, string providedUserName)
        {
            contentItem.PersistedContent.Username = providedUserName;
        }

        /// <summary>
        /// This is going to create the user with the membership provider and check for validation
        /// </summary>
        /// <param name="contentItem"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        /// <remarks>
        /// Depending on if the Umbraco membership provider is active or not, the process differs slightly:
        /// 
        /// * If the umbraco membership provider is used - we create the membership user first with the membership provider, since 
        ///     it's the umbraco membership provider, this writes to the umbraco tables. When that is complete we re-fetch the IMember
        ///     model data from the db. In this case we don't care what the provider user key is.
        /// * If we're using a non-umbraco membership provider - we check if there is a 'Member' member type - if so 
        ///     we create an empty IMember instance first (of type 'Member'), this gives us a unique ID (GUID)
        ///     that we then use to create the member in the custom membership provider. This acts as the link between Umbraco data and 
        ///     the custom membership provider data. This gives us the ability to eventually have custom membership properties but still use
        ///     a custom memberhip provider. If there is no 'Member' member type, then we will simply just create the membership provider member
        ///     with no link to our data.
        /// 
        /// If this is successful, it will go and re-fetch the IMember from the db because it will now have an ID because the Umbraco provider 
        /// uses the umbraco data store - then of course we need to re-map it to the saved property values.
        /// </remarks>
        private MembershipUser CreateWithMembershipProvider(MemberSave contentItem, out MembershipCreateStatus status)
        {
            MembershipUser membershipUser;

            switch (MembershipScenario)
            {
                case MembershipScenario.NativeUmbraco:
                    //We are using the umbraco membership provider, create the member using the membership provider first.
                    var umbracoMembershipProvider = (UmbracoMembershipProviderBase)_provider;
                    //TODO: We are not supporting q/a - passing in empty here
                    membershipUser = umbracoMembershipProvider.CreateUser(
                        contentItem.ContentTypeAlias, contentItem.Username,
                        contentItem.Password.NewPassword,
                        contentItem.Email, "", "",
                        contentItem.IsApproved,
                        Guid.NewGuid(), //since it's the umbraco provider, the user key here doesn't make any difference
                        out status);

                    break;
                case MembershipScenario.CustomProviderWithUmbracoLink:
                    //We are using a custom membership provider, we'll create an empty IMember first to get the unique id to use
                    // as the provider user key.                    
                    //create it - this persisted item has already been set in the MemberBinder based on the 'Member' member type:
                    Services.MemberService.Save(contentItem.PersistedContent);

                    //TODO: We are not supporting q/a - passing in empty here
                    membershipUser = _provider.CreateUser(
                        contentItem.Username,
                        contentItem.Password.NewPassword,
                        contentItem.Email,
                        "TEMP", //some membership provider's require something here even if q/a is disabled!
                        "TEMP", //some membership provider's require something here even if q/a is disabled!
                        contentItem.IsApproved,
                        contentItem.PersistedContent.Key, //custom membership provider, we'll link that based on the IMember unique id (GUID)
                        out status);

                    break;
                case MembershipScenario.StandaloneCustomProvider:
                    // we don't have a member type to use so we will just create the basic membership user with the provider with no
                    // link back to the umbraco data

                    var newKey = Guid.NewGuid();
                    //TODO: We are not supporting q/a - passing in empty here
                    membershipUser = _provider.CreateUser(
                        contentItem.Username,
                        contentItem.Password.NewPassword,
                        contentItem.Email,
                        "TEMP", //some membership provider's require something here even if q/a is disabled!
                        "TEMP", //some membership provider's require something here even if q/a is disabled!
                        contentItem.IsApproved,
                        newKey,
                        out status);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //TODO: Localize these!
            switch (status)
            {
                case MembershipCreateStatus.Success:

                    //map the key back
                    contentItem.Key = membershipUser.ProviderUserKey.TryConvertTo<Guid>().Result;
                    contentItem.PersistedContent.Key = contentItem.Key;

                    //if the comments are there then we need to save them
                    if (contentItem.Comments.IsNullOrWhiteSpace() == false)
                    {
                        membershipUser.Comment = contentItem.Comments;
                        _provider.UpdateUser(membershipUser);
                    }

                    RefetchMemberData(contentItem, LookupType.ByUserName);

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
        /// 
        [HttpPost]
        public HttpResponseMessage DeleteByKey(Guid key)
        {
            IMember foundMember;
            MembershipUser foundMembershipUser;
            switch (MembershipScenario)
            {
                case MembershipScenario.NativeUmbraco:
                    foundMember = Services.MemberService.GetByKey(key);
                    if (foundMember == null)
                    {
                        return HandleContentNotFound(key, false);
                    }
                    Services.MemberService.Delete(foundMember);
                    break;
                case MembershipScenario.CustomProviderWithUmbracoLink:
                    foundMember = Services.MemberService.GetByKey(key);
                    if (foundMember != null)
                    {
                        Services.MemberService.Delete(foundMember);
                    }
                    foundMembershipUser = _provider.GetUser(key, false);
                    if (foundMembershipUser != null)
                    {
                        _provider.DeleteUser(foundMembershipUser.UserName, true);
                    }
                    break;
                case MembershipScenario.StandaloneCustomProvider:
                    foundMembershipUser = _provider.GetUser(key, false);
                    if (foundMembershipUser != null)
                    {
                        _provider.DeleteUser(foundMembershipUser.UserName, true);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
