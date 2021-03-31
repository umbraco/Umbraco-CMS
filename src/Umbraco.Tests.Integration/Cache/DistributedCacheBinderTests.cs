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
                new EventDefinition<IFileService, SaveEventArgs<IStylesheet>>(null, FileService, new SaveEventArgs<IStylesheet>(Enumerable.Empty<IStylesheet>())),
                new EventDefinition<IFileService, DeleteEventArgs<IStylesheet>>(null, FileService, new DeleteEventArgs<IStylesheet>(Enumerable.Empty<IStylesheet>())),

                new EventDefinition<IDomainService, SaveEventArgs<IDomain>>(null, DomainService, new SaveEventArgs<IDomain>(Enumerable.Empty<IDomain>())),
                new EventDefinition<IDomainService, DeleteEventArgs<IDomain>>(null, DomainService, new DeleteEventArgs<IDomain>(Enumerable.Empty<IDomain>())),

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

                // not managed
                //new EventDefinition<IContentService, SaveEventArgs<IContent>>(null, ContentService, new SaveEventArgs<IContent>(Enumerable.Empty<IContent>()), "SavedBlueprint"),
                //new EventDefinition<IContentService, DeleteEventArgs<IContent>>(null, ContentService, new DeleteEventArgs<IContent>(Enumerable.Empty<IContent>()), "DeletedBlueprint"),
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
