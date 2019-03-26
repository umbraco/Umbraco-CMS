using System;
using System.Collections.Generic;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Core.Services.Implement;
using UserProfile = Umbraco.Web.Models.ContentEditing.UserProfile;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Declares model mappings for members.
    /// </summary>
    internal class MemberMapperProfile : IMapperProfile
    {
        private readonly CommonMapper _commonMapper;
        private readonly IMemberTypeService _memberTypeService;
        private readonly TabsAndPropertiesMapper<IMember> _tabsAndPropertiesMapper;

        public MemberMapperProfile(CommonMapper commonMapper, IMemberTypeService memberTypeService, ILocalizedTextService localizedTextService)
        {
            _commonMapper = commonMapper;
            _memberTypeService = memberTypeService;

            _tabsAndPropertiesMapper = new TabsAndPropertiesMapper<IMember>(localizedTextService);
        }

        public void SetMaps(Mapper mapper)
        {
            mapper.Define<MembershipUser, MemberDisplay>((source, context) => new MemberDisplay(), Map);
            mapper.Define<MembershipUser, IMember>((source, context) => MemberService.CreateGenericMembershipProviderMember(source.UserName, source.Email, source.UserName, ""), Map);
            mapper.Define<IMember, MemberDisplay>((source, context) => new MemberDisplay(), Map);
            mapper.Define<IMember, MemberBasic>((source, context) => new MemberBasic(), Map);
            mapper.Define<MembershipUser, MemberBasic>((source, context) => new MemberBasic(), Map);
            mapper.Define<IMemberGroup, MemberGroupDisplay>((source, context) => new MemberGroupDisplay(), Map);
        }

        private void Map(MembershipUser source, MemberDisplay target, MapperContext context)
        {
            //first convert to IMember
            var member = context.Mapper.Map<IMember>(source);
            //then convert to MemberDisplay
            context.Mapper.Map<IMember, MemberDisplay>(member);
        }

        // TODO: SD: I can't remember why this mapping is here?
        // Umbraco.Code.MapAll -Properties -CreatorId -Level -Name -CultureInfos -ParentId
        // Umbraco.Code.MapAll -Path -SortOrder -DeleteDate -WriterId -VersionId -PasswordQuestion
        // Umbraco.Code.MapAll -RawPasswordAnswerValue -FailedPasswordAttempts
        private void Map(MembershipUser source, IMember target, MapperContext context)
        {
            target.Comments = source.Comment;
            target.CreateDate = source.CreationDate;
            target.Email = source.Email;
            target.Id = int.MaxValue;
            target.IsApproved = source.IsApproved;
            target.IsLockedOut = source.IsLockedOut;
            target.Key = source.ProviderUserKey.TryConvertTo<Guid>().Result;
            target.LastLockoutDate = source.LastLockoutDate;
            target.LastLoginDate = source.LastLoginDate;
            target.LastPasswordChangeDate = source.LastPasswordChangedDate;
            target.ProviderUserKey = source.ProviderUserKey;
            target.RawPasswordValue = source.CreationDate > DateTime.MinValue ? Guid.NewGuid().ToString("N") : "";
            target.UpdateDate = source.LastActivityDate;
            target.Username = source.UserName;
        }

        // Umbraco.Code.MapAll -Properties -Errors -Edited -Updater -Alias -IsChildOfListView
        // Umbraco.Code.MapAll -Trashed -IsContainer -VariesByCulture
        private void Map(IMember source, MemberDisplay target, MapperContext context)
        {
            target.ContentTypeAlias = source.ContentType.Alias;
            target.ContentTypeName = source.ContentType.Name;
            target.CreateDate = source.CreateDate;
            target.Email = source.Email;
            target.Icon = source.ContentType.Icon;
            target.Id = source.Id;
            target.Key = source.Key;
            target.MemberProviderFieldMapping = GetMemberProviderFieldMapping();
            target.MembershipScenario = GetMembershipScenario();
            target.Name = source.Name;
            target.Owner = _commonMapper.GetOwner(source, context.Mapper);
            target.ParentId = source.ParentId;
            target.Path = source.Path;
            target.SortOrder = source.SortOrder;
            target.State = null;
            target.Tabs = _tabsAndPropertiesMapper.Map(source, context);
            target.TreeNodeUrl = _commonMapper.GetMemberTreeNodeUrl(source);
            target.Udi = Udi.Create(Constants.UdiEntityType.Member, source.Key);
            target.UpdateDate = source.UpdateDate;
            target.Username = source.Username;
        }

        // Umbraco.Code.MapAll -Trashed -Edited -Updater -Alias -VariesByCulture
        private void Map(IMember source, MemberBasic target, MapperContext context)
        {
            target.ContentTypeAlias = source.ContentType.Alias;
            target.CreateDate = source.CreateDate;
            target.Email = source.Email;
            target.Icon = source.ContentType.Icon;
            target.Id = int.MaxValue;
            target.Key = source.Key;
            target.Name = source.Name;
            target.Owner = _commonMapper.GetOwner(source, context.Mapper);
            target.ParentId = source.ParentId;
            target.Path = source.Path;
            target.Properties = context.Mapper.Map<IEnumerable<ContentPropertyBasic>>(source.Properties);
            target.SortOrder = source.SortOrder;
            target.State = null;
            target.Udi = Udi.Create(Constants.UdiEntityType.Member, source.Key);
            target.UpdateDate = source.UpdateDate;
            target.Username = source.Username;
        }

        //TODO: SD: I can't remember why this mapping is here?
        // Umbraco.Code.MapAll -Udi -Properties -ParentId -Path -SortOrder -Edited -Updater
        // Umbraco.Code.MapAll -Trashed -Alias -ContentTypeAlias -VariesByCulture
        private void Map(MembershipUser source, MemberBasic target, MapperContext context)
        {
            target.CreateDate = source.CreationDate;
            target.Email = source.Email;
            target.Icon = "icon-user";
            target.Id = int.MaxValue;
            target.Key = source.ProviderUserKey.TryConvertTo<Guid>().Result;
            target.Name = source.UserName;
            target.Owner = new UserProfile { Name = "Admin", UserId = -1 };
            target.State = ContentSavedState.Draft;
            target.UpdateDate = source.LastActivityDate;
            target.Username = source.UserName;
}

        // Umbraco.Code.MapAll -Icon -Trashed -ParentId -Alias
        private void Map(IMemberGroup source, MemberGroupDisplay target, MapperContext context)
        {
            target.Id = source.Id;
            target.Key = source.Key;
            target.Name = source.Name;
            target.Path = "-1" + source.Id;
            target.Udi = Udi.Create(Constants.UdiEntityType.MemberGroup, source.Key);
        }

        private MembershipScenario GetMembershipScenario()
        {
            var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();

            if (provider.IsUmbracoMembershipProvider())
            {
                return MembershipScenario.NativeUmbraco;
            }
            var memberType = _memberTypeService.Get(Constants.Conventions.MemberTypes.DefaultAlias);
            return memberType != null
                ? MembershipScenario.CustomProviderWithUmbracoLink
                : MembershipScenario.StandaloneCustomProvider;
        }

        private static IDictionary<string, string> GetMemberProviderFieldMapping()
        {
            var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();

            if (provider.IsUmbracoMembershipProvider() == false)
            {
                return new Dictionary<string, string>
                {
                    {Constants.Conventions.Member.IsLockedOut, Constants.Conventions.Member.IsLockedOut},
                    {Constants.Conventions.Member.IsApproved, Constants.Conventions.Member.IsApproved},
                    {Constants.Conventions.Member.Comments, Constants.Conventions.Member.Comments}
                };
            }

            var umbracoProvider = (IUmbracoMemberTypeMembershipProvider)provider;

            return new Dictionary<string, string>
            {
                {Constants.Conventions.Member.IsLockedOut, umbracoProvider.LockPropertyTypeAlias},
                {Constants.Conventions.Member.IsApproved, umbracoProvider.ApprovedPropertyTypeAlias},
                {Constants.Conventions.Member.Comments, umbracoProvider.CommentPropertyTypeAlias}
            };
        }
    }
}
