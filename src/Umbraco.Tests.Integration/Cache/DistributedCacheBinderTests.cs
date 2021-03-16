using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Cache
{
    [TestFixture]
    [UmbracoTest(Boot = true)]
    public class DistributedCacheBinderTests : UmbracoIntegrationTest
    {
        private IUserService UserService => GetRequiredService<IUserService>();
        private ILocalizationService LocalizationService => GetRequiredService<ILocalizationService>();
        private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();
        private IFileService FileService => GetRequiredService<IFileService>();
        private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();
        private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();
        private IDomainService DomainService => GetRequiredService<IDomainService>();
        private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();
        private IMacroService MacroService => GetRequiredService<IMacroService>();
        private IMemberService MemberService => GetRequiredService<IMemberService>();
        private IMemberGroupService MemberGroupService => GetRequiredService<IMemberGroupService>();
        private IMediaService MediaService => GetRequiredService<IMediaService>();
        private IContentService ContentService => GetRequiredService<IContentService>();
        private IPublicAccessService PublicAccessService => GetRequiredService<IPublicAccessService>();
        private IRelationService RelationService => GetRequiredService<IRelationService>();
        private UriUtility UriUtility => GetRequiredService<UriUtility>();
        private IUmbracoContextFactory UmbracoContextFactory => GetRequiredService<IUmbracoContextFactory>();

        [Test]
        public void Can_Find_All_Event_Handlers()
        {

            var definitions = new IEventDefinition[]
            {
                new EventDefinition<IUserService, SaveEventArgs<IUser>>(null, UserService, new SaveEventArgs<IUser>(Enumerable.Empty<IUser>())),
                new EventDefinition<IUserService, DeleteEventArgs<IUser>>(null, UserService, new DeleteEventArgs<IUser>(Enumerable.Empty<IUser>())),
                new EventDefinition<IUserService, SaveEventArgs<UserGroupWithUsers>>(null, UserService, new SaveEventArgs<UserGroupWithUsers>(Enumerable.Empty<UserGroupWithUsers>())),
                new EventDefinition<IUserService, DeleteEventArgs<IUserGroup>>(null, UserService, new DeleteEventArgs<IUserGroup>(Enumerable.Empty<IUserGroup>())),

                new EventDefinition<ILocalizationService, SaveEventArgs<IDictionaryItem>>(null, LocalizationService, new SaveEventArgs<IDictionaryItem>(Enumerable.Empty<IDictionaryItem>())),
                new EventDefinition<ILocalizationService, DeleteEventArgs<IDictionaryItem>>(null, LocalizationService, new DeleteEventArgs<IDictionaryItem>(Enumerable.Empty<IDictionaryItem>())),

                new EventDefinition<IDataTypeService, SaveEventArgs<IDataType>>(null, DataTypeService, new SaveEventArgs<IDataType>(Enumerable.Empty<IDataType>())),
                new EventDefinition<IDataTypeService, DeleteEventArgs<IDataType>>(null, DataTypeService, new DeleteEventArgs<IDataType>(Enumerable.Empty<IDataType>())),

                new EventDefinition<IFileService, SaveEventArgs<IStylesheet>>(null, FileService, new SaveEventArgs<IStylesheet>(Enumerable.Empty<IStylesheet>())),
                new EventDefinition<IFileService, DeleteEventArgs<IStylesheet>>(null, FileService, new DeleteEventArgs<IStylesheet>(Enumerable.Empty<IStylesheet>())),

                new EventDefinition<IDomainService, SaveEventArgs<IDomain>>(null, DomainService, new SaveEventArgs<IDomain>(Enumerable.Empty<IDomain>())),
                new EventDefinition<IDomainService, DeleteEventArgs<IDomain>>(null, DomainService, new DeleteEventArgs<IDomain>(Enumerable.Empty<IDomain>())),

                new EventDefinition<ILocalizationService, SaveEventArgs<ILanguage>>(null, LocalizationService, new SaveEventArgs<ILanguage>(Enumerable.Empty<ILanguage>())),
                new EventDefinition<ILocalizationService, DeleteEventArgs<ILanguage>>(null, LocalizationService, new DeleteEventArgs<ILanguage>(Enumerable.Empty<ILanguage>())),

                new EventDefinition<IContentTypeService, SaveEventArgs<IContentType>>(null, ContentTypeService, new SaveEventArgs<IContentType>(Enumerable.Empty<IContentType>())),
                new EventDefinition<IContentTypeService, DeleteEventArgs<IContentType>>(null, ContentTypeService, new DeleteEventArgs<IContentType>(Enumerable.Empty<IContentType>())),
                new EventDefinition<IMediaTypeService, SaveEventArgs<IMediaType>>(null, MediaTypeService, new SaveEventArgs<IMediaType>(Enumerable.Empty<IMediaType>())),
                new EventDefinition<IMediaTypeService, DeleteEventArgs<IMediaType>>(null, MediaTypeService, new DeleteEventArgs<IMediaType>(Enumerable.Empty<IMediaType>())),

                new EventDefinition<IMemberTypeService, SaveEventArgs<IMemberType>>(null, MemberTypeService, new SaveEventArgs<IMemberType>(Enumerable.Empty<IMemberType>())),
                new EventDefinition<IMemberTypeService, DeleteEventArgs<IMemberType>>(null, MemberTypeService, new DeleteEventArgs<IMemberType>(Enumerable.Empty<IMemberType>())),

                new EventDefinition<IFileService, SaveEventArgs<ITemplate>>(null, FileService, new SaveEventArgs<ITemplate>(Enumerable.Empty<ITemplate>())),
                new EventDefinition<IFileService, DeleteEventArgs<ITemplate>>(null, FileService, new DeleteEventArgs<ITemplate>(Enumerable.Empty<ITemplate>())),

                new EventDefinition<IMacroService, SaveEventArgs<IMacro>>(null, MacroService, new SaveEventArgs<IMacro>(Enumerable.Empty<IMacro>())),
                new EventDefinition<IMacroService, DeleteEventArgs<IMacro>>(null, MacroService, new DeleteEventArgs<IMacro>(Enumerable.Empty<IMacro>())),

                new EventDefinition<IMemberService, SaveEventArgs<IMember>>(null, MemberService, new SaveEventArgs<IMember>(Enumerable.Empty<IMember>())),
                new EventDefinition<IMemberService, DeleteEventArgs<IMember>>(null, MemberService, new DeleteEventArgs<IMember>(Enumerable.Empty<IMember>())),

                new EventDefinition<IMemberGroupService, SaveEventArgs<IMemberGroup>>(null, MemberGroupService, new SaveEventArgs<IMemberGroup>(Enumerable.Empty<IMemberGroup>())),
                new EventDefinition<IMemberGroupService, DeleteEventArgs<IMemberGroup>>(null, MemberGroupService, new DeleteEventArgs<IMemberGroup>(Enumerable.Empty<IMemberGroup>())),

                // not managed
                //new EventDefinition<IContentService, SaveEventArgs<IContent>>(null, ContentService, new SaveEventArgs<IContent>(Enumerable.Empty<IContent>()), "SavedBlueprint"),
                //new EventDefinition<IContentService, DeleteEventArgs<IContent>>(null, ContentService, new DeleteEventArgs<IContent>(Enumerable.Empty<IContent>()), "DeletedBlueprint"),

                new EventDefinition<IPublicAccessService, SaveEventArgs<PublicAccessEntry>>(null, PublicAccessService, new SaveEventArgs<PublicAccessEntry>(Enumerable.Empty<PublicAccessEntry>())),
                new EventDefinition<IPublicAccessService, DeleteEventArgs<PublicAccessEntry>>(null, PublicAccessService, new DeleteEventArgs<PublicAccessEntry>(Enumerable.Empty<PublicAccessEntry>())),

                new EventDefinition<IRelationService, SaveEventArgs<IRelationType>>(null, RelationService, new SaveEventArgs<IRelationType>(Enumerable.Empty<IRelationType>())),
                new EventDefinition<IRelationService, DeleteEventArgs<IRelationType>>(null, RelationService, new DeleteEventArgs<IRelationType>(Enumerable.Empty<IRelationType>())),

                new EventDefinition<IRelationService, SaveEventArgs<IRelationType>>(null, RelationService, new SaveEventArgs<IRelationType>(Enumerable.Empty<IRelationType>())),
                new EventDefinition<IRelationService, DeleteEventArgs<IRelationType>>(null, RelationService, new DeleteEventArgs<IRelationType>(Enumerable.Empty<IRelationType>())),
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
                new EventDefinition<IContentTypeService, SaveEventArgs<IContentType>>(null, ContentTypeService, new SaveEventArgs<IContentType>(Enumerable.Empty<IContentType>()), "Saved"),

            };

            Assert.DoesNotThrow(() =>
            {
                var refreshers = new DistributedCacheBinder(null, UmbracoContextFactory, null);
                refreshers.HandleEvents(definitions);
            });

        }
    }
}
