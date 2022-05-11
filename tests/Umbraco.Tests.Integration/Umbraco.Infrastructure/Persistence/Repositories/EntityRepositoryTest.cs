// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Constants = Umbraco.Cms.Core.Constants;

using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Mapper = true, Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class EntityRepositoryTest : UmbracoIntegrationTest
    {
        [Test]
        public void Get_Paged_Mixed_Entities_By_Ids()
        {
            // Create content
            IContentService contentService = GetRequiredService<IContentService>();
            IContentTypeService contentTypeService = GetRequiredService<IContentTypeService>();
            var createdContent = new List<IContent>();
            ContentType contentType = ContentTypeBuilder.CreateBasicContentType("blah");
            contentTypeService.Save(contentType);
            for (int i = 0; i < 10; i++)
            {
                Content c1 = ContentBuilder.CreateBasicContent(contentType);
                contentService.Save(c1);
                createdContent.Add(c1);
            }

            // Create media
            IMediaService mediaService = GetRequiredService<IMediaService>();
            IMediaTypeService mediaTypeService = GetRequiredService<IMediaTypeService>();
            var createdMedia = new List<IMedia>();
            MediaType imageType = MediaTypeBuilder.CreateImageMediaType("myImage");
            mediaTypeService.Save(imageType);
            for (int i = 0; i < 10; i++)
            {
                Media c1 = MediaBuilder.CreateMediaImage(imageType, -1);
                mediaService.Save(c1);
                createdMedia.Add(c1);
            }

            // Create members
            IMemberService memberService = GetRequiredService<IMemberService>();
            IMemberTypeService memberTypeService = GetRequiredService<IMemberTypeService>();
            MemberType memberType = MemberTypeBuilder.CreateSimpleMemberType("simple");
            memberTypeService.Save(memberType);
            var createdMembers = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10).ToList();
            memberService.Save(createdMembers);

            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope(autoComplete: true))
            {
                EntityRepository repo = CreateRepository((IScopeAccessor)provider);

                IEnumerable<int> ids = createdContent.Select(x => x.Id).Concat(createdMedia.Select(x => x.Id)).Concat(createdMembers.Select(x => x.Id));

                System.Guid[] objectTypes = new[] { Constants.ObjectTypes.Document, Constants.ObjectTypes.Media, Constants.ObjectTypes.Member };

                IQuery<IUmbracoEntity> query = provider.CreateQuery<IUmbracoEntity>()
                    .WhereIn(e => e.Id, ids);

                var entities = repo.GetPagedResultsByQuery(query, objectTypes, 0, 20, out long totalRecords, null, null).ToList();

                Assert.AreEqual(20, entities.Count);
                Assert.AreEqual(30, totalRecords);

                // add the next page
                entities.AddRange(repo.GetPagedResultsByQuery(query, objectTypes, 1, 20, out totalRecords, null, null));

                Assert.AreEqual(30, entities.Count);
                Assert.AreEqual(30, totalRecords);

                var contentEntities = entities.OfType<IDocumentEntitySlim>().ToList();
                var mediaEntities = entities.OfType<IMediaEntitySlim>().ToList();
                var memberEntities = entities.OfType<IMemberEntitySlim>().ToList();

                Assert.AreEqual(10, contentEntities.Count);
                Assert.AreEqual(10, mediaEntities.Count);
                Assert.AreEqual(10, memberEntities.Count);
            }
        }

        private EntityRepository CreateRepository(IScopeAccessor scopeAccessor) => new EntityRepository(scopeAccessor, AppCaches.Disabled);
    }
}
