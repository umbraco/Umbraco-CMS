﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;
using Umbraco.Web.Cache;

namespace Umbraco.Tests.Cache
{
    [TestFixture]
    [UmbracoTest(WithApplication = true)]
    public class CacheRefresherEventHandlerTests : UmbracoTestBase
    {
        [Test]
        public void Can_Find_All_Event_Handlers()
        {
            // fixme - cannot work with mocks
            // because the events are defined on actual static classes, not on the interfaces, so name matching fails
            // we should really refactor events entirely - in the meantime, let it be an UmbracoTestBase ;(
            //var testObjects = new TestObjects(null);
            //var serviceContext = testObjects.GetServiceContextMock();
            var serviceContext = Current.Services;

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

                new EventDefinition<IUserService, SaveEventArgs<IUser>>(null, serviceContext.UserService, new SaveEventArgs<IUser>(Enumerable.Empty<IUser>())),
                new EventDefinition<IUserService, DeleteEventArgs<IUser>>(null, serviceContext.UserService, new DeleteEventArgs<IUser>(Enumerable.Empty<IUser>())),
                new EventDefinition<IUserService, SaveEventArgs<UserGroupWithUsers>>(null, serviceContext.UserService, new SaveEventArgs<UserGroupWithUsers>(Enumerable.Empty<UserGroupWithUsers>())),
                new EventDefinition<IUserService, DeleteEventArgs<IUserGroup>>(null, serviceContext.UserService, new DeleteEventArgs<IUserGroup>(Enumerable.Empty<IUserGroup>())),

                new EventDefinition<ILocalizationService, SaveEventArgs<IDictionaryItem>>(null, serviceContext.LocalizationService, new SaveEventArgs<IDictionaryItem>(Enumerable.Empty<IDictionaryItem>())),
                new EventDefinition<ILocalizationService, DeleteEventArgs<IDictionaryItem>>(null, serviceContext.LocalizationService, new DeleteEventArgs<IDictionaryItem>(Enumerable.Empty<IDictionaryItem>())),

                new EventDefinition<IDataTypeService, SaveEventArgs<IDataType>>(null, serviceContext.DataTypeService, new SaveEventArgs<IDataType>(Enumerable.Empty<IDataType>())),
                new EventDefinition<IDataTypeService, DeleteEventArgs<IDataType>>(null, serviceContext.DataTypeService, new DeleteEventArgs<IDataType>(Enumerable.Empty<IDataType>())),

                new EventDefinition<IFileService, SaveEventArgs<Stylesheet>>(null, serviceContext.FileService, new SaveEventArgs<Stylesheet>(Enumerable.Empty<Stylesheet>())),
                new EventDefinition<IFileService, DeleteEventArgs<Stylesheet>>(null, serviceContext.FileService, new DeleteEventArgs<Stylesheet>(Enumerable.Empty<Stylesheet>())),

                new EventDefinition<IDomainService, SaveEventArgs<IDomain>>(null, serviceContext.DomainService, new SaveEventArgs<IDomain>(Enumerable.Empty<IDomain>())),
                new EventDefinition<IDomainService, DeleteEventArgs<IDomain>>(null, serviceContext.DomainService, new DeleteEventArgs<IDomain>(Enumerable.Empty<IDomain>())),

                new EventDefinition<ILocalizationService, SaveEventArgs<ILanguage>>(null, serviceContext.LocalizationService, new SaveEventArgs<ILanguage>(Enumerable.Empty<ILanguage>())),
                new EventDefinition<ILocalizationService, DeleteEventArgs<ILanguage>>(null, serviceContext.LocalizationService, new DeleteEventArgs<ILanguage>(Enumerable.Empty<ILanguage>())),

                new EventDefinition<IContentTypeService, SaveEventArgs<IContentType>>(null, serviceContext.ContentTypeService, new SaveEventArgs<IContentType>(Enumerable.Empty<IContentType>())),
                new EventDefinition<IContentTypeService, DeleteEventArgs<IContentType>>(null, serviceContext.ContentTypeService, new DeleteEventArgs<IContentType>(Enumerable.Empty<IContentType>())),
                new EventDefinition<IMediaTypeService, SaveEventArgs<IMediaType>>(null, serviceContext.MediaTypeService, new SaveEventArgs<IMediaType>(Enumerable.Empty<IMediaType>())),
                new EventDefinition<IMediaTypeService, DeleteEventArgs<IMediaType>>(null, serviceContext.MediaTypeService, new DeleteEventArgs<IMediaType>(Enumerable.Empty<IMediaType>())),

                new EventDefinition<IMemberTypeService, SaveEventArgs<IMemberType>>(null, serviceContext.MemberTypeService, new SaveEventArgs<IMemberType>(Enumerable.Empty<IMemberType>())),
                new EventDefinition<IMemberTypeService, DeleteEventArgs<IMemberType>>(null, serviceContext.MemberTypeService, new DeleteEventArgs<IMemberType>(Enumerable.Empty<IMemberType>())),

                new EventDefinition<IFileService, SaveEventArgs<ITemplate>>(null, serviceContext.FileService, new SaveEventArgs<ITemplate>(Enumerable.Empty<ITemplate>())),
                new EventDefinition<IFileService, DeleteEventArgs<ITemplate>>(null, serviceContext.FileService, new DeleteEventArgs<ITemplate>(Enumerable.Empty<ITemplate>())),

                new EventDefinition<IMacroService, SaveEventArgs<IMacro>>(null, serviceContext.MacroService, new SaveEventArgs<IMacro>(Enumerable.Empty<IMacro>())),
                new EventDefinition<IMacroService, DeleteEventArgs<IMacro>>(null, serviceContext.MacroService, new DeleteEventArgs<IMacro>(Enumerable.Empty<IMacro>())),

                new EventDefinition<IMemberService, SaveEventArgs<IMember>>(null, serviceContext.MemberService, new SaveEventArgs<IMember>(Enumerable.Empty<IMember>())),
                new EventDefinition<IMemberService, DeleteEventArgs<IMember>>(null, serviceContext.MemberService, new DeleteEventArgs<IMember>(Enumerable.Empty<IMember>())),

                new EventDefinition<IMemberGroupService, SaveEventArgs<IMemberGroup>>(null, serviceContext.MemberGroupService, new SaveEventArgs<IMemberGroup>(Enumerable.Empty<IMemberGroup>())),
                new EventDefinition<IMemberGroupService, DeleteEventArgs<IMemberGroup>>(null, serviceContext.MemberGroupService, new DeleteEventArgs<IMemberGroup>(Enumerable.Empty<IMemberGroup>())),

                new EventDefinition<IMediaService, SaveEventArgs<IMedia>>(null, serviceContext.MediaService, new SaveEventArgs<IMedia>(Enumerable.Empty<IMedia>())),
                new EventDefinition<IMediaService, DeleteEventArgs<IMedia>>(null, serviceContext.MediaService, new DeleteEventArgs<IMedia>(Enumerable.Empty<IMedia>())),
                new EventDefinition<IMediaService, MoveEventArgs<IMedia>>(null, serviceContext.MediaService, new MoveEventArgs<IMedia>(new MoveEventInfo<IMedia>(null, "", -1)), "Moved"),
                new EventDefinition<IMediaService, MoveEventArgs<IMedia>>(null, serviceContext.MediaService, new MoveEventArgs<IMedia>(new MoveEventInfo<IMedia>(null, "", -1)), "Trashed"),
                new EventDefinition<IMediaService, RecycleBinEventArgs>(null, serviceContext.MediaService, new RecycleBinEventArgs(Guid.NewGuid())),

                new EventDefinition<IContentService, SaveEventArgs<IContent>>(null, serviceContext.ContentService, new SaveEventArgs<IContent>(Enumerable.Empty<IContent>()), "Saved"),
                new EventDefinition<IContentService, DeleteEventArgs<IContent>>(null, serviceContext.ContentService, new DeleteEventArgs<IContent>(Enumerable.Empty<IContent>()), "Deleted"),

                // not managed
                //new EventDefinition<IContentService, SaveEventArgs<IContent>>(null, serviceContext.ContentService, new SaveEventArgs<IContent>(Enumerable.Empty<IContent>()), "SavedBlueprint"),
                //new EventDefinition<IContentService, DeleteEventArgs<IContent>>(null, serviceContext.ContentService, new DeleteEventArgs<IContent>(Enumerable.Empty<IContent>()), "DeletedBlueprint"),

                new EventDefinition<IContentService, CopyEventArgs<IContent>>(null, serviceContext.ContentService, new CopyEventArgs<IContent>(null, null, -1)),
                new EventDefinition<IContentService, MoveEventArgs<IContent>>(null, serviceContext.ContentService, new MoveEventArgs<IContent>(new MoveEventInfo<IContent>(null, "", -1)), "Trashed"),
                new EventDefinition<IContentService, RecycleBinEventArgs>(null, serviceContext.ContentService, new RecycleBinEventArgs(Guid.NewGuid())),
                new EventDefinition<IContentService, PublishEventArgs<IContent>>(null, serviceContext.ContentService, new PublishEventArgs<IContent>(Enumerable.Empty<IContent>()), "Published"),
                new EventDefinition<IContentService, PublishEventArgs<IContent>>(null, serviceContext.ContentService, new PublishEventArgs<IContent>(Enumerable.Empty<IContent>()), "Unpublished"),

                new EventDefinition<IPublicAccessService, SaveEventArgs<PublicAccessEntry>>(null, serviceContext.PublicAccessService, new SaveEventArgs<PublicAccessEntry>(Enumerable.Empty<PublicAccessEntry>())),
                new EventDefinition<IPublicAccessService, DeleteEventArgs<PublicAccessEntry>>(null, serviceContext.PublicAccessService, new DeleteEventArgs<PublicAccessEntry>(Enumerable.Empty<PublicAccessEntry>())),

                new EventDefinition<IRelationService, SaveEventArgs<IRelationType>>(null, serviceContext.RelationService, new SaveEventArgs<IRelationType>(Enumerable.Empty<IRelationType>())),
                new EventDefinition<IRelationService, DeleteEventArgs<IRelationType>>(null, serviceContext.RelationService, new DeleteEventArgs<IRelationType>(Enumerable.Empty<IRelationType>())),

                new EventDefinition<IRelationService, SaveEventArgs<IRelationType>>(null, serviceContext.RelationService, new SaveEventArgs<IRelationType>(Enumerable.Empty<IRelationType>())),
                new EventDefinition<IRelationService, DeleteEventArgs<IRelationType>>(null, serviceContext.RelationService, new DeleteEventArgs<IRelationType>(Enumerable.Empty<IRelationType>())),
            };

            var ok = true;
            foreach (var definition in definitions)
            {
                var found = CacheRefresherComponent.FindHandler(definition);
                if (found == null)
                {
                    Console.WriteLine("Couldn't find method for " + definition.EventName + " on " + definition.Sender.GetType());
                    ok = false;
                }
            }
            Assert.IsTrue(ok, "see log for details");
        }
    }
}
