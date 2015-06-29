using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// This validates the submitted data in regards to the current membership provider
    /// </summary>
    internal class MembershipProviderValidationFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            //default provider!
            var membershipProvider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();
            
            var contentItem = (MemberSave) actionContext.ActionArguments["contentItem"];

            var validEmail = ValidateUniqueEmail(contentItem, membershipProvider, actionContext);
            if (validEmail == false)
            {
                actionContext.ModelState.AddPropertyError(
                    new ValidationResult("Email address is already in use", new[] {"value"}),
                    string.Format("{0}email", Constants.PropertyEditors.InternalGenericPropertiesPrefix));
            }

            var validLogin = ValidateUniqueLogin(contentItem, membershipProvider, actionContext);
            if (validLogin == false)
            {
                actionContext.ModelState.AddPropertyError(
                    new ValidationResult("Username is already in use", new[] { "value" }),
                    string.Format("{0}login", Constants.PropertyEditors.InternalGenericPropertiesPrefix));
            }
        }

        internal bool ValidateUniqueLogin(MemberSave contentItem, MembershipProvider membershipProvider, HttpActionContext actionContext)
        {
            if (contentItem == null) throw new ArgumentNullException("contentItem");
            if (membershipProvider == null) throw new ArgumentNullException("membershipProvider");

            int totalRecs;
            var existingByName = membershipProvider.FindUsersByName(contentItem.Username.Trim(), 0, int.MaxValue, out totalRecs);
            switch (contentItem.Action)
            {
                case ContentSaveAction.Save:

                    //ok, we're updating the member, we need to check if they are changing their login and if so, does it exist already ?
                    if (contentItem.PersistedContent.Username.InvariantEquals(contentItem.Username.Trim()) == false)
                    {
                        //they are changing their login name
                        if (existingByName.Cast<MembershipUser>().Select(x => x.UserName)
                            .Any(x => x == contentItem.Username.Trim()))
                        {
                            //the user cannot use this login
                            return false;
                        }
                    }
                    break;
                case ContentSaveAction.SaveNew:
                    //check if the user's login already exists
                    if (existingByName.Cast<MembershipUser>().Select(x => x.UserName)
                        .Any(x => x == contentItem.Username.Trim()))
                    {
                        //the user cannot use this login
                        return false;
                    }
                    break;
                default:
                    //we don't support this for members
                    throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return true;
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
                default:
                    //we don't support this for members
                    throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return true;
        }
    }
}