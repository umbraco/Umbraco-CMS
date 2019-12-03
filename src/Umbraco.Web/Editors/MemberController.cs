using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Web.WebApi;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using System.Collections.Generic;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Persistence;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.ContentApps;
using Umbraco.Web.Editors.Binders;
using Umbraco.Web.Editors.Filters;
using Constants = Umbraco.Core.Constants;
using Umbraco.Core.Dictionary;
using Umbraco.Web.Security;
using Umbraco.Core.Security;
using System.Threading.Tasks;

namespace Umbraco.Web.Editors
{
    /// <remarks>
    /// This controller is decorated with the UmbracoApplicationAuthorizeAttribute which means that any user requesting
    /// access to ALL of the methods on this controller will need access to the member application.
    /// </remarks>
    [PluginController("UmbracoApi")]
    [UmbracoApplicationAuthorize(Constants.Applications.Members)]
    [OutgoingNoHyphenGuidFormat]
    public class MemberController : ContentControllerBase
    {
        public MemberController(IMemberPasswordConfiguration passwordConfig, ICultureDictionary cultureDictionary, PropertyEditorCollection propertyEditors, IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper)
            : base(cultureDictionary, globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper)
        {
            _passwordConfig = passwordConfig ?? throw new ArgumentNullException(nameof(passwordConfig));
            _propertyEditors = propertyEditors ?? throw new ArgumentNullException(nameof(propertyEditors));
        }

        private readonly IMemberPasswordConfiguration _passwordConfig;
        private readonly PropertyEditorCollection _propertyEditors;
        private PasswordSecurity _passwordSecurity;
        private PasswordChanger _passwordChanger;
        private PasswordSecurity PasswordSecurity => _passwordSecurity ?? (_passwordSecurity = new PasswordSecurity(_passwordConfig));
        private PasswordChanger PasswordChanger => _passwordChanger ?? (_passwordChanger = new PasswordChanger(Logger));

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

            var members = Services.MemberService
                    .GetAll((pageNumber - 1), pageSize, out var totalRecords, orderBy, orderDirection, orderBySystemField, memberTypeAlias, filter).ToArray();
            if (totalRecords == 0)
            {
                return new PagedResult<MemberBasic>(0, 0, 0);
            }

            var pagedResult = new PagedResult<MemberBasic>(totalRecords, pageNumber, pageSize)
            {
                Items = members
                    .Select(x => Mapper.Map<MemberBasic>(x))
            };
            return pagedResult;
        }

