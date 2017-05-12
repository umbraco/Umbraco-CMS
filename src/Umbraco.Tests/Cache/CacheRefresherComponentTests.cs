using System;
using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Cache;

namespace Umbraco.Tests.Cache
{
    [TestFixture]
    public class CacheRefresherEventHandlerTests
    {
        [Test]
        public void Can_Find_All_Event_Handlers()
        {
            var testObjects = new TestObjects(null);
            var serviceContext = testObjects.GetServiceContextMock();
            var definitions = new IEventDefinition[]
            {
                //I would test these but they are legacy events and we don't need them for deploy, when we migrate to new/better events we can wire up the check
                //Permission.New += PermissionNew;
                //Permission.Updated += PermissionUpdated;
                //Permission.Deleted += PermissionDeleted;
                //PermissionRepository<IContent>.AssignedPermissions += CacheRefresherEventHandler_AssignedPermissions;

                new EventDefinition<IApplicationTreeService, EventArgs>(null, serviceContext.ApplicationTreeService, new EventArgs(), "Deleted"),
                new EventDefinition<IApplicationTreeService, EventArgs>(null, serviceContext.ApplicationTreeService, new EventArgs(), "Updated"),
                new EventDefinition<IApplicationTreeService, EventArgs>(null, serviceContext.ApplicationTreeService, new EventArgs(), "New"),

                new EventDefinition<ISectionService, EventArgs>(null, serviceContext.SectionService, new EventArgs(), "Deleted"),
                new EventDefinition<ISectionService, EventArgs>(null, serviceContext.SectionService, new EventArgs(), "New"),

                new EventDefinition<IUserService, SaveEventArgs<IUserType>>(null, serviceContext.UserService, new SaveEventArgs<IUserType>((IUserType) null)),
                new EventDefinition<IUserService, DeleteEventArgs<IUserType>>(null, serviceContext.UserService, new DeleteEventArgs<IUserType>((IUserType) null)),

                new EventDefinition<IUserService, SaveEventArgs<IUser>>(null, serviceContext.UserService, new SaveEventArgs<IUser>((IUser) null)),
                new EventDefinition<IUserService, DeleteEventArgs<IUser>>(null, serviceContext.UserService, new DeleteEventArgs<IUser>((IUser) null)),
                new EventDefinition<IUserService, DeleteEventArgs<IUser>>(null, serviceContext.UserService, new DeleteEventArgs<IUser>((IUser) null)),

                new EventDefinition<ILocalizationService, SaveEventArgs<IDictionaryItem>>(null, serviceContext.LocalizationService, new SaveEventArgs<IDictionaryItem>((IDictionaryItem) null)),
                new EventDefinition<ILocalizationService, DeleteEventArgs<IDictionaryItem>>(null, serviceContext.LocalizationService, new DeleteEventArgs<IDictionaryItem>((IDictionaryItem) null)),

                new EventDefinition<IDataTypeService, SaveEventArgs<IDataTypeDefinition>>(null, serviceContext.DataTypeService, new SaveEventArgs<IDataTypeDefinition>((IDataTypeDefinition) null)),
                new EventDefinition<IDataTypeService, DeleteEventArgs<IDataTypeDefinition>>(null, serviceContext.DataTypeService, new DeleteEventArgs<IDataTypeDefinition>((IDataTypeDefinition) null)),

                new EventDefinition<IFileService, SaveEventArgs<Stylesheet>>(null, serviceContext.FileService, new SaveEventArgs<Stylesheet>((Stylesheet) null)),
                new EventDefinition<IFileService, DeleteEventArgs<Stylesheet>>(null, serviceContext.FileService, new DeleteEventArgs<Stylesheet>((Stylesheet) null)),

                new EventDefinition<IDomainService, SaveEventArgs<IDomain>>(null, serviceContext.DomainService, new SaveEventArgs<IDomain>((IDomain) null)),
                new EventDefinition<IDomainService, DeleteEventArgs<IDomain>>(null, serviceContext.DomainService, new DeleteEventArgs<IDomain>((IDomain) null)),

                new EventDefinition<ILocalizationService, SaveEventArgs<ILanguage>>(null, serviceContext.LocalizationService, new SaveEventArgs<ILanguage>((ILanguage) null)),
                new EventDefinition<ILocalizationService, DeleteEventArgs<ILanguage>>(null, serviceContext.LocalizationService, new DeleteEventArgs<ILanguage>((ILanguage) null)),

                new EventDefinition<IContentTypeService, SaveEventArgs<IContentType>>(null, serviceContext.ContentTypeService, new SaveEventArgs<IContentType>((IContentType) null)),
                new EventDefinition<IContentTypeService, DeleteEventArgs<IContentType>>(null, serviceContext.ContentTypeService, new DeleteEventArgs<IContentType>((IContentType) null)),
                new EventDefinition<IContentTypeService, SaveEventArgs<IMediaType>>(null, serviceContext.ContentTypeService, new SaveEventArgs<IMediaType>((IMediaType) null)),
                new EventDefinition<IContentTypeService, DeleteEventArgs<IMediaType>>(null, serviceContext.ContentTypeService, new DeleteEventArgs<IMediaType>((IMediaType) null)),

                new EventDefinition<IMemberTypeService, SaveEventArgs<IMemberType>>(null, serviceContext.MemberTypeService, new SaveEventArgs<IMemberType>((IMemberType) null)),
                new EventDefinition<IMemberTypeService, DeleteEventArgs<IMemberType>>(null, serviceContext.MemberTypeService, new DeleteEventArgs<IMemberType>((IMemberType) null)),

                new EventDefinition<IFileService, SaveEventArgs<ITemplate>>(null, serviceContext.FileService, new SaveEventArgs<ITemplate>((ITemplate) null)),
                new EventDefinition<IFileService, DeleteEventArgs<ITemplate>>(null, serviceContext.FileService, new DeleteEventArgs<ITemplate>((ITemplate) null)),

                new EventDefinition<IMacroService, SaveEventArgs<IMacro>>(null, serviceContext.MacroService, new SaveEventArgs<IMacro>((IMacro) null)),
                new EventDefinition<IMacroService, DeleteEventArgs<IMacro>>(null, serviceContext.MacroService, new DeleteEventArgs<IMacro>((IMacro) null)),

                new EventDefinition<IMemberService, SaveEventArgs<IMember>>(null, serviceContext.MemberService, new SaveEventArgs<IMember>((IMember) null)),
                new EventDefinition<IMemberService, DeleteEventArgs<IMember>>(null, serviceContext.MemberService, new DeleteEventArgs<IMember>((IMember) null)),

                new EventDefinition<IMemberGroupService, SaveEventArgs<IMemberGroup>>(null, serviceContext.MemberGroupService, new SaveEventArgs<IMemberGroup>((IMemberGroup) null)),
                new EventDefinition<IMemberGroupService, DeleteEventArgs<IMemberGroup>>(null, serviceContext.MemberGroupService, new DeleteEventArgs<IMemberGroup>((IMemberGroup) null)),

                new EventDefinition<IMediaService, SaveEventArgs<IMedia>>(null, serviceContext.MediaService, new SaveEventArgs<IMedia>((IMedia) null)),
                new EventDefinition<IMediaService, DeleteEventArgs<IMedia>>(null, serviceContext.MediaService, new DeleteEventArgs<IMedia>((IMedia) null)),
                new EventDefinition<IMediaService, MoveEventArgs<IMedia>>(null, serviceContext.MediaService, new MoveEventArgs<IMedia>(new MoveEventInfo<IMedia>(null, "", -1)), "Moved"),
                new EventDefinition<IMediaService, MoveEventArgs<IMedia>>(null, serviceContext.MediaService, new MoveEventArgs<IMedia>(new MoveEventInfo<IMedia>(null, "", -1)), "Trashed"),
                new EventDefinition<IMediaService, RecycleBinEventArgs>(null, serviceContext.MediaService, new RecycleBinEventArgs(Guid.NewGuid(), new Dictionary<int, IEnumerable<Property>>(), true)),

                new EventDefinition<IContentService, SaveEventArgs<IContent>>(null, serviceContext.ContentService, new SaveEventArgs<IContent>((IContent) null)),
                new EventDefinition<IContentService, DeleteEventArgs<IContent>>(null, serviceContext.ContentService, new DeleteEventArgs<IContent>((IContent) null)),
                new EventDefinition<IContentService, CopyEventArgs<IContent>>(null, serviceContext.ContentService, new CopyEventArgs<IContent>(null, null, -1)),
                new EventDefinition<IContentService, MoveEventArgs<IContent>>(null, serviceContext.ContentService, new MoveEventArgs<IContent>(new MoveEventInfo<IContent>(null, "", -1)), "Trashed"),
                new EventDefinition<IContentService, RecycleBinEventArgs>(null, serviceContext.ContentService, new RecycleBinEventArgs(Guid.NewGuid(), new Dictionary<int, IEnumerable<Property>>(), true)),
                new EventDefinition<IContentService, PublishEventArgs<IContent>>(null, serviceContext.ContentService, new PublishEventArgs<IContent>((IContent) null), "Published"),
                new EventDefinition<IContentService, PublishEventArgs<IContent>>(null, serviceContext.ContentService, new PublishEventArgs<IContent>((IContent) null), "UnPublished"),

                new EventDefinition<IPublicAccessService, SaveEventArgs<PublicAccessEntry>>(null, serviceContext.PublicAccessService, new SaveEventArgs<PublicAccessEntry>((PublicAccessEntry) null)),
                new EventDefinition<IPublicAccessService, DeleteEventArgs<PublicAccessEntry>>(null, serviceContext.PublicAccessService, new DeleteEventArgs<PublicAccessEntry>((PublicAccessEntry) null)),

                new EventDefinition<IRelationService, SaveEventArgs<IRelationType>>(null, serviceContext.RelationService, new SaveEventArgs<IRelationType>((IRelationType) null)),
                new EventDefinition<IRelationService, DeleteEventArgs<IRelationType>>(null, serviceContext.RelationService, new DeleteEventArgs<IRelationType>((IRelationType) null)),

                new EventDefinition<IRelationService, SaveEventArgs<IRelationType>>(null, serviceContext.RelationService, new SaveEventArgs<IRelationType>((IRelationType) null)),
                new EventDefinition<IRelationService, DeleteEventArgs<IRelationType>>(null, serviceContext.RelationService, new DeleteEventArgs<IRelationType>((IRelationType) null)),
            };

            foreach (var definition in definitions)
            {
                var found = CacheRefresherComponent.FindHandler(definition);
                Assert.IsNotNull(found, "Couldn't find method for " + definition.EventName + " on " + definition.Sender.GetType());
            }
        }
    }
}