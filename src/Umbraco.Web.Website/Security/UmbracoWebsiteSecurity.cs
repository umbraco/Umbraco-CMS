using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Security;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Web.Models;

namespace Umbraco.Web.Website.Security
{
    public class UmbracoWebsiteSecurity : IUmbracoWebsiteSecurity
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemberService _memberService;
        private readonly IMemberTypeService _memberTypeService;
        private readonly IShortStringHelper _shortStringHelper;

        public UmbracoWebsiteSecurity(IHttpContextAccessor httpContextAccessor,
                                      IMemberService memberService,
                                      IMemberTypeService memberTypeService,
                                      IShortStringHelper shortStringHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _memberService = memberService;
            _memberTypeService = memberTypeService;
            _shortStringHelper = shortStringHelper;
        }

        /// <inheritdoc/>
        public RegisterModel CreateRegistrationModel(string memberTypeAlias = null)
        {
            var providedOrDefaultMemberTypeAlias = memberTypeAlias ?? Constants.Conventions.MemberTypes.DefaultAlias;
            var memberType = _memberTypeService.Get(providedOrDefaultMemberTypeAlias);
            if (memberType == null)
            {
                throw new InvalidOperationException($"Could not find a member type with alias: {providedOrDefaultMemberTypeAlias}.");
            }

            var model = RegisterModel.CreateModel();
            model.MemberTypeAlias = providedOrDefaultMemberTypeAlias;
            model.MemberProperties = GetMemberPropertiesViewModel(memberType);
            return model;
        }

        private List<UmbracoProperty> GetMemberPropertiesViewModel(IMemberType memberType, IMember member = null)
        {
            var viewProperties = new List<UmbracoProperty>();

            var builtIns = ConventionsHelper.GetStandardPropertyTypeStubs(_shortStringHelper).Select(x => x.Key).ToArray();

            var propertyTypes = memberType.PropertyTypes
                .Where(x => builtIns.Contains(x.Alias) == false && memberType.MemberCanEditProperty(x.Alias))
                .OrderBy(p => p.SortOrder);

            foreach (var prop in propertyTypes)
            {
                var value = string.Empty;
                if (member != null)
                {
                    var propValue = member.Properties[prop.Alias];
                    if (propValue != null && propValue.GetValue() != null)
                    {
                        value = propValue.GetValue().ToString();
                    }
                }

                var viewProperty = new UmbracoProperty
                {
                    Alias = prop.Alias,
                    Name = prop.Name,
                    Value = value
                };

                // TODO: Perhaps one day we'll ship with our own EditorTempates but for now developers
                // can just render their own.

                ////This is a rudimentary check to see what data template we should render
                //// if developers want to change the template they can do so dynamically in their views or controllers
                //// for a given property.
                ////These are the default built-in MVC template types: “Boolean”, “Decimal”, “EmailAddress”, “HiddenInput”, “HTML”, “Object”, “String”, “Text”, and “Url”
                //// by default we'll render a text box since we've defined that metadata on the UmbracoProperty.Value property directly.
                //if (prop.DataTypeId == new Guid(Constants.PropertyEditors.TrueFalse))
                //{
                //    viewProperty.EditorTemplate = "UmbracoBoolean";
                //}
                //else
                //{
                //    switch (prop.DataTypeDatabaseType)
                //    {
                //        case DataTypeDatabaseType.Integer:
                //            viewProperty.EditorTemplate = "Decimal";
                //            break;
                //        case DataTypeDatabaseType.Ntext:
                //            viewProperty.EditorTemplate = "Text";
                //            break;
                //        case DataTypeDatabaseType.Date:
                //        case DataTypeDatabaseType.Nvarchar:
                //            break;
                //    }
                //}

                viewProperties.Add(viewProperty);
            }

            return viewProperties;
        }

        public Task<RegisterMemberStatus> RegisterMemberAsync(RegisterModel model, bool logMemberIn = true)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<ProfileModel> GetCurrentMemberProfileModelAsync()
        {
            if (IsLoggedIn() == false)
            {
                return null;
            }

            var member = GetCurrentPersistedMember();

            // This shouldn't happen but will if the member is deleted in the back office while the member is trying
            // to use the front-end!
            if (member == null)
            {
                // Log them out since they've been removed
                await LogOutAsync();

                return null;
            }

            var model = new ProfileModel
            {
                Name = member.Name,
                MemberTypeAlias = member.ContentTypeAlias,

                // TODO: get ASP.NET Core Identity equiavlant of MemberShipUser in order to get common membership properties such as Email
                // and UserName (see MembershipProviderExtensions.GetCurrentUserName()for legacy membership provider implementation).

                //Email = membershipUser.Email,
                //UserName = membershipUser.UserName,
                //Comment = membershipUser.Comment,
                //IsApproved = membershipUser.IsApproved,
                //IsLockedOut = membershipUser.IsLockedOut,
                //LastLockoutDate = membershipUser.LastLockoutDate,
                //CreationDate = membershipUser.CreationDate,
                //LastLoginDate = membershipUser.LastLoginDate,
                //LastActivityDate = membershipUser.LastActivityDate,
                //LastPasswordChangedDate = membershipUser.LastPasswordChangedDate
            };

            var memberType = _memberTypeService.Get(member.ContentTypeId);

            model.MemberProperties = GetMemberPropertiesViewModel(memberType, member);

            return model;
        }

        /// <inheritdoc/>
        public Task<UpdateMemberProfileResult> UpdateMemberProfileAsync(ProfileModel model)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool IsLoggedIn()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.User != null && httpContext.User.Identity.IsAuthenticated;
        }

        /// <inheritdoc/>
        public async Task<LoginStatusModel> GetCurrentLoginStatusAsync()
        {
            var model = LoginStatusModel.CreateModel();

            if (IsLoggedIn() == false)
            {
                model.IsLoggedIn = false;
                return model;
            }

            var member = GetCurrentPersistedMember();

            // This shouldn't happen but will if the member is deleted in the back office while the member is trying
            // to use the front-end!
            if (member == null)
            {
                // Log them out since they've been removed.
                await LogOutAsync();
                model.IsLoggedIn = false;
                return model;
            }

            model.Name = member.Name;
            model.Username = member.Username;
            model.Email = member.Email;
            model.IsLoggedIn = true;

            return model;
        }

        /// <summary>
        /// Returns the currently logged in IMember object - this should never be exposed to the front-end since it's returning a business logic entity!
        /// </summary>
        /// <returns></returns>
        private IMember GetCurrentPersistedMember()
        {
            // TODO: get user name from ASP.NET Core Identity (see MembershipProviderExtensions.GetCurrentUserName()
            // for legacy membership provider implementation).
            var username = "";

            // The result of this is cached by the MemberRepository
            return _memberService.GetByUsername(username);
        }

        /// <inheritdoc/>
        public Task<bool> LoginAsync(string username, string password)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task LogOutAsync()
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        /// <inheritdoc/>
        public bool IsMemberAuthorized(IEnumerable<string> allowTypes = null, IEnumerable<string> allowGroups = null, IEnumerable<int> allowMembers = null)
        {
            throw new NotImplementedException();
        }
    }
}
