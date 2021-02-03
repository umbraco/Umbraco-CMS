using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Events;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Security;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Extensions;
using Umbraco.Infrastructure.Security;
using Umbraco.Infrastructure.Services.Implement;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.BackOffice.ModelBinders;
using Umbraco.Web.Common.ActionsResults;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Authorization;
using Umbraco.Web.Common.Filters;
using Umbraco.Web.ContentApps;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.BackOffice.Controllers
{
    /// <remarks>
    /// This controller is decorated with the UmbracoApplicationAuthorizeAttribute which means that any user requesting
    /// access to ALL of the methods on this controller will need access to the member application.
    /// </remarks>
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [Authorize(Policy = AuthorizationPolicies.SectionAccessMembers)]
    [OutgoingNoHyphenGuidFormat]
    public class MemberController : ContentControllerBase
    {
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly UmbracoMapper _umbracoMapper;
        private readonly IMemberService _memberService;
        private readonly IMemberTypeService _memberTypeService;
        private readonly IMembersUserManager _memberManager;
        private readonly IDataTypeService _dataTypeService;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IShortStringHelper _shortStringHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberController"/> class.
        /// </summary>
        /// <param name="cultureDictionary">The culture dictionary</param>
        /// <param name="loggerFactory">The logger factory</param>
        /// <param name="shortStringHelper">The string helper</param>
        /// <param name="eventMessages">The event messages factory</param>
        /// <param name="localizedTextService">The entry point for localizing key services</param>
        /// <param name="propertyEditors">The property editors</param>
        /// <param name="umbracoMapper">The mapper</param>
        /// <param name="memberService">The member service</param>
        /// <param name="memberTypeService">The member type service</param>
        /// <param name="memberManager">The member manager</param>
        /// <param name="dataTypeService">The data-type service</param>
        /// <param name="backOfficeSecurityAccessor">The back office security accessor</param>
        /// <param name="jsonSerializer">The JSON serializer</param>
        public MemberController(
            ICultureDictionary cultureDictionary,
            ILoggerFactory loggerFactory,
            IShortStringHelper shortStringHelper,
            IEventMessagesFactory eventMessages,
            ILocalizedTextService localizedTextService,
            PropertyEditorCollection propertyEditors,
            UmbracoMapper umbracoMapper,
            IMemberService memberService,
            IMemberTypeService memberTypeService,
            IMembersUserManager memberManager,
            IDataTypeService dataTypeService,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            IJsonSerializer jsonSerializer)
            : base(cultureDictionary, loggerFactory, shortStringHelper, eventMessages, localizedTextService, jsonSerializer)
        {
            _propertyEditors = propertyEditors;
            _umbracoMapper = umbracoMapper;
            _memberService = memberService;
            _memberTypeService = memberTypeService;
            _memberManager = memberManager;
            _dataTypeService = dataTypeService;
            _localizedTextService = localizedTextService;
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
            _jsonSerializer = jsonSerializer;
            _shortStringHelper = shortStringHelper;
        }

        /// <summary>
        /// The paginated list of members
        /// </summary>
        /// <param name="pageNumber">The page number to display</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="orderBy">The ordering of the member list</param>
        /// <param name="orderDirection">The direction of the member list</param>
        /// <param name="orderBySystemField">The system field to order by</param>
        /// <param name="filter">The current filter for the list</param>
        /// <param name="memberTypeAlias">The member type</param>
        /// <returns>The paged result of members</returns>
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

            IMember[] members = _memberService.GetAll(
                pageNumber - 1,
                pageSize,
                out var totalRecords,
                orderBy,
                orderDirection,
                orderBySystemField,
                memberTypeAlias,
                filter).ToArray();
            if (totalRecords == 0)
            {
                return new PagedResult<MemberBasic>(0, 0, 0);
            }

            var pagedResult = new PagedResult<MemberBasic>(totalRecords, pageNumber, pageSize)
            {
                Items = members.Select(x => _umbracoMapper.Map<MemberBasic>(x))
            };
            return pagedResult;
        }

        /// <summary>
        /// Returns a display node with a list view to render members
        /// </summary>
        /// <param name="listName">The member type to list</param>
        /// <returns>The member list for display</returns>
        public MemberListDisplay GetListNodeDisplay(string listName)
        {
            IMemberType foundType = _memberTypeService.Get(listName);
            var name = foundType != null ? foundType.Name : listName;

            var apps = new List<ContentApp>
            {
                ListViewContentAppFactory.CreateContentApp(
                    _dataTypeService,
                    _propertyEditors,
                    listName,
                    Constants.Security.DefaultMemberTypeAlias.ToLower(),
                    Constants.DataTypes.DefaultMembersListView)
            };
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
        /// <param name="key">The Guid key of the member</param>
        /// <returns>The member for display</returns>
        [OutgoingEditorModelEvent]
        public MemberDisplay GetByKey(Guid key)
        {
            IMember foundMember = _memberService.GetByKey(key);
            if (foundMember == null)
            {
                HandleContentNotFound(key);
            }

            return _umbracoMapper.Map<MemberDisplay>(foundMember);
        }

        /// <summary>
        /// Gets an empty content item for the
        /// </summary>
        /// <param name="contentTypeAlias">The content type</param>
        /// <returns>The empty member for display</returns>
        [OutgoingEditorModelEvent]
        public ActionResult<MemberDisplay> GetEmpty(string contentTypeAlias = null)
        {
            if (contentTypeAlias == null)
            {
                return NotFound();
            }

            IMemberType contentType = _memberTypeService.Get(contentTypeAlias);
            if (contentType == null)
            {
                return NotFound();
            }

            string newPassword = _memberManager.GeneratePassword();

            IMember emptyContent = new Member(contentType);
            emptyContent.AdditionalData["NewPassword"] = newPassword;
            return _umbracoMapper.Map<MemberDisplay>(emptyContent);
        }

        /// <summary>
        /// Saves member
        /// </summary>
        /// <param name="contentItem">The content item to save as a member</param>
        /// <returns>The resulting member display object</returns>
        [FileUploadCleanupFilter]
        [OutgoingEditorModelEvent]
        [MemberSaveValidation]
        public async Task<ActionResult<MemberDisplay>> PostSave([ModelBinder(typeof(MemberBinder))] MemberSave contentItem)
        {
            if (contentItem == null)
            {
                throw new ArgumentNullException("The member content item was null");
            }

            // If we've reached here it means:
            // * Our model has been bound
            // * and validated
            // * any file attachments have been saved to their temporary location for us to use
            // * we have a reference to the DTO object and the persisted object
            // * Permissions are valid

            // map the properties to the persisted entity
            MapPropertyValues(contentItem);

            await ValidateMemberDataAsync(contentItem);

            // Unlike content/media - if there are errors for a member, we do NOT proceed to save them, we cannot so return the errors
            if (ModelState.IsValid == false)
            {
                MemberDisplay forDisplay = _umbracoMapper.Map<MemberDisplay>(contentItem.PersistedContent);
                forDisplay.Errors = ModelState.ToErrorDictionary();
                return new ValidationErrorResult(forDisplay);
            }

            // Depending on the action we need to first do a create or update using the membership manager
            // this ensures that passwords are formatted correctly and also performs the validation on the provider itself.

            switch (contentItem.Action)
            {
                case ContentSaveAction.Save:
                    Task<ActionResult<bool>> updateSuccessful = UpdateMemberAsync(contentItem);
                    break;
                case ContentSaveAction.SaveNew:
                    Task<ActionResult<bool>> createSuccessful = CreateMemberAsync(contentItem);
                    break;
                default:
                    // we don't support anything else for members
                    return NotFound();
            }

            // TODO: There's 3 things saved here and we should do this all in one transaction, which we can do here by wrapping in a scope
            // but it would be nicer to have this taken care of within the Save method itself

            // return the updated model
            MemberDisplay display = _umbracoMapper.Map<MemberDisplay>(contentItem.PersistedContent);

            // lastly, if it is not valid, add the model state to the outgoing object and throw a 403
            if (!ModelState.IsValid)
            {
                display.Errors = ModelState.ToErrorDictionary();
                return new ValidationErrorResult(display, StatusCodes.Status403Forbidden);
            }

            ILocalizedTextService localizedTextService = _localizedTextService;

            // put the correct messages in
            switch (contentItem.Action)
            {
                case ContentSaveAction.Save:
                case ContentSaveAction.SaveNew:
                    display.AddSuccessNotification(
                        localizedTextService.Localize("speechBubbles/editMemberSaved"),
                        localizedTextService.Localize("speechBubbles/editMemberSaved"));
                    break;
            }

            return display;
        }

        /// <summary>
        /// Maps the property values to the persisted entity
        /// </summary>
        /// <param name="contentItem">The member content item to map properties from</param>
        private void MapPropertyValues(MemberSave contentItem)
        {
            // Don't update the name if it is empty
            if (contentItem.Name.IsNullOrWhiteSpace() == false)
            {
                contentItem.PersistedContent.Name = contentItem.Name;
            }

            // map the custom properties - this will already be set for new entities in our member binder
            contentItem.PersistedContent.Email = contentItem.Email;
            contentItem.PersistedContent.Username = contentItem.Username;

            // use the base method to map the rest of the properties
            MapPropertyValuesForPersistence<IMember, MemberSave>(
                contentItem,
                contentItem.PropertyCollectionDto,
                (save, property) => property.GetValue(), // get prop val
                (save, property, v) => property.SetValue(v), // set prop val
                null); // member are all invariant
        }

        /// <summary>
        /// Create a member from the supplied member content data
        /// 
        /// All member password processing and creation is done via the identity manager
        /// </summary>
        /// <param name="contentItem">Member content data</param>
        /// <returns>The identity result of the created member</returns>
        private async Task<ActionResult<bool>> CreateMemberAsync(MemberSave contentItem)
        {
            IMemberType memberType = _memberTypeService.Get(contentItem.ContentTypeAlias);
            if (memberType == null)
            {
                throw new InvalidOperationException($"No member type found with alias {contentItem.ContentTypeAlias}");
            }

            var identityMember = MembersIdentityUser.CreateNew(
                contentItem.Username,
                contentItem.Email,
                memberType.Alias,
                contentItem.Name);

            IdentityResult created = await _memberManager.CreateAsync(identityMember, contentItem.Password.NewPassword);

            if (created.Succeeded == false)
            {
                return new ValidationErrorResult(created.Errors.ToErrorMessage());
            }

            // now re-look up the member, which will now exist
            IMember member = _memberService.GetByEmail(contentItem.Email);

            // map the save info over onto the user
            member = _umbracoMapper.Map<MemberSave, IMember>(contentItem, member);

            int creatorId = _backOfficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id;
            member.CreatorId = creatorId;

            // assign the mapped property values that are not part of the identity properties
            string[] builtInAliases = ConventionsHelper.GetStandardPropertyTypeStubs(_shortStringHelper).Select(x => x.Key).ToArray();
            foreach (ContentPropertyBasic property in contentItem.Properties)
            {
                if (builtInAliases.Contains(property.Alias) == false)
                {
                    member.Properties[property.Alias].SetValue(property.Value);
                }
            }

            //TODO: do we need to resave the key?
            //contentItem.PersistedContent.Key = contentItem.Key;

            // now the member has been saved via identity, resave the member with mapped content properties
            _memberService.Save(member);
            contentItem.PersistedContent = member;

            await AddOrUpdateRoles(contentItem, identityMember);
            return true;
        }

        /// <summary>
        /// Update the member security data
        /// If the password has been reset then this method will return the reset/generated password, otherwise will return null.
        /// </summary>
        /// <param name="contentItem">The member to save</param>
        private async Task<ActionResult<bool>> UpdateMemberAsync(MemberSave contentItem)
        {
            contentItem.PersistedContent.WriterId = _backOfficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id;

            // If the user doesn't have access to sensitive values, then we need to check if any of the built in member property types
            // have been marked as sensitive. If that is the case we cannot change these persisted values no matter what value has been posted.
            // There's only 3 special ones we need to deal with that are part of the MemberSave instance: Comments, IsApproved, IsLockedOut
            // but we will take care of this in a generic way below so that it works for all props.
            if (!_backOfficeSecurityAccessor.BackOfficeSecurity.CurrentUser.HasAccessToSensitiveData())
            {
                IMemberType memberType = _memberTypeService.Get(contentItem.PersistedContent.ContentTypeId);
                var sensitiveProperties = memberType
                    .PropertyTypes.Where(x => memberType.IsSensitiveProperty(x.Alias))
                    .ToList();

                foreach (IPropertyType sensitiveProperty in sensitiveProperties)
                {
                    ContentPropertyBasic destProp = contentItem.Properties.FirstOrDefault(x => x.Alias == sensitiveProperty.Alias);
                    if (destProp != null)
                    {
                        // if found, change the value of the contentItem model to the persisted value so it remains unchanged
                        object origValue = contentItem.PersistedContent.GetValue(sensitiveProperty.Alias);
                        destProp.Value = origValue;
                    }
                }
            }

            bool isLockedOut = contentItem.IsLockedOut;

            // if they were locked but now they are trying to be unlocked
            if (contentItem.PersistedContent.IsLockedOut && isLockedOut == false)
            {
                contentItem.PersistedContent.IsLockedOut = false;
                contentItem.PersistedContent.FailedPasswordAttempts = 0;
            }
            else if (!contentItem.PersistedContent.IsLockedOut && isLockedOut)
            {
                // NOTE: This should not ever happen unless someone is mucking around with the request data.
                // An admin cannot simply lock a user, they get locked out by password attempts, but an admin can un-approve them
                ModelState.AddModelError("custom", "An admin cannot lock a user");
            }

            MembersIdentityUser identityMember = await _memberManager.FindByIdAsync(contentItem.Id.ToString());
            if (identityMember == null)
            {
                return new ValidationErrorResult("Member was not found");
            }

            if (contentItem.Password != null)
            {
                IdentityResult validatePassword = await _memberManager.ValidatePasswordAsync(contentItem.Password.NewPassword);
                if (validatePassword.Succeeded == false)
                {
                    return new ValidationErrorResult(validatePassword.Errors.ToErrorMessage());
                }

                string newPassword = _memberManager.HashPassword(contentItem.Password.NewPassword);
                identityMember.PasswordHash = newPassword;
                contentItem.PersistedContent.RawPasswordValue = identityMember.PasswordHash;
                if (identityMember.LastPasswordChangeDateUtc != null)
                {
                    contentItem.PersistedContent.LastPasswordChangeDate = DateTime.UtcNow;
                    identityMember.LastPasswordChangeDateUtc = contentItem.PersistedContent.LastPasswordChangeDate;
                }
            }

            IdentityResult updatedResult = await _memberManager.UpdateAsync(identityMember);

            if (updatedResult.Succeeded == false)
            {
                return new ValidationErrorResult(updatedResult.Errors.ToErrorMessage());
            }

            _memberService.Save(contentItem.PersistedContent);

            await AddOrUpdateRoles(contentItem, identityMember);
            return true;
        }

        private async Task<bool> ValidateMemberDataAsync(MemberSave contentItem)
        {
            if (contentItem.Name.IsNullOrWhiteSpace())
            {
                ModelState.AddPropertyError(
                    new ValidationResult("Invalid user name", new[] { "value" }),
                    $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}login");
                return false;
            }

            if (contentItem.Password != null && !contentItem.Password.NewPassword.IsNullOrWhiteSpace())
            {
                IdentityResult validPassword = await _memberManager.ValidatePasswordAsync(contentItem.Password.NewPassword);
                if (!validPassword.Succeeded)
                {
                    ModelState.AddPropertyError(
                       new ValidationResult("Invalid password: " + MapErrors(validPassword.Errors), new[] { "value" }),
                       $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}password");
                    return false;
                }
            }

            IMember byUsername = _memberService.GetByUsername(contentItem.Username);
            if (byUsername != null && byUsername.Key != contentItem.Key)
            {
                ModelState.AddPropertyError(
                    new ValidationResult("Username is already in use", new[] { "value" }),
                    $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}login");
                return false;
            }

            IMember byEmail = _memberService.GetByEmail(contentItem.Email);
            if (byEmail != null && byEmail.Key != contentItem.Key)
            {
                ModelState.AddPropertyError(
                    new ValidationResult("Email address is already in use", new[] { "value" }),
                    $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}email");
                return false;
            }

            return true;
        }

        private string MapErrors(IEnumerable<IdentityError> result)
        {
            var sb = new StringBuilder();
            IEnumerable<IdentityError> identityErrors = result.ToList();
            foreach (IdentityError error in identityErrors)
            {
                string errorString = $"{error.Description}";
                sb.AppendLine(errorString);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Add or update the identity roles
        /// </summary>
        /// <param name="contentItem">The member content item</param>
        /// <param name="identityMember">The member as an identity user</param>
        private async Task AddOrUpdateRoles(MemberSave contentItem, MembersIdentityUser identityMember)
        {
            // We're gonna look up the current roles now because the below code can cause
            // events to be raised and developers could be manually adding roles to members in
            // their handlers. If we don't look this up now there's a chance we'll just end up
            // removing the roles they've assigned.
            IEnumerable<string> currentRoles = await _memberManager.GetRolesAsync(identityMember);

            // find the ones to remove and remove them
            IEnumerable<string> roles = currentRoles.ToList();
            string[] rolesToRemove = roles.Except(contentItem.Groups).ToArray();

            // Now let's do the role provider stuff - now that we've saved the content item (that is important since
            // if we are changing the username, it must be persisted before looking up the member roles).
            if (rolesToRemove.Any())
            {
                IdentityResult rolesIdentityResult = await _memberManager.RemoveFromRolesAsync(identityMember, rolesToRemove);
            }

            // find the ones to add and add them
            string[] toAdd = contentItem.Groups.Except(roles).ToArray();
            if (toAdd.Any())
            {
                // add the ones submitted
                IdentityResult identityResult = await _memberManager.AddToRolesAsync(identityMember, toAdd);
            }
        }

        /// <summary>
        /// Permanently deletes a member
        /// </summary>
        /// <param name="key">Guid of the member to delete</param>
        /// <returns>The result of the deletion</returns>
        ///
        [HttpPost]
        public IActionResult DeleteByKey(Guid key)
        {
            //TODO: move to MembersUserStore
            IMember foundMember = _memberService.GetByKey(key);
            if (foundMember == null)
            {
                return HandleContentNotFound(key);
            }

            _memberService.Delete(foundMember);

            return Ok();
        }

        /// <summary>
        /// Exports member data based on their unique Id
        /// </summary>
        /// <param name="key">The unique <see cref="Guid">member identifier</see></param>
        /// <returns><see cref="HttpResponseMessage"/></returns>
        [HttpGet]
        public IActionResult ExportMemberData(Guid key)
        {
            IUser currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity.CurrentUser;

            if (currentUser.HasAccessToSensitiveData() == false)
            {
                return Forbid();
            }

            MemberExportModel member = ((MemberService)_memberService).ExportMember(key);
            if (member is null)
            {
                throw new NullReferenceException("No member found with key " + key);
            }

            var json = _jsonSerializer.Serialize(member);

            var fileName = $"{member.Name}_{member.Email}.txt";

            // Set custom header so umbRequestHelper.downloadFile can save the correct filename
            HttpContext.Response.Headers.Add("x-filename", fileName);

            return File(Encoding.UTF8.GetBytes(json), MediaTypeNames.Application.Octet, fileName);
        }
    }
}
