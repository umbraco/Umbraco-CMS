using System;
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

        protected override IMember GetExisting(MemberSave model)
        {
            //TODO: We're going to remove the built-in member properties from this editor - We didn't support these in 6.x so 
            // pretty hard to support them in 7 when the member type editor is using the old APIs!
            var toRemove = Constants.Conventions.Member.StandardPropertyTypeStubs.Select(x => x.Value.Alias).ToArray();

            var member =  ApplicationContext.Services.MemberService.GetByUsername(model.Username);
            foreach (var remove in toRemove)
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

            //TODO: We're going to remove the built-in member properties from this editor - We didn't support these in 6.x so 
            // pretty hard to support them in 7 when the member type editor is using the old APIs!
            var toRemove = Constants.Conventions.Member.StandardPropertyTypeStubs.Select(x => x.Value.Alias).ToArray();
            foreach (var remove in toRemove)
            {
                contentType.RemovePropertyType(remove);
            }

            //return the new member with the details filled in
            return new Member(model.Name, model.Email, model.Username, model.Password.NewPassword, -1, contentType);
        }

        protected override ContentItemDto<IMember> MapFromPersisted(MemberSave model)
        {
            return Mapper.Map<IMember, ContentItemDto<IMember>>(model.PersistedContent);
        }
    }
}