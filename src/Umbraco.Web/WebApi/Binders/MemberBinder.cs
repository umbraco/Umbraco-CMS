using System;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Security;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.WebApi.Filters;
using System.Linq;

namespace Umbraco.Web.WebApi.Binders
{
    internal class MemberBinder : ContentItemBaseBinder<IMember, MemberSave>
    {
        public MemberBinder(ApplicationContext applicationContext)
            : base(applicationContext)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MemberBinder()
            : this(ApplicationContext.Current)
        {
        }

        protected override ContentItemValidationHelper<IMember, MemberSave> GetValidationHelper()
        {
            return new MemberValidationHelper();
        }

        protected override IMember GetExisting(MemberSave model)
        {
            var member = ApplicationContext.Services.MemberService.GetByKey(model.Key);
            if (member == null)
            {
                throw new InvalidOperationException("Could not find member with key " + model.Key);
            }

            //remove all membership properties, these values are set with the membership provider.
            var exclude = Constants.Conventions.Member.StandardPropertyTypeStubs.Select(x => x.Value.Alias).ToArray();

            foreach (var remove in exclude)
            {
                member.Properties.Remove(remove);
            }
            return member;
        }

        protected override IMember CreateNew(MemberSave model)
        {
            var contentType = ApplicationContext.Services.MemberTypeService.GetMemberType(model.ContentTypeAlias);
            if (contentType == null)
            {
                throw new InvalidOperationException("No member type found wth alias " + model.ContentTypeAlias);
            }

            //remove all membership properties, these values are set with the membership provider.
            var exclude = Constants.Conventions.Member.StandardPropertyTypeStubs.Select(x => x.Value.Alias).ToArray();
            foreach (var remove in exclude)
            {
                if (contentType.PropertyTypeExists(remove))
                {
                    contentType.RemovePropertyType(remove);    
                }
            }

            //return the new member with the details filled in
            return new Member(model.Name, model.Email, model.Username, model.Password.NewPassword, -1, contentType);
        }

        protected override ContentItemDto<IMember> MapFromPersisted(MemberSave model)
        {
            return Mapper.Map<IMember, ContentItemDto<IMember>>(model.PersistedContent);
        }

        /// <summary>
        /// Custom validation helper so that we can exclude the Member.StandardPropertyTypeStubs from being validating for existence
        /// </summary>
        internal class MemberValidationHelper : ContentItemValidationHelper<IMember, MemberSave>
        {
            protected override bool ValidateProperties(ContentItemBasic<ContentPropertyBasic, IMember> postedItem, HttpActionContext actionContext)
            {
                var propertiesToValidate = postedItem.Properties.ToList();
                var exclude = Constants.Conventions.Member.StandardPropertyTypeStubs.Select(x => x.Value.Alias).ToArray();
                foreach (var remove in exclude)
                {
                    propertiesToValidate.RemoveAll(property => property.Alias == remove);
                }

                return ValidateProperties(propertiesToValidate.ToArray(), postedItem.PersistedContent.Properties.ToArray(), actionContext);
            }
        }
    }
}