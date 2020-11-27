using System;
using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using System.Linq;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services.Implement;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Editors.Binders
{
    /// <summary>
    /// The model binder for <see cref="T:Umbraco.Web.Models.ContentEditing.MemberSave" />
    /// </summary>
    internal class MemberBinder : IModelBinder
    {
        private readonly ServiceContext _services;

        public MemberBinder() : this(Current.Services)
        {
        }

        public MemberBinder(ServiceContext services)
        {
            _services = services;
        }

        /// <summary>
        /// Creates the model from the request and binds it to the context
        /// </summary>
        /// <param name="actionContext"></param>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            var model = ContentModelBinderHelper.BindModelFromMultipartRequest<MemberSave>(actionContext, bindingContext);
            if (model == null) return false;

            model.PersistedContent = ContentControllerBase.IsCreatingAction(model.Action) ? CreateNew(model) : GetExisting(model);

            //create the dto from the persisted model
            if (model.PersistedContent != null)
            {
                model.PropertyCollectionDto = Current.Mapper.Map<IMember, ContentPropertyCollectionDto>(model.PersistedContent);
                //now map all of the saved values to the dto
                ContentModelBinderHelper.MapPropertyValuesFromSaved(model, model.PropertyCollectionDto);
            }

            model.Name = model.Name.Trim();

            return true;
        }

        /// <summary>
        /// Returns an IMember instance used to bind values to and save (depending on the membership scenario)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private IMember GetExisting(MemberSave model)
        {
            var scenario = _services.MemberService.GetMembershipScenario();
            var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();
            switch (scenario)
            {
                case MembershipScenario.NativeUmbraco:
                    return GetExisting(model.Key);
                case MembershipScenario.CustomProviderWithUmbracoLink:
                case MembershipScenario.StandaloneCustomProvider:
                default:
                    var membershipUser = provider.GetUser(model.Key, false);
                    if (membershipUser == null)
                    {
                        throw new InvalidOperationException("Could not find member with key " + model.Key);
                    }

                    // TODO: Support this scenario!
                    //if (scenario == MembershipScenario.CustomProviderWithUmbracoLink)
                    //{
                    //    //if there's a 'Member' type then we should be able to just go get it from the db since it was created with a link
                    //    // to our data.
                    //    var memberType = ApplicationContext.Services.MemberTypeService.GetMemberType(Constants.Conventions.MemberTypes.Member);
                    //    if (memberType != null)
                    //    {
                    //        var existing = GetExisting(model.Key);
                    //        FilterContentTypeProperties(existing.ContentType, existing.ContentType.PropertyTypes.Select(x => x.Alias).ToArray());
                    //    }
                    //}

                    //generate a member for a generic membership provider
                    //NOTE: We don't care about the password here, so just generate something
                    //var member = MemberService.CreateGenericMembershipProviderMember(model.Name, model.Email, model.Username, Guid.NewGuid().ToString("N"));

                    //var convertResult = membershipUser.ProviderUserKey.TryConvertTo<Guid>();
                    //if (convertResult.Success == false)
                    //{
                    //    throw new InvalidOperationException("Only membership providers that store a GUID as their ProviderUserKey are supported" + model.Key);
                    //}
                    //member.Key = convertResult.Result;

                    var member = Current.Mapper.Map<MembershipUser, IMember>(membershipUser);

                    return member;
            }
        }

        private IMember GetExisting(Guid key)
        {
            var member = _services.MemberService.GetByKey(key);
            if (member == null)
            {
                throw new InvalidOperationException("Could not find member with key " + key);
            }

            return member;
        }

        /// <summary>
        /// Gets an instance of IMember used when creating a member
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// Depending on whether a custom membership provider is configured this will return different results.
        /// </remarks>
        private IMember CreateNew(MemberSave model)
        {
            var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();

            if (provider.IsUmbracoMembershipProvider())
            {
                var contentType = _services.MemberTypeService.Get(model.ContentTypeAlias);
                if (contentType == null)
                {
                    throw new InvalidOperationException("No member type found with alias " + model.ContentTypeAlias);
                }

                //remove all membership properties, these values are set with the membership provider.
                FilterMembershipProviderProperties(contentType);

                //return the new member with the details filled in
                return new Member(model.Name, model.Email, model.Username, model.Password.NewPassword, contentType);
            }
            else
            {
                //A custom membership provider is configured

                //NOTE: Below we are assigning the password to just a new GUID because we are not actually storing the password, however that
                // field is mandatory in the database so we need to put something there.

                //If the default Member type exists, we'll use that to create the IMember - that way we can associate the custom membership
                // provider to our data - eventually we can support editing custom properties with a custom provider.
                var memberType = _services.MemberTypeService.Get(Constants.Conventions.MemberTypes.DefaultAlias);
                if (memberType != null)
                {
                    FilterContentTypeProperties(memberType, memberType.PropertyTypes.Select(x => x.Alias).ToArray());
                    return new Member(model.Name, model.Email, model.Username, Guid.NewGuid().ToString("N"), memberType);
                }

                //generate a member for a generic membership provider
                var member = MemberService.CreateGenericMembershipProviderMember(model.Name, model.Email, model.Username, Guid.NewGuid().ToString("N"));
                //we'll just remove all properties here otherwise we'll end up with validation errors, we don't want to persist any property data anyways
                // in this case.
                memberType = _services.MemberTypeService.Get(member.ContentTypeId);
                FilterContentTypeProperties(memberType, memberType.PropertyTypes.Select(x => x.Alias).ToArray());
                return member;
            }
        }

        /// <summary>
        /// This will remove all of the special membership provider properties which were required to display the property editors
        /// for editing - but the values have been mapped back to the MemberSave object directly - we don't want to keep these properties
        /// on the IMember because they will attempt to be persisted which we don't want since they might not even exist.
        /// </summary>
        /// <param name="contentType"></param>
        private void FilterMembershipProviderProperties(IContentTypeBase contentType)
        {
            var defaultProps = Constants.Conventions.Member.GetStandardPropertyTypeStubs();
            //remove all membership properties, these values are set with the membership provider.
            var exclude = defaultProps.Select(x => x.Value.Alias).ToArray();
            FilterContentTypeProperties(contentType, exclude);
        }

        private void FilterContentTypeProperties(IContentTypeBase contentType, IEnumerable<string> exclude)
        {
            //remove all properties based on the exclusion list
            foreach (var remove in exclude)
            {
                if (contentType.PropertyTypeExists(remove))
                {
                    contentType.RemovePropertyType(remove);
                }
            }
        }
        
    }
}
