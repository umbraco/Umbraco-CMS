using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Mapper = true, Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class EntityRepositoryTest : TestWithDatabaseBase
    {

        private EntityRepository CreateRepository(IScopeAccessor scopeAccessor)
        {
            var entityRepository = new EntityRepository(scopeAccessor);
            return entityRepository;
        }

        [Test]
        public void Get_Paged_Mixed_Entities_By_Ids()
        {
            //Create content

            var createdContent = new List<IContent>();
            var contentType = MockedContentTypes.CreateBasicContentType("blah");
            ServiceContext.ContentTypeService.Save(contentType);            
            for (int i = 0; i < 10; i++)
            {
                var c1 = MockedContent.CreateBasicContent(contentType);
                ServiceContext.ContentService.Save(c1);
                createdContent.Add(c1);
            }

            //Create media

            var createdMedia = new List<IMedia>();
            var imageType = MockedContentTypes.CreateImageMediaType("myImage");
            ServiceContext.MediaTypeService.Save(imageType);
            for (int i = 0; i < 10; i++)
            {
                var c1 = MockedMedia.CreateMediaImage(imageType, -1);
                ServiceContext.MediaService.Save(c1);
                createdMedia.Add(c1);
            }

            // Create members
            var memberType = MockedContentTypes.CreateSimpleMemberType("simple");
            ServiceContext.MemberTypeService.Save(memberType);
            var createdMembers = MockedMember.CreateSimpleMember(memberType, 10).ToList();
            ServiceContext.MemberService.Save(createdMembers);

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repo = CreateRepository((IScopeAccessor)provider);

                var ids = createdContent.Select(x => x.Id).Concat(createdMedia.Select(x => x.Id)).Concat(createdMembers.Select(x => x.Id));

                var objectTypes = new[] { Constants.ObjectTypes.Document, Constants.ObjectTypes.Media, Constants.ObjectTypes.Member };

                var query = SqlContext.Query<IUmbracoEntity>()
                    .WhereIn(e => e.Id, ids);

                var entities = repo.GetPagedResultsByQuery(query, objectTypes, 0, 20, out var totalRecords, null, null).ToList();
                
                Assert.AreEqual(20, entities.Count);
                Assert.AreEqual(30, totalRecords);

                //add the next page
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

    }
}