        /// <summary>
        /// Returns a display node with a list view to render members
        /// </summary>
        /// <param name="listName"></param>
        /// <returns></returns>
        public MemberListDisplay GetListNodeDisplay(string listName)
        {
            var foundType = Services.MemberTypeService.Get(listName);
            var name = foundType != null ? foundType.Name : listName;

            var apps = new List<ContentApp>();
            apps.Add(ListViewContentAppFactory.CreateContentApp(Services.DataTypeService, _propertyEditors, listName, "member", Core.Constants.DataTypes.DefaultMembersListView));
            apps[0].Active = true;

            var display = new MemberListDisplay
            {
                ContentTypeAlias = listName,
                ContentTypeName = name,
                Id = listName,
                IsContainer = true,
                Name = listName == Constants.Conventions.MemberTypes.AllMembersListId ? "All Members" : name,
                Path = "-1," + listName,
                ParentId = -1,
                ContentApps = apps
            };

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
            var foundMember = Services.MemberService.GetByKey(key);
            if (foundMember == null)
            {
                HandleContentNotFound(key);
            }
            return Mapper.Map<MemberDisplay>(foundMember);
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
            if (contentTypeAlias == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var contentType = Services.MemberTypeService.Get(contentTypeAlias);
            if (contentType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var passwordGenerator = new PasswordGenerator(_passwordConfig);

            emptyContent = new Member(contentType);
            emptyContent.AdditionalData["NewPassword"] = passwordGenerator.GeneratePassword();
            return Mapper.Map<MemberDisplay>(emptyContent);
        }

        /// <summary>
        /// Saves member
        /// </summary>
        /// <returns></returns>
        [FileUploadCleanupFilter]
        [OutgoingEditorModelEvent]
        [MemberSaveValidation]
        public async Task<MemberDisplay> PostSave(
            [ModelBinder(typeof(MemberBinder))]
                MemberSave contentItem)
        {

            //If we've reached here it means:
            // * Our model has been bound
            // * and validated
            // * any file attachments have been saved to their temporary location for us to use
            // * we have a reference to the DTO object and the persisted object
            // * Permissions are valid

            //map the properties to the persisted entity
            MapPropertyValues(contentItem);

            await ValidateMemberDataAsync(contentItem);

            //Unlike content/media - if there are errors for a member, we do NOT proceed to save them, we cannot so return the errors
            if (ModelState.IsValid == false)
            {
                var forDisplay = Mapper.Map<MemberDisplay>(contentItem.PersistedContent);
                forDisplay.Errors = ModelState.ToErrorDictionary();
                throw new HttpResponseException(Request.CreateValidationErrorResponse(forDisplay));
            }

            //We're gonna look up the current roles now because the below code can cause
            // events to be raised and developers could be manually adding roles to members in
            // their handlers. If we don't look this up now there's a chance we'll just end up
            // removing the roles they've assigned.
            var currRoles = Services.MemberService.GetAllRoles(contentItem.PersistedContent.Username);
            //find the ones to remove and remove them
            var rolesToRemove = currRoles.Except(contentItem.Groups).ToArray();

            //Depending on the action we need to first do a create or update using the membership provider
            // this ensures that passwords are formatted correctly and also performs the validation on the provider itself.
            switch (contentItem.Action)
            {
                case ContentSaveAction.Save:
                    UpdateMemberData(contentItem);
                    break;
                case ContentSaveAction.SaveNew:
                    contentItem.PersistedContent = CreateMemberData(contentItem);
                    break;
                default:
                    //we don't support anything else for members
                    throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            //TODO: There's 3 things saved here and we should do this all in one transaction, which we can do here by wrapping in a scope
            // but it would be nicer to have this taken care of within the Save method itself

            //create/save the IMember
            Services.MemberService.Save(contentItem.PersistedContent);

            //Now let's do the role provider stuff - now that we've saved the content item (that is important since
            // if we are changing the username, it must be persisted before looking up the member roles).
            if (rolesToRemove.Any())
            {
                Services.MemberService.DissociateRoles(new[] { contentItem.PersistedContent.Username }, rolesToRemove);
            }
            //find the ones to add and add them
            var toAdd = contentItem.Groups.Except(currRoles).ToArray();
            if (toAdd.Any())
            {
                //add the ones submitted
                Services.MemberService.AssignRoles(new[] { contentItem.PersistedContent.Username }, toAdd);
            }

            //return the updated model
            var display = Mapper.Map<MemberDisplay>(contentItem.PersistedContent);

            //lastly, if it is not valid, add the model state to the outgoing object and throw a 403
            HandleInvalidModelState(display);

            var localizedTextService = Services.TextService;
            //put the correct messages in
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
            base.MapPropertyValuesForPersistence<IMember, MemberSave>(
                contentItem,
                contentItem.PropertyCollectionDto,
                (save, property) => property.GetValue(), //get prop val
                (save, property, v) => property.SetValue(v), //set prop val
                null); // member are all invariant
        }

        private IMember CreateMemberData(MemberSave contentItem)
        {
            var memberType = Services.MemberTypeService.Get(contentItem.ContentTypeAlias);
            if (memberType == null)
                throw new InvalidOperationException($"No member type found with alias {contentItem.ContentTypeAlias}");
            var member = new Member(contentItem.Name, contentItem.Email, contentItem.Username, memberType, true)
            {
                CreatorId = Security.CurrentUser.Id
            };

            return member;            
        }

        /// <summary>
        /// Update the member security data
        /// </summary>
        /// <param name="contentItem"></param>
        /// <returns>
        /// If the password has been reset then this method will return the reset/generated password, otherwise will return null.
        /// </returns>
        private void UpdateMemberData(MemberSave contentItem)
        {
            contentItem.PersistedContent.WriterId = Security.CurrentUser.Id;

            //if the user doesn't have access to sensitive values, then we need to check if any of the built in member property types
            //have been marked as sensitive. If that is the case we cannot change these persisted values no matter what value has been posted.
            //There's only 3 special ones we need to deal with that are part of the MemberSave instance
            if (!Security.CurrentUser.HasAccessToSensitiveData())
            {
                var memberType = Services.MemberTypeService.Get(contentItem.PersistedContent.ContentTypeId);
                var sensitiveProperties = memberType
                    .PropertyTypes.Where(x => memberType.IsSensitiveProperty(x.Alias))
                    .ToList();

                foreach (var sensitiveProperty in sensitiveProperties)
                {
                    //if found, change the value of the contentItem model to the persisted value so it remains unchanged
                    switch (sensitiveProperty.Alias)
                    {
                        case Constants.Conventions.Member.Comments:
                            contentItem.Comments = contentItem.PersistedContent.Comments;
                            break;
                        case Constants.Conventions.Member.IsApproved:
                            contentItem.IsApproved = contentItem.PersistedContent.IsApproved;
                            break;
                        case Constants.Conventions.Member.IsLockedOut:
                            contentItem.IsLockedOut = contentItem.PersistedContent.IsLockedOut;
                            break;
                    }
                }
            }

            //if they were locked but now they are trying to be unlocked
            if (contentItem.PersistedContent.IsLockedOut && contentItem.IsLockedOut == false)
            {
                contentItem.PersistedContent.IsLockedOut = false;
                contentItem.PersistedContent.FailedPasswordAttempts = 0;
            }
            else if (!contentItem.PersistedContent.IsLockedOut && contentItem.IsLockedOut)
            {
                //NOTE: This should not ever happen unless someone is mucking around with the request data.
                //An admin cannot simply lock a user, they get locked out by password attempts, but an admin can un-approve them
                ModelState.AddModelError("custom", "An admin cannot lock a user");
            }

            //no password changes then exit ?
            if (contentItem.Password == null)
                return;

            // change the password
            contentItem.PersistedContent.RawPasswordValue = PasswordChanger.ChangePassword(contentItem.Password, PasswordSecurity);
        }

        private static void UpdateName(MemberSave memberSave)
        {
            //Don't update the name if it is empty
            if (memberSave.Name.IsNullOrWhiteSpace() == false)
            {
                memberSave.PersistedContent.Name = memberSave.Name;
            }
        }

        // TODO: This logic should be pulled into the service layer
        private async Task<bool> ValidateMemberDataAsync(MemberSave contentItem)
        {
            if (contentItem.Name.IsNullOrWhiteSpace())
            {
                ModelState.AddPropertyError(
                        new ValidationResult("Invalid user name", new[] { "value" }),
                        string.Format("{0}login", Constants.PropertyEditors.InternalGenericPropertiesPrefix));
                return false;
            }

            if (contentItem.Password != null && !contentItem.Password.NewPassword.IsNullOrWhiteSpace())
            {
                var validPassword = await PasswordSecurity.IsValidPasswordAsync(contentItem.Password.NewPassword);
                if (!validPassword)
                {
                    ModelState.AddPropertyError(
                       new ValidationResult("Invalid password: " + string.Join(", ", validPassword.Result), new[] { "value" }),
                       string.Format("{0}password", Constants.PropertyEditors.InternalGenericPropertiesPrefix));
                    return false;
                }
            }

            var byUsername = Services.MemberService.GetByUsername(contentItem.Username);
            if (byUsername != null && byUsername.Key != contentItem.Key)
            {
                ModelState.AddPropertyError(
                        new ValidationResult("Username is already in use", new[] { "value" }),
                        string.Format("{0}login", Constants.PropertyEditors.InternalGenericPropertiesPrefix));
                return false;
            }

            var byEmail = Services.MemberService.GetByEmail(contentItem.Email);
            if (byEmail != null && byEmail.Key != contentItem.Key)
            {
                ModelState.AddPropertyError(
                        new ValidationResult("Email address is already in use", new[] { "value" }),
                        string.Format("{0}email", Constants.PropertyEditors.InternalGenericPropertiesPrefix));
                return false;
            }

            return true;
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
            var foundMember = Services.MemberService.GetByKey(key);
            if (foundMember == null)
            {
                return HandleContentNotFound(key, false);
            }
            Services.MemberService.Delete(foundMember);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Exports member data based on their unique Id
        /// </summary>
        /// <param name="key">The unique <see cref="Guid">member identifier</see></param>
        /// <returns><see cref="HttpResponseMessage"/></returns>
        [HttpGet]
        public HttpResponseMessage ExportMemberData(Guid key)
        {
            var currentUser = Security.CurrentUser;

            var httpResponseMessage = Request.CreateResponse();
            if (currentUser.HasAccessToSensitiveData() == false)
            {
                httpResponseMessage.StatusCode = HttpStatusCode.Forbidden;
                return httpResponseMessage;
            }

            var member = ((MemberService)Services.MemberService).ExportMember(key);

            var fileName = $"{member.Name}_{member.Email}.txt";

            httpResponseMessage.Content = new ObjectContent<MemberExportModel>(member, new JsonMediaTypeFormatter { Indent = true });
            httpResponseMessage.Content.Headers.Add("x-filename", fileName);
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            httpResponseMessage.Content.Headers.ContentDisposition.FileName = fileName;
            httpResponseMessage.StatusCode = HttpStatusCode.OK;

            return httpResponseMessage;
        }
    }


}
