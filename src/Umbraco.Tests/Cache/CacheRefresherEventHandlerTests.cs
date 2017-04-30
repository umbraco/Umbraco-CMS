using System;
using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Cache;
using System.Linq;

namespace Umbraco.Tests.Cache
{
    [TestFixture]
    public class CacheRefresherEventHandlerTests : BaseUmbracoApplicationTest
    {
        [Test]
        public void Can_Find_All_Event_Handlers()
        {
            var definitions = new IEventDefinition[]
            {
                //I would test these but they are legacy events and we don't need them for deploy, when we migrate to new/better events we can wire up the check
                //Permission.New += PermissionNew;
                //Permission.Updated += PermissionUpdated;
                //Permission.Deleted += PermissionDeleted;
                //PermissionRepository<IContent>.AssignedPermissions += CacheRefresherEventHandler_AssignedPermissions;

                new EventDefinition<IApplicationTreeService, EventArgs>(null, ServiceContext.ApplicationTreeService, new EventArgs(), "Deleted"),
                new EventDefinition<IApplicationTreeService, EventArgs>(null, ServiceContext.ApplicationTreeService, new EventArgs(), "Updated"),
                new EventDefinition<IApplicationTreeService, EventArgs>(null, ServiceContext.ApplicationTreeService, new EventArgs(), "New"),

                new EventDefinition<ISectionService, EventArgs>(null, ServiceContext.SectionService, new EventArgs(), "Deleted"),
                new EventDefinition<ISectionService, EventArgs>(null, ServiceContext.SectionService, new EventArgs(), "New"),

                new EventDefinition<IUserService, SaveEventArgs<IUserType>>(null, ServiceContext.UserService, new SaveEventArgs<IUserType>(Enumerable.Empty<IUserType>())),
                new EventDefinition<IUserService, DeleteEventArgs<IUserType>>(null, ServiceContext.UserService, new DeleteEventArgs<IUserType>((IUserType) null)),

                new EventDefinition<IUserService, SaveEventArgs<IUser>>(null, ServiceContext.UserService, new SaveEventArgs<IUser>((IUser) null)),
                new EventDefinition<IUserService, DeleteEventArgs<IUser>>(null, ServiceContext.UserService, new DeleteEventArgs<IUser>((IUser) null)),
                new EventDefinition<IUserService, DeleteEventArgs<IUser>>(null, ServiceContext.UserService, new DeleteEventArgs<IUser>((IUser) null)),

                new EventDefinition<ILocalizationService, SaveEventArgs<IDictionaryItem>>(null, ServiceContext.LocalizationService, new SaveEventArgs<IDictionaryItem>((IDictionaryItem) null)),
                new EventDefinition<ILocalizationService, DeleteEventArgs<IDictionaryItem>>(null, ServiceContext.LocalizationService, new DeleteEventArgs<IDictionaryItem>((IDictionaryItem) null)),

                new EventDefinition<IDataTypeService, SaveEventArgs<IDataTypeDefinition>>(null, ServiceContext.DataTypeService, new SaveEventArgs<IDataTypeDefinition>((IDataTypeDefinition) null)),
                new EventDefinition<IDataTypeService, DeleteEventArgs<IDataTypeDefinition>>(null, ServiceContext.DataTypeService, new DeleteEventArgs<IDataTypeDefinition>((IDataTypeDefinition) null)),

                new EventDefinition<IFileService, SaveEventArgs<Stylesheet>>(null, ServiceContext.FileService, new SaveEventArgs<Stylesheet>((Stylesheet) null)),
                new EventDefinition<IFileService, DeleteEventArgs<Stylesheet>>(null, ServiceContext.FileService, new DeleteEventArgs<Stylesheet>((Stylesheet) null)),

                new EventDefinition<IDomainService, SaveEventArgs<IDomain>>(null, ServiceContext.DomainService, new SaveEventArgs<IDomain>((IDomain) null)),
                new EventDefinition<IDomainService, DeleteEventArgs<IDomain>>(null, ServiceContext.DomainService, new DeleteEventArgs<IDomain>((IDomain) null)),

                new EventDefinition<ILocalizationService, SaveEventArgs<ILanguage>>(null, ServiceContext.LocalizationService, new SaveEventArgs<ILanguage>((ILanguage) null)),
                new EventDefinition<ILocalizationService, DeleteEventArgs<ILanguage>>(null, ServiceContext.LocalizationService, new DeleteEventArgs<ILanguage>((ILanguage) null)),

                new EventDefinition<IContentTypeService, SaveEventArgs<IContentType>>(null, ServiceContext.ContentTypeService, new SaveEventArgs<IContentType>((IContentType) null)),
                new EventDefinition<IContentTypeService, DeleteEventArgs<IContentType>>(null, ServiceContext.ContentTypeService, new DeleteEventArgs<IContentType>((IContentType) null)),
                new EventDefinition<IContentTypeService, SaveEventArgs<IMediaType>>(null, ServiceContext.ContentTypeService, new SaveEventArgs<IMediaType>((IMediaType) null)),
                new EventDefinition<IContentTypeService, DeleteEventArgs<IMediaType>>(null, ServiceContext.ContentTypeService, new DeleteEventArgs<IMediaType>((IMediaType) null)),

                new EventDefinition<IMemberTypeService, SaveEventArgs<IMemberType>>(null, ServiceContext.MemberTypeService, new SaveEventArgs<IMemberType>((IMemberType) null)),
                new EventDefinition<IMemberTypeService, DeleteEventArgs<IMemberType>>(null, ServiceContext.MemberTypeService, new DeleteEventArgs<IMemberType>((IMemberType) null)),

                new EventDefinition<IFileService, SaveEventArgs<ITemplate>>(null, ServiceContext.FileService, new SaveEventArgs<ITemplate>((ITemplate) null)),
                new EventDefinition<IFileService, DeleteEventArgs<ITemplate>>(null, ServiceContext.FileService, new DeleteEventArgs<ITemplate>((ITemplate) null)),

                new EventDefinition<IMacroService, SaveEventArgs<IMacro>>(null, ServiceContext.MacroService, new SaveEventArgs<IMacro>((IMacro) null)),
                new EventDefinition<IMacroService, DeleteEventArgs<IMacro>>(null, ServiceContext.MacroService, new DeleteEventArgs<IMacro>((IMacro) null)),

                new EventDefinition<IMemberService, SaveEventArgs<IMember>>(null, ServiceContext.MemberService, new SaveEventArgs<IMember>((IMember) null)),
                new EventDefinition<IMemberService, DeleteEventArgs<IMember>>(null, ServiceContext.MemberService, new DeleteEventArgs<IMember>((IMember) null)),

                new EventDefinition<IMemberGroupService, SaveEventArgs<IMemberGroup>>(null, ServiceContext.MemberGroupService, new SaveEventArgs<IMemberGroup>((IMemberGroup) null)),
                new EventDefinition<IMemberGroupService, DeleteEventArgs<IMemberGroup>>(null, ServiceContext.MemberGroupService, new DeleteEventArgs<IMemberGroup>((IMemberGroup) null)),

                new EventDefinition<IMediaService, SaveEventArgs<IMedia>>(null, ServiceContext.MediaService, new SaveEventArgs<IMedia>((IMedia) null)),
                new EventDefinition<IMediaService, DeleteEventArgs<IMedia>>(null, ServiceContext.MediaService, new DeleteEventArgs<IMedia>((IMedia) null)),
                new EventDefinition<IMediaService, MoveEventArgs<IMedia>>(null, ServiceContext.MediaService, new MoveEventArgs<IMedia>(new MoveEventInfo<IMedia>(null, "", -1)), "Moved"),
                new EventDefinition<IMediaService, MoveEventArgs<IMedia>>(null, ServiceContext.MediaService, new MoveEventArgs<IMedia>(new MoveEventInfo<IMedia>(null, "", -1)), "Trashed"),
                new EventDefinition<IMediaService, RecycleBinEventArgs>(null, ServiceContext.MediaService, new RecycleBinEventArgs(Guid.NewGuid(), new Dictionary<int, IEnumerable<Property>>(), true)),

                new EventDefinition<IContentService, SaveEventArgs<IContent>>(null, ServiceContext.ContentService, new SaveEventArgs<IContent>((IContent) null)),
                new EventDefinition<IContentService, DeleteEventArgs<IContent>>(null, ServiceContext.ContentService, new DeleteEventArgs<IContent>((IContent) null)),
                new EventDefinition<IContentService, CopyEventArgs<IContent>>(null, ServiceContext.ContentService, new CopyEventArgs<IContent>(null, null, -1)),
                new EventDefinition<IContentService, MoveEventArgs<IContent>>(null, ServiceContext.ContentService, new MoveEventArgs<IContent>(new MoveEventInfo<IContent>(null, "", -1)), "Trashed"),
                new EventDefinition<IContentService, RecycleBinEventArgs>(null, ServiceContext.ContentService, new RecycleBinEventArgs(Guid.NewGuid(), new Dictionary<int, IEnumerable<Property>>(), true)),
                new EventDefinition<IContentService, PublishEventArgs<IContent>>(null, ServiceContext.ContentService, new PublishEventArgs<IContent>((IContent) null), "Published"),
                new EventDefinition<IContentService, PublishEventArgs<IContent>>(null, ServiceContext.ContentService, new PublishEventArgs<IContent>((IContent) null), "UnPublished"),

                new EventDefinition<IPublicAccessService, SaveEventArgs<PublicAccessEntry>>(null, ServiceContext.PublicAccessService, new SaveEventArgs<PublicAccessEntry>((PublicAccessEntry) null)),
                new EventDefinition<IPublicAccessService, DeleteEventArgs<PublicAccessEntry>>(null, ServiceContext.PublicAccessService, new DeleteEventArgs<PublicAccessEntry>((PublicAccessEntry) null)),

                new EventDefinition<IRelationService, SaveEventArgs<IRelationType>>(null, ServiceContext.RelationService, new SaveEventArgs<IRelationType>((IRelationType) null)),
                new EventDefinition<IRelationService, DeleteEventArgs<IRelationType>>(null, ServiceContext.RelationService, new DeleteEventArgs<IRelationType>((IRelationType) null)),

                new EventDefinition<IRelationService, SaveEventArgs<IRelationType>>(null, ServiceContext.RelationService, new SaveEventArgs<IRelationType>((IRelationType) null)),
                new EventDefinition<IRelationService, DeleteEventArgs<IRelationType>>(null, ServiceContext.RelationService, new DeleteEventArgs<IRelationType>((IRelationType) null)),
            };

            foreach (var definition in definitions)
            {
                var found = CacheRefresherEventHandler.FindHandler(definition);
                Assert.IsNotNull(found, "Couldn't find method for " + definition.EventName + " on " + definition.Sender.GetType());
            }
            
        }
    }
}