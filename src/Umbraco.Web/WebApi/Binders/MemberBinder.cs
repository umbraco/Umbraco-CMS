using System;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

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
            return ApplicationContext.Services.MemberService.GetByUsername(model.Username);
        }

        protected override IMember CreateNew(MemberSave model)
        {
            var contentType = ApplicationContext.Services.ContentTypeService.GetMemberType(model.ContentTypeAlias);
            if (contentType == null)
            {
                throw new InvalidOperationException("No member type found wth alias " + model.ContentTypeAlias);
            }
            return new Member(model.Name, model.ParentId, contentType, new PropertyCollection());
        }

        protected override ContentItemDto<IMember> MapFromPersisted(MemberSave model)
        {
            return Mapper.Map<IMember, ContentItemDto<IMember>>(model.PersistedContent);
        }
    }
}