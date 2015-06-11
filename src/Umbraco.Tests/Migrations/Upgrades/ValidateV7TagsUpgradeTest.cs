using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Migrations.Upgrades
{
    [NUnit.Framework.Ignore("This won't work because it is tested against the v6 database but once we upgrade the TagRelationshipDto to have the new cols the old cols wont exist in this test. Rest assured that this test did pass before I modified TagRelationshipDto. Just not sure how to force a legacy db to be created for a test.")]
    [TestFixture]
    public class ValidateV7TagsUpgradeTest : BaseDatabaseFactoryTest
    {
        private ContentRepository CreateRepository(IDatabaseUnitOfWork unitOfWork, out ContentTypeRepository contentTypeRepository)
        {
            var templateRepository = new TemplateRepository(unitOfWork, NullCacheProvider.Current);
            var tagRepository = new TagRepository(unitOfWork, NullCacheProvider.Current);
            contentTypeRepository = new ContentTypeRepository(unitOfWork, NullCacheProvider.Current, templateRepository);
            var repository = new ContentRepository(unitOfWork, NullCacheProvider.Current, contentTypeRepository, templateRepository, tagRepository, CacheHelper.CreateDisabledCacheHelper());
            return repository;
        }

        [Test]
        public void Validate_Data_Upgrade()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            var insertedContent = new List<IContent>();
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                //we need to populate some data to upgrade
                var contentTypeWith1Tag = MockedContentTypes.CreateSimpleContentType(
                    "tags1", "tags1",
                    new PropertyTypeCollection(new[]
                    {
                        new PropertyType("test", DataTypeDatabaseType.Ntext, "tags1") {Name = "tags1", SortOrder = 1, DataTypeDefinitionId = 1041},
                    }));
                var contentTypeWith2Tags = MockedContentTypes.CreateSimpleContentType(
                    "tags2", "tags2",
                    new PropertyTypeCollection(new[]
                    {
                        new PropertyType("test", DataTypeDatabaseType.Ntext, "tags1") {Name = "tags1", SortOrder = 1, DataTypeDefinitionId = 1041},
                        new PropertyType("test", DataTypeDatabaseType.Ntext, "tags2") {Name = "tags2", SortOrder = 1, DataTypeDefinitionId = 1041}
                    }));

                contentTypeRepository.AddOrUpdate(contentTypeWith1Tag);
                contentTypeRepository.AddOrUpdate(contentTypeWith2Tags);
                unitOfWork.Commit();

                for (var i = 0; i < 10; i++)
                {
                    var content = new Content("test" + i, -1, contentTypeWith1Tag) { Language = "en-US", CreatorId = 0, WriterId = 0 };
                    var obj = new
                    {
                        tags1 = "tag1,tag2,tag3,tag4,tag5"                            
                    };
                    content.PropertyValues(obj);
                    content.ResetDirtyProperties(false);
                    insertedContent.Add(content);
                    repository.AddOrUpdate(content);
                }
                for (var i = 0; i < 10; i++)
                {
                    var content = new Content("test-multi" + i, -1, contentTypeWith2Tags) { Language = "en-US", CreatorId = 0, WriterId = 0 };
                    var obj = new
                    {
                        //NOTE: These will always be the same during an upgrade since we can only support tags per document not per property
                        tags1 = "tag1,tag2,tag3,anothertag1,anothertag2",
                        tags2 = "tag1,tag2,tag3,anothertag1,anothertag2"
                    };
                    content.PropertyValues(obj);
                    content.ResetDirtyProperties(false);
                    insertedContent.Add(content);
                    repository.AddOrUpdate(content);
                }
                unitOfWork.Commit();
            }
            //now that we have to create some test tag data
            foreach (var tag in "tag1,tag2,tag3,tag4,tag5,anothertag1,anothertag2".Split(','))
            {
                DatabaseContext.Database.Insert(new TagDto {Tag = tag, Group = "default"});
            }
            var alltags = DatabaseContext.Database.Fetch<TagDto>("SELECT * FROM cmsTags").ToArray();
            foreach (var content in insertedContent)
            {
                if (content.ContentType.Alias == "tags1")
                {
                    var tags1Tags = alltags.Where(x => "tag1,tag2,tag3,tag4,tag5".Split(',').Contains(x.Tag));
                    foreach (var t in tags1Tags)
                    {
                        DatabaseContext.Database.Insert(new TagRelationshipDto {NodeId = content.Id, TagId = t.Id});
                    }                    
                }
                else
                {
                    var tags1Tags = alltags.Where(x => "tag1,tag2,tag3,anothertag1,anothertag2".Split(',').Contains(x.Tag));
                    foreach (var t in tags1Tags)
                    {
                        DatabaseContext.Database.Insert(new TagRelationshipDto { NodeId = content.Id, TagId = t.Id });
                    }   
                }
            }
            
            //lastly, we'll insert a tag relation with a relation to only an umbracoNode - 
            // this will generate a delete clause and a warning
            DatabaseContext.Database.Insert(new TagRelationshipDto { NodeId = -1, TagId = alltags.First().Id });


            var migration = new AlterTagRelationsTable();
            var migrationContext = new MigrationContext(DatabaseProviders.SqlServerCE, DatabaseContext.Database);
            migration.GetUpExpressions(migrationContext);

            Assert.AreEqual(
                (10 * 5) //the docs that only have 1 tag prop per document
                + (10 * 5) //the docs that have 2 tag prop per document - these are the update statements
                + (10 * 5) //the docs that have 2 tag prop per document - these are the insert statements
                + 1//the delete clause
                + 7 , //additional db expressions
                migrationContext.Expressions.Count);
        }
    }
}