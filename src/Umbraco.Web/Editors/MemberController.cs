using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using AutoMapper;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;
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

            UpdateName(contentItem);

            //map the custom properties
            contentItem.PersistedContent.Email = contentItem.Email;
            //TODO: If we allow changing the alias then we'll need to change URLs, etc... in the editor, would prefer to use 
            // a unique id but then need to figure out how to handle that with custom membership providers - waiting on feedback from morten.

            MapPropertyValues(contentItem);

            //We need to manually check the validation results here because:
            // * We still need to save the entity even if there are validation value errors
            // * Depending on if the entity is new, and if there are non property validation errors (i.e. the name is null)
            //      then we cannot continue saving, we can only display errors
            // * If there are validation errors and they were attempting to publish, we can only save, NOT publish and display 
            //      a message indicating this
            if (!ModelState.IsValid)
            {
                if (ValidationHelper.ModelHasRequiredForPersistenceErrors(contentItem)
                    && (contentItem.Action == ContentSaveAction.SaveNew))
                {
                    //ok, so the absolute mandatory data is invalid and it's new, we cannot actually continue!
                    // add the modelstate to the outgoing object and throw validation response
                    var forDisplay = Mapper.Map<IMember, MemberDisplay>(contentItem.PersistedContent);
                    forDisplay.Errors = ModelState.ToErrorDictionary();
                    throw new HttpResponseException(Request.CreateValidationErrorResponse(forDisplay));
                }
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
    }
}
