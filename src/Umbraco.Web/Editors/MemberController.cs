using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;
using AutoMapper;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.WebApi;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Binders;
using Umbraco.Web.WebApi.Filters;
using umbraco;
using Constants = Umbraco.Core.Constants;
using Examine;
using System.Web.Security;
using Member = umbraco.cms.businesslogic.member.Member;

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
            if (Member.InUmbracoMemberMode())
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
            //If we've reached here it means:
            // * Our model has been bound
            // * and validated
            // * any file attachments have been saved to their temporary location for us to use
            // * we have a reference to the DTO object and the persisted object
            // * Permissions are valid

            UpdateName(contentItem);

            //map the custom properties - this will already be set for new entities in our member binder
            contentItem.PersistedContent.Email = contentItem.Email;
            contentItem.PersistedContent.Username = contentItem.Username;
            
            MapPropertyValues(contentItem);

            //Unlike content/media - if there are errors for a member, we do NOT proceed to save them, we cannot so return the errors
            if (!ModelState.IsValid)
            {                
                var forDisplay = Mapper.Map<IMember, MemberDisplay>(contentItem.PersistedContent);
                forDisplay.Errors = ModelState.ToErrorDictionary();
                throw new HttpResponseException(Request.CreateValidationErrorResponse(forDisplay));
            }

            //save the item
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

    /// <summary>
    /// This validates the submitted data in regards to the current membership provider
    /// </summary>
    internal class MembershipProviderValidationFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            var membershipProvider = Membership.Providers[Constants.Conventions.Member.UmbracoMemberProviderName];
            if (membershipProvider == null)
            {
                throw new InvalidOperationException("No membership provider found with name " + Constants.Conventions.Member.UmbracoMemberProviderName);
            }

            var contentItem = (MemberSave) actionContext.ActionArguments["contentItem"];

            var validEmail = ValidateUniqueEmail(contentItem, membershipProvider, actionContext);
            if (validEmail == false)
            {
                actionContext.ModelState.AddPropertyError(new ValidationResult("Email address is already in use"), "umb_email");
            }
        }

        internal bool ValidateUniqueEmail(MemberSave contentItem, MembershipProvider membershipProvider, HttpActionContext actionContext)
        {
            if (contentItem == null) throw new ArgumentNullException("contentItem");
            if (membershipProvider == null) throw new ArgumentNullException("membershipProvider");

            if (membershipProvider.RequiresUniqueEmail == false)
            {
                return true;
            }

            int totalRecs;
            var existingByEmail = membershipProvider.FindUsersByEmail(contentItem.Email.Trim(), 0, int.MaxValue, out totalRecs);            
            switch (contentItem.Action)
            {
                case ContentSaveAction.Save:
                    //ok, we're updating the member, we need to check if they are changing their email and if so, does it exist already ?
                    if (contentItem.PersistedContent.Email.InvariantEquals(contentItem.Email.Trim()) == false)
                    {
                        //they are changing their email
                        if (existingByEmail.Cast<MembershipUser>().Select(x => x.Email)
                            .Any(x => x.InvariantEquals(contentItem.Email.Trim())))
                        {
                            //the user cannot use this email
                            return false;
                        }
                    }
                    break;
                case ContentSaveAction.SaveNew:
                    //check if the user's email already exists
                    if (existingByEmail.Cast<MembershipUser>().Select(x => x.Email)
                        .Any(x => x.InvariantEquals(contentItem.Email.Trim())))
                    {
                        //the user cannot use this email
                        return false;
                    }
                    break;
                case ContentSaveAction.Publish:
                case ContentSaveAction.PublishNew:
                default:
                    //we don't support this for members
                    throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return true;
        }
    }
}
