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
using Umbraco.Web.Security;

namespace Umbraco.Web.Editors.Binders
{
    /// <summary>
    /// The model binder for <see cref="T:Umbraco.Web.Models.ContentEditing.MemberSave" />
    /// </summary>
    internal class MemberBinder : IModelBinder
    {
        private readonly ContentModelBinderHelper _modelBinderHelper;
        private readonly ServiceContext _services;

        public MemberBinder() : this(Current.Services)
        {
        }

        public MemberBinder(ServiceContext services)
        {
            _services = services;
            _modelBinderHelper = new ContentModelBinderHelper();
        }

        /// <summary>
        /// Creates the model from the request and binds it to the context
        /// </summary>
        /// <param name="actionContext"></param>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            var model = _modelBinderHelper.BindModelFromMultipartRequest<MemberSave>(actionContext, bindingContext);
            if (model == null) return false;

            model.PersistedContent = ContentControllerBase.IsCreatingAction(model.Action) ? CreateNew(model) : GetExisting(model);

            //create the dto from the persisted model
            if (model.PersistedContent != null)
            {
                model.PropertyCollectionDto = Current.Mapper.Map<IMember, ContentPropertyCollectionDto>(model.PersistedContent);
                //now map all of the saved values to the dto
                _modelBinderHelper.MapPropertyValuesFromSaved(model, model.PropertyCollectionDto);
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
            return GetExisting(model.Key);
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

        /// <summary>
        /// This will remove all of the special membership provider properties which were required to display the property editors
        /// for editing - but the values have been mapped back to the MemberSave object directly - we don't want to keep these properties
        /// on the IMember because they will attempt to be persisted which we don't want since they might not even exist.
        /// </summary>
        /// <param name="contentType"></param>
        private void FilterMembershipProviderProperties(IContentTypeBase contentType)
        {
            var defaultProps = ConventionsHelper.GetStandardPropertyTypeStubs();
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
