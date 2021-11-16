using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Changes;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Web;
using Umbraco.Web.Cache;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Cache
{
    [TestFixture]
    [UmbracoTest(WithApplication = true)]
    public class DistributedCacheBinderTests : UmbracoTestBase
    {
        protected override void Compose(Composition composition)
        {
            base.Compose(composition);
            // refreshers.HandleEvents wants a UmbracoContext
            // which wants these
            composition.RegisterUnique(_ => Mock.Of<IPublishedSnapshotService>());
            composition.WithCollectionBuilder<UrlProviderCollectionBuilder>();
        }

        [Test]
        public void Can_Find_All_Event_Handlers()
        {
            // FIXME: cannot work with mocks
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
                var found = DistributedCacheBinder.FindHandler(definition);
                if (found == null)
                {
                    Console.WriteLine("Couldn't find method for " + definition.EventName + " on " + definition.Sender.GetType());
                    ok = false;
                }
            }
            Assert.IsTrue(ok, "see log for details");
        }

        [Test]
        public void CanHandleEvent()
        {
            // refreshers.HandleEvents wants a UmbracoContext
            // which wants an HttpContext, which we build using a SimpleWorkerRequest
            // which requires these to be non-null
            var domain = Thread.GetDomain();
            if (domain.GetData(".appPath") == null)
                domain.SetData(".appPath", "");
            if (domain.GetData(".appVPath") == null)
                domain.SetData(".appVPath", "");

            // create some event definitions
            var definitions = new IEventDefinition[]
            {
                // works because that event definition maps to an empty handler
                new EventDefinition<IContentTypeService, SaveEventArgs<IContentType>>(null, Current.Services.ContentTypeService, new SaveEventArgs<IContentType>(Enumerable.Empty<IContentType>()), "Saved"),
            };

            var umbracoContextFactory = new UmbracoContextFactory(
                new TestUmbracoContextAccessor(),
                Mock.Of<IPublishedSnapshotService>(),
                new TestVariationContextAccessor(),
                new TestDefaultCultureAccessor(),
                TestObjects.GetUmbracoSettings(),
                TestObjects.GetGlobalSettings(),
                new UrlProviderCollection(Enumerable.Empty<IUrlProvider>()),
                new MediaUrlProviderCollection(Enumerable.Empty<IMediaUrlProvider>()),
                Mock.Of<IUserService>());

            // just assert it does not throw
            var refreshers = new DistributedCacheBinder(null, umbracoContextFactory, null);
            refreshers.HandleEvents(definitions);
        }

        [Test]
        public void GroupsContentTypeEvents()
        {
            var num = 30;
            var contentTypes = Enumerable.Repeat(MockedContentTypes.CreateBasicContentType(), num);
            var mediaTypes = Enumerable.Repeat(MockedContentTypes.CreateImageMediaType(), num);
            var memberTypes = Enumerable.Repeat(MockedContentTypes.CreateSimpleMemberType(), num);
            var definitionsContent = contentTypes.SelectMany(x => new IEventDefinition[]
            {
                new EventDefinition<IContentTypeService, ContentTypeChange<IContentType>.EventArgs>(null, Current.Services.ContentTypeService, new ContentTypeChange<IContentType>.EventArgs(new ContentTypeChange<IContentType>(x, ContentTypeChangeTypes.Create)), "Changed"),
                new EventDefinition<IContentTypeService, SaveEventArgs<IContentType>>(null, Current.Services.ContentTypeService, new SaveEventArgs<IContentType>(x), "Saved"),
            });

            var definitionsMedia = mediaTypes.SelectMany(x => new IEventDefinition[]
            {
                new EventDefinition<IMediaTypeService, ContentTypeChange<IMediaType>.EventArgs>(null, Current.Services.MediaTypeService, new ContentTypeChange<IMediaType>.EventArgs(new ContentTypeChange<IMediaType>(x, ContentTypeChangeTypes.Create)), "Changed"),
                new EventDefinition<IMediaTypeService, SaveEventArgs<IMediaType>>(null, Current.Services.MediaTypeService, new SaveEventArgs<IMediaType>(x), "Saved"),
            });
            var definitionsMember = memberTypes.SelectMany(x => new IEventDefinition[]
            {
                new EventDefinition<IMemberTypeService, ContentTypeChange<IMemberType>.EventArgs>(null, Current.Services.MemberTypeService, new ContentTypeChange<IMemberType>.EventArgs(new ContentTypeChange<IMemberType>(x, ContentTypeChangeTypes.Create)), "Changed"),
                new EventDefinition<IMemberTypeService, SaveEventArgs<IMemberType>>(null, Current.Services.MemberTypeService, new SaveEventArgs<IMemberType>(x), "Saved"),
            });

            var definitions = new List<IEventDefinition>();
            definitions.AddRange(definitionsContent);
            definitions.AddRange(definitionsMedia);
            definitions.AddRange(definitionsMember);

            var result = DistributedCacheBinder.GetGroupedEventList(definitions);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(num * 6, definitions.Count(), "Precondition is we have many definitions");
                Assert.AreEqual(6, result.Count(), "Unexpected number of reduced definitions");
                foreach (var eventDefinition in result)
                {
                    if (eventDefinition.Args is SaveEventArgs<IContentType> saveContentEventArgs)
                    {
                        Assert.AreEqual(num, saveContentEventArgs.SavedEntities.Count());
                    }

                    if (eventDefinition.Args is ContentTypeChange<IContentType>.EventArgs changeContentEventArgs)
                    {
                        Assert.AreEqual(num, changeContentEventArgs.Changes.Count());
                    }

                    if (eventDefinition.Args is SaveEventArgs<IMediaType> saveMediaEventArgs)
                    {
                        Assert.AreEqual(num, saveMediaEventArgs.SavedEntities.Count());
                    }

                    if (eventDefinition.Args is ContentTypeChange<IMediaType>.EventArgs changeMediaEventArgs)
                    {
                        Assert.AreEqual(num, changeMediaEventArgs.Changes.Count());
                    }

                    if (eventDefinition.Args is SaveEventArgs<IMemberType> saveMemberEventArgs)
                    {
                        Assert.AreEqual(num, saveMemberEventArgs.SavedEntities.Count());
                    }

                    if (eventDefinition.Args is ContentTypeChange<IMemberType>.EventArgs changeMemberEventArgs)
                    {
                        Assert.AreEqual(num, changeMemberEventArgs.Changes.Count());
                    }
                }
            });
        }
    }
}
