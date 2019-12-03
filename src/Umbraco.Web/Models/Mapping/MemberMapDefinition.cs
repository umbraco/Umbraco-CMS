using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Core.Services.Implement;
using UserProfile = Umbraco.Web.Models.ContentEditing.UserProfile;
using Umbraco.Web.Security;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Declares model mappings for members.
    /// </summary>
    internal class MemberMapDefinition : IMapDefinition
    {
        private readonly CommonMapper _commonMapper;
        private readonly MemberTabsAndPropertiesMapper _tabsAndPropertiesMapper;

        public MemberMapDefinition(CommonMapper commonMapper, MemberTabsAndPropertiesMapper tabsAndPropertiesMapper)
        {
            _commonMapper = commonMapper;

            _tabsAndPropertiesMapper = tabsAndPropertiesMapper;
        }

        public void DefineMaps(UmbracoMapper mapper)
        {
            mapper.Define<IMember, MemberDisplay>((source, context) => new MemberDisplay(), Map);
            mapper.Define<IMember, MemberBasic>((source, context) => new MemberBasic(), Map);
            mapper.Define<IMemberGroup, MemberGroupDisplay>((source, context) => new MemberGroupDisplay(), Map);
            mapper.Define<IMember, ContentPropertyCollectionDto>((source, context) => new ContentPropertyCollectionDto(), Map);
        }      

        // Umbraco.Code.MapAll -Properties -Errors -Edited -Updater -Alias -IsChildOfListView
        // Umbraco.Code.MapAll -Trashed -IsContainer -VariesByCulture
        private void Map(IMember source, MemberDisplay target, MapperContext context)
        {
            target.ContentTypeId = source.ContentType.Id;
            target.ContentTypeAlias = source.ContentType.Alias;
            target.ContentTypeName = source.ContentType.Name;
            target.CreateDate = source.CreateDate;
            target.Email = source.Email;
            target.Icon = source.ContentType.Icon;
            target.Id = source.Id;
            target.Key = source.Key;
            target.MemberProviderFieldMapping = GetMemberProviderFieldMapping();
            target.Name = source.Name;
            target.Owner = _commonMapper.GetOwner(source, context);
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
            target.ContentTypeId = source.ContentType.Id;
            target.ContentTypeAlias = source.ContentType.Alias;
            target.CreateDate = source.CreateDate;
            target.Email = source.Email;
            target.Icon = source.ContentType.Icon;
            target.Id = int.MaxValue;
            target.Key = source.Key;
            target.Name = source.Name;
            target.Owner = _commonMapper.GetOwner(source, context);
            target.ParentId = source.ParentId;
            target.Path = source.Path;
            target.Properties = context.MapEnumerable<IProperty, ContentPropertyBasic>(source.Properties);
            target.SortOrder = source.SortOrder;
            target.State = null;
            target.Udi = Udi.Create(Constants.UdiEntityType.Member, source.Key);
            target.UpdateDate = source.UpdateDate;
            target.Username = source.Username;
        }

        // Umbraco.Code.MapAll -Icon -Trashed -ParentId -Alias
        private void Map(IMemberGroup source, MemberGroupDisplay target, MapperContext context)
        {
            target.Id = source.Id;
            target.Key = source.Key;
            target.Name = source.Name;
            target.Path = $"-1,{source.Id}";
            target.Udi = Udi.Create(Constants.UdiEntityType.MemberGroup, source.Key);
        }

        // Umbraco.Code.MapAll
        private static void Map(IMember source, ContentPropertyCollectionDto target, MapperContext context)
        {
            target.Properties = context.MapEnumerable<IProperty, ContentPropertyDto>(source.Properties);
        }

        private static IDictionary<string, string> GetMemberProviderFieldMapping()
        {
            var provider = MembershipProviderExtensions.GetMembersMembershipProvider();
            
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
