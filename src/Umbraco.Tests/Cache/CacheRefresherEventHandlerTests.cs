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
                
                new EventDefinition<IUserService, SaveEventArgs<IUser>>(null, ServiceContext.UserService, new SaveEventArgs<IUser>(Enumerable.Empty<IUser>())),
                new EventDefinition<IUserService, DeleteEventArgs<IUser>>(null, ServiceContext.UserService, new DeleteEventArgs<IUser>(Enumerable.Empty<IUser>())),
                new EventDefinition<IUserService, SaveEventArgs<IUserGroup>>(null, ServiceContext.UserService, new SaveEventArgs<IUserGroup>(Enumerable.Empty<IUserGroup>())),
                new EventDefinition<IUserService, DeleteEventArgs<IUserGroup>>(null, ServiceContext.UserService, new DeleteEventArgs<IUserGroup>(Enumerable.Empty<IUserGroup>())),

                new EventDefinition<ILocalizationService, SaveEventArgs<IDictionaryItem>>(null, ServiceContext.LocalizationService, new SaveEventArgs<IDictionaryItem>(Enumerable.Empty<IDictionaryItem>())),
                new EventDefinition<ILocalizationService, DeleteEventArgs<IDictionaryItem>>(null, ServiceContext.LocalizationService, new DeleteEventArgs<IDictionaryItem>(Enumerable.Empty<IDictionaryItem>())),

                new EventDefinition<IDataTypeService, SaveEventArgs<IDataTypeDefinition>>(null, ServiceContext.DataTypeService, new SaveEventArgs<IDataTypeDefinition>(Enumerable.Empty<IDataTypeDefinition>())),
                new EventDefinition<IDataTypeService, DeleteEventArgs<IDataTypeDefinition>>(null, ServiceContext.DataTypeService, new DeleteEventArgs<IDataTypeDefinition>(Enumerable.Empty<IDataTypeDefinition>())),

                new EventDefinition<IFileService, SaveEventArgs<Stylesheet>>(null, ServiceContext.FileService, new SaveEventArgs<Stylesheet>(Enumerable.Empty<Stylesheet>())),
                new EventDefinition<IFileService, DeleteEventArgs<Stylesheet>>(null, ServiceContext.FileService, new DeleteEventArgs<Stylesheet>(Enumerable.Empty<Stylesheet>())),

                new EventDefinition<IDomainService, SaveEventArgs<IDomain>>(null, ServiceContext.DomainService, new SaveEventArgs<IDomain>(Enumerable.Empty<IDomain>())),
                new EventDefinition<IDomainService, DeleteEventArgs<IDomain>>(null, ServiceContext.DomainService, new DeleteEventArgs<IDomain>(Enumerable.Empty<IDomain>())),

                new EventDefinition<ILocalizationService, SaveEventArgs<ILanguage>>(null, ServiceContext.LocalizationService, new SaveEventArgs<ILanguage>(Enumerable.Empty<ILanguage>())),
                new EventDefinition<ILocalizationService, DeleteEventArgs<ILanguage>>(null, ServiceContext.LocalizationService, new DeleteEventArgs<ILanguage>(Enumerable.Empty<ILanguage>())),

                new EventDefinition<IContentTypeService, SaveEventArgs<IContentType>>(null, ServiceContext.ContentTypeService, new SaveEventArgs<IContentType>(Enumerable.Empty<IContentType>())),
                new EventDefinition<IContentTypeService, DeleteEventArgs<IContentType>>(null, ServiceContext.ContentTypeService, new DeleteEventArgs<IContentType>(Enumerable.Empty<IContentType>())),
                new EventDefinition<IContentTypeService, SaveEventArgs<IMediaType>>(null, ServiceContext.ContentTypeService, new SaveEventArgs<IMediaType>(Enumerable.Empty<IMediaType>())),
                new EventDefinition<IContentTypeService, DeleteEventArgs<IMediaType>>(null, ServiceContext.ContentTypeService, new DeleteEventArgs<IMediaType>(Enumerable.Empty<IMediaType>())),

                new EventDefinition<IMemberTypeService, SaveEventArgs<IMemberType>>(null, ServiceContext.MemberTypeService, new SaveEventArgs<IMemberType>(Enumerable.Empty<IMemberType>())),
                new EventDefinition<IMemberTypeService, DeleteEventArgs<IMemberType>>(null, ServiceContext.MemberTypeService, new DeleteEventArgs<IMemberType>(Enumerable.Empty<IMemberType>())),

                new EventDefinition<IFileService, SaveEventArgs<ITemplate>>(null, ServiceContext.FileService, new SaveEventArgs<ITemplate>(Enumerable.Empty<ITemplate>())),
                new EventDefinition<IFileService, DeleteEventArgs<ITemplate>>(null, ServiceContext.FileService, new DeleteEventArgs<ITemplate>(Enumerable.Empty<ITemplate>())),

                new EventDefinition<IMacroService, SaveEventArgs<IMacro>>(null, ServiceContext.MacroService, new SaveEventArgs<IMacro>(Enumerable.Empty<IMacro>())),
                new EventDefinition<IMacroService, DeleteEventArgs<IMacro>>(null, ServiceContext.MacroService, new DeleteEventArgs<IMacro>(Enumerable.Empty<IMacro>())),

                new EventDefinition<IMemberService, SaveEventArgs<IMember>>(null, ServiceContext.MemberService, new SaveEventArgs<IMember>(Enumerable.Empty<IMember>())),
                new EventDefinition<IMemberService, DeleteEventArgs<IMember>>(null, ServiceContext.MemberService, new DeleteEventArgs<IMember>(Enumerable.Empty<IMember>())),

                new EventDefinition<IMemberGroupService, SaveEventArgs<IMemberGroup>>(null, ServiceContext.MemberGroupService, new SaveEventArgs<IMemberGroup>(Enumerable.Empty<IMemberGroup>())),
                new EventDefinition<IMemberGroupService, DeleteEventArgs<IMemberGroup>>(null, ServiceContext.MemberGroupService, new DeleteEventArgs<IMemberGroup>(Enumerable.Empty<IMemberGroup>())),

                new EventDefinition<IMediaService, SaveEventArgs<IMedia>>(null, ServiceContext.MediaService, new SaveEventArgs<IMedia>(Enumerable.Empty<IMedia>())),
                new EventDefinition<IMediaService, DeleteEventArgs<IMedia>>(null, ServiceContext.MediaService, new DeleteEventArgs<IMedia>(Enumerable.Empty<IMedia>())),
                new EventDefinition<IMediaService, MoveEventArgs<IMedia>>(null, ServiceContext.MediaService, new MoveEventArgs<IMedia>(new MoveEventInfo<IMedia>(null, "", -1)), "Moved"),
                new EventDefinition<IMediaService, MoveEventArgs<IMedia>>(null, ServiceContext.MediaService, new MoveEventArgs<IMedia>(new MoveEventInfo<IMedia>(null, "", -1)), "Trashed"),
                new EventDefinition<IMediaService, RecycleBinEventArgs>(null, ServiceContext.MediaService, new RecycleBinEventArgs(Guid.NewGuid(), new Dictionary<int, IEnumerable<Property>>(), true)),

                new EventDefinition<IContentService, SaveEventArgs<IContent>>(null, ServiceContext.ContentService, new SaveEventArgs<IContent>(Enumerable.Empty<IContent>()), "Saved"),
                new EventDefinition<IContentService, SaveEventArgs<IContent>>(null, ServiceContext.ContentService, new SaveEventArgs<IContent>(Enumerable.Empty<IContent>()), "SavedBlueprint"),
                new EventDefinition<IContentService, DeleteEventArgs<IContent>>(null, ServiceContext.ContentService, new DeleteEventArgs<IContent>(Enumerable.Empty<IContent>()), "Deleted"),
                new EventDefinition<IContentService, DeleteEventArgs<IContent>>(null, ServiceContext.ContentService, new DeleteEventArgs<IContent>(Enumerable.Empty<IContent>()), "DeletedBlueprint"),
                new EventDefinition<IContentService, CopyEventArgs<IContent>>(null, ServiceContext.ContentService, new CopyEventArgs<IContent>(null, null, -1)),
                new EventDefinition<IContentService, MoveEventArgs<IContent>>(null, ServiceContext.ContentService, new MoveEventArgs<IContent>(new MoveEventInfo<IContent>(null, "", -1)), "Trashed"),
                new EventDefinition<IContentService, RecycleBinEventArgs>(null, ServiceContext.ContentService, new RecycleBinEventArgs(Guid.NewGuid(), new Dictionary<int, IEnumerable<Property>>(), true)),
                new EventDefinition<IContentService, PublishEventArgs<IContent>>(null, ServiceContext.ContentService, new PublishEventArgs<IContent>(Enumerable.Empty<IContent>()), "Published"),
                new EventDefinition<IContentService, PublishEventArgs<IContent>>(null, ServiceContext.ContentService, new PublishEventArgs<IContent>(Enumerable.Empty<IContent>()), "UnPublished"),

                new EventDefinition<IPublicAccessService, SaveEventArgs<PublicAccessEntry>>(null, ServiceContext.PublicAccessService, new SaveEventArgs<PublicAccessEntry>(Enumerable.Empty<PublicAccessEntry>())),
                new EventDefinition<IPublicAccessService, DeleteEventArgs<PublicAccessEntry>>(null, ServiceContext.PublicAccessService, new DeleteEventArgs<PublicAccessEntry>(Enumerable.Empty<PublicAccessEntry>())),

                new EventDefinition<IRelationService, SaveEventArgs<IRelationType>>(null, ServiceContext.RelationService, new SaveEventArgs<IRelationType>(Enumerable.Empty<IRelationType>())),
                new EventDefinition<IRelationService, DeleteEventArgs<IRelationType>>(null, ServiceContext.RelationService, new DeleteEventArgs<IRelationType>(Enumerable.Empty<IRelationType>())),

                new EventDefinition<IRelationService, SaveEventArgs<IRelationType>>(null, ServiceContext.RelationService, new SaveEventArgs<IRelationType>(Enumerable.Empty<IRelationType>())),
                new EventDefinition<IRelationService, DeleteEventArgs<IRelationType>>(null, ServiceContext.RelationService, new DeleteEventArgs<IRelationType>(Enumerable.Empty<IRelationType>())),
            };

            foreach (var definition in definitions)
            {
                var found = CacheRefresherEventHandler.FindHandler(definition);
                Assert.IsNotNull(found, "Couldn't find method for " + definition.EventName + " on " + definition.Sender.GetType());
            }
            
        }
    }
}