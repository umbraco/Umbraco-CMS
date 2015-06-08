using System;
using System.Data;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Repositories
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class DataTypeDefinitionRepositoryTest : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        private DataTypeDefinitionRepository CreateRepository(IDatabaseUnitOfWork unitOfWork)
        {
            var dataTypeDefinitionRepository = new DataTypeDefinitionRepository(
                unitOfWork, CacheHelper.CreateDisabledCacheHelper(),
                CacheHelper.CreateDisabledCacheHelper(),
                Mock.Of<ILogger>(), SqlSyntax,
                new ContentTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax,
                    new TemplateRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, Mock.Of<IFileSystem>(), Mock.Of<IFileSystem>(), Mock.Of<ITemplatesSection>())));
            return dataTypeDefinitionRepository;
        }

        [Test]
        public void Can_Create()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            int id;
            using (var repository = CreateRepository(unitOfWork))
            {
                var dataTypeDefinition = new DataTypeDefinition(-1, new Guid(Constants.PropertyEditors.RadioButtonList)) {Name = "test"};

                repository.AddOrUpdate(dataTypeDefinition);

                unitOfWork.Commit();
                id = dataTypeDefinition.Id;
                Assert.That(id, Is.GreaterThan(0));
            }
            using (var repository = CreateRepository(unitOfWork))
            {
                // Act
                var dataTypeDefinition = repository.Get(id);

                // Assert
                Assert.That(dataTypeDefinition, Is.Not.Null);
                Assert.That(dataTypeDefinition.HasIdentity, Is.True);
            }
        }

        [Test]
        public void Cannot_Create_Duplicate_Name()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            int id;
            using (var repository = CreateRepository(unitOfWork))
            {
                var dataTypeDefinition = new DataTypeDefinition(-1, new Guid(Constants.PropertyEditors.RadioButtonList)) { Name = "test" };
                repository.AddOrUpdate(dataTypeDefinition);
                unitOfWork.Commit();
                id = dataTypeDefinition.Id;
                Assert.That(id, Is.GreaterThan(0));
            }
            using (var repository = CreateRepository(unitOfWork))
            {
                var dataTypeDefinition = new DataTypeDefinition(-1, new Guid(Constants.PropertyEditors.RadioButtonList)) { Name = "test" };
                repository.AddOrUpdate(dataTypeDefinition);

                Assert.Throws<DuplicateNameException>(unitOfWork.Commit);
                
            }
        }

      

        [Test]
        public void Can_Perform_Get_On_DataTypeDefinitionRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                // Act
                var dataTypeDefinition = repository.Get(-42);

                // Assert
                Assert.That(dataTypeDefinition, Is.Not.Null);
                Assert.That(dataTypeDefinition.HasIdentity, Is.True);
                Assert.That(dataTypeDefinition.Name, Is.EqualTo("Dropdown"));    
            }
            
        }

        [Test]
        public void Can_Perform_GetAll_On_DataTypeDefinitionRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
                var dataTypeDefinitions = repository.GetAll();

                // Assert
                Assert.That(dataTypeDefinitions, Is.Not.Null);
                Assert.That(dataTypeDefinitions.Any(), Is.True);
                Assert.That(dataTypeDefinitions.Any(x => x == null), Is.False);
                Assert.That(dataTypeDefinitions.Count(), Is.EqualTo(25));
            }
        }

        [Test]
        public void Can_Perform_GetAll_With_Params_On_DataTypeDefinitionRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
                var dataTypeDefinitions = repository.GetAll(-40, -41, -42);

                // Assert
                Assert.That(dataTypeDefinitions, Is.Not.Null);
                Assert.That(dataTypeDefinitions.Any(), Is.True);
                Assert.That(dataTypeDefinitions.Any(x => x == null), Is.False);
                Assert.That(dataTypeDefinitions.Count(), Is.EqualTo(3));
            }
        }

        [Test]
        public void Can_Perform_GetByQuery_On_DataTypeDefinitionRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
            var query = Query<IDataTypeDefinition>.Builder.Where(x => x.PropertyEditorAlias == Constants.PropertyEditors.RadioButtonListAlias);
                var result = repository.GetByQuery(query);

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Any(), Is.True);
                Assert.That(result.FirstOrDefault().Name, Is.EqualTo("Radiobox"));
            }
        }

        [Test]
        public void Can_Perform_Count_On_DataTypeDefinitionRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
                var query = Query<IDataTypeDefinition>.Builder.Where(x => x.Name.StartsWith("D"));
                int count = repository.Count(query);

                // Assert
                Assert.That(count, Is.EqualTo(4));
            }
        }

        [Test]
        public void Can_Perform_Add_On_DataTypeDefinitionRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

            var dataTypeDefinition = new DataTypeDefinition("Test.TestEditor")
                    {
                        DatabaseType = DataTypeDatabaseType.Integer,
                        Name = "AgeDataType",
                        CreatorId = 0
                    };

                // Act
                repository.AddOrUpdate(dataTypeDefinition);
                unitOfWork.Commit();
                var exists = repository.Exists(dataTypeDefinition.Id);

                // Assert
                Assert.That(dataTypeDefinition.HasIdentity, Is.True);
                Assert.That(exists, Is.True);
            }
        }

        [Test]
        public void Can_Perform_Update_On_DataTypeDefinitionRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

            var dataTypeDefinition = new DataTypeDefinition("Test.blah")
                    {
                        DatabaseType = DataTypeDatabaseType.Integer,
                        Name = "AgeDataType",
                        CreatorId = 0
                    };
                repository.AddOrUpdate(dataTypeDefinition);
                unitOfWork.Commit();

                // Act
                var definition = repository.Get(dataTypeDefinition.Id);
                definition.Name = "AgeDataType Updated";
            definition.PropertyEditorAlias = "Test.TestEditor"; //change
                repository.AddOrUpdate(definition);
                unitOfWork.Commit();

                var definitionUpdated = repository.Get(dataTypeDefinition.Id);

                // Assert
                Assert.That(definitionUpdated, Is.Not.Null);
                Assert.That(definitionUpdated.Name, Is.EqualTo("AgeDataType Updated"));
            Assert.That(definitionUpdated.PropertyEditorAlias, Is.EqualTo("Test.TestEditor"));
            }
        }

        [Test]
        public void Can_Perform_Delete_On_DataTypeDefinitionRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

            var dataTypeDefinition = new DataTypeDefinition("Test.TestEditor")
                    {
                        DatabaseType = DataTypeDatabaseType.Integer,
                        Name = "AgeDataType",
                        CreatorId = 0
                    };

                // Act
                repository.AddOrUpdate(dataTypeDefinition);
                unitOfWork.Commit();
                var existsBefore = repository.Exists(dataTypeDefinition.Id);

                repository.Delete(dataTypeDefinition);
                unitOfWork.Commit();

                var existsAfter = repository.Exists(dataTypeDefinition.Id);

                // Assert
                Assert.That(existsBefore, Is.True);
                Assert.That(existsAfter, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Exists_On_DataTypeDefinitionRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
                var exists = repository.Exists(1034); //Content picker
                var doesntExist = repository.Exists(-80);

                // Assert
                Assert.That(exists, Is.True);
                Assert.That(doesntExist, Is.False);
            }
        }

        [Test]
        public void Can_Get_Pre_Value_Collection()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();

            int dtid;
            using (var repository = CreateRepository(unitOfWork))
            {
                var dataTypeDefinition = new DataTypeDefinition(-1, new Guid(Constants.PropertyEditors.RadioButtonList)) { Name = "test" };
                repository.AddOrUpdate(dataTypeDefinition);
                unitOfWork.Commit();
                dtid = dataTypeDefinition.Id;
            }

            DatabaseContext.Database.Insert(new DataTypePreValueDto() { DataTypeNodeId = dtid, SortOrder = 0, Value = "test1"});
            DatabaseContext.Database.Insert(new DataTypePreValueDto() { DataTypeNodeId = dtid, SortOrder = 1, Value = "test2" });

            using (var repository = CreateRepository(unitOfWork))
            {
                var collection = repository.GetPreValuesCollectionByDataTypeId(dtid);
                Assert.AreEqual(2, collection.PreValuesAsArray.Count());
            }
        }

        [Test]
        public void Can_Get_Pre_Value_As_String()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();

            int dtid;
            using (var repository = CreateRepository(unitOfWork))
            {
                var dataTypeDefinition = new DataTypeDefinition(-1, new Guid(Constants.PropertyEditors.RadioButtonList)) { Name = "test" };
                repository.AddOrUpdate(dataTypeDefinition);
                unitOfWork.Commit();
                dtid = dataTypeDefinition.Id;
            }

            var id = DatabaseContext.Database.Insert(new DataTypePreValueDto() { DataTypeNodeId = dtid, SortOrder = 0, Value = "test1" });
            DatabaseContext.Database.Insert(new DataTypePreValueDto() { DataTypeNodeId = dtid, SortOrder = 1, Value = "test2" });

            using (var repository = CreateRepository(unitOfWork))
            {
                var val = repository.GetPreValueAsString(Convert.ToInt32(id));
                Assert.AreEqual("test1", val);
            }
        }

        [Test]
        public void Can_Get_Pre_Value_Collection_With_Cache()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();

            var cache = new CacheHelper(new ObjectCacheRuntimeCacheProvider(), new StaticCacheProvider(), new StaticCacheProvider());

            Func<DataTypeDefinitionRepository> creator = () => new DataTypeDefinitionRepository(
                unitOfWork, CacheHelper.CreateDisabledCacheHelper(),
                cache,
                Mock.Of<ILogger>(), SqlSyntax,
                new ContentTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax,
                    new TemplateRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, Mock.Of<IFileSystem>(), Mock.Of<IFileSystem>(), Mock.Of<ITemplatesSection>())));

            DataTypeDefinition dtd;
            using (var repository = creator())
            {
                dtd = new DataTypeDefinition(-1, new Guid(Constants.PropertyEditors.RadioButtonList)) { Name = "test" };
                repository.AddOrUpdate(dtd);
                unitOfWork.Commit();                
            }

            DatabaseContext.Database.Insert(new DataTypePreValueDto() { DataTypeNodeId = dtd.Id, SortOrder = 0, Value = "test1" });
            DatabaseContext.Database.Insert(new DataTypePreValueDto() { DataTypeNodeId = dtd.Id, SortOrder = 1, Value = "test2" });

            //this will cache the result
            using (var repository = creator())
            {
                var collection = repository.GetPreValuesCollectionByDataTypeId(dtd.Id);
            }

            var cached = cache.RuntimeCache.GetCacheItemsByKeySearch<PreValueCollection>(CacheKeys.DataTypePreValuesCacheKey + dtd.Id + "-");

            Assert.IsNotNull(cached);
            Assert.AreEqual(1, cached.Count());
            Assert.AreEqual(2, cached.Single().FormatAsDictionary().Count);
        }

        [Test]
        public void Can_Get_Pre_Value_As_String_With_Cache()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();

            var cache = new CacheHelper(new ObjectCacheRuntimeCacheProvider(), new StaticCacheProvider(), new StaticCacheProvider());

            Func<DataTypeDefinitionRepository> creator = () => new DataTypeDefinitionRepository(
                unitOfWork, CacheHelper.CreateDisabledCacheHelper(),
                cache,
                Mock.Of<ILogger>(), SqlSyntax,
                new ContentTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax,
                    new TemplateRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, Mock.Of<IFileSystem>(), Mock.Of<IFileSystem>(), Mock.Of<ITemplatesSection>())));

            DataTypeDefinition dtd;
            using (var repository = creator())
            {
                dtd = new DataTypeDefinition(-1, new Guid(Constants.PropertyEditors.RadioButtonList)) { Name = "test" };
                repository.AddOrUpdate(dtd);
                unitOfWork.Commit();
            }

            var id = DatabaseContext.Database.Insert(new DataTypePreValueDto() { DataTypeNodeId = dtd.Id, SortOrder = 0, Value = "test1" });
            DatabaseContext.Database.Insert(new DataTypePreValueDto() { DataTypeNodeId = dtd.Id, SortOrder = 1, Value = "test2" });

            //this will cache the result
            using (var repository = creator())
            {
                var val = repository.GetPreValueAsString(Convert.ToInt32(id));
            }

            var cached = cache.RuntimeCache.GetCacheItemsByKeySearch<PreValueCollection>(CacheKeys.DataTypePreValuesCacheKey + dtd.Id + "-");

            Assert.IsNotNull(cached);
            Assert.AreEqual(1, cached.Count());
            Assert.AreEqual(2, cached.Single().FormatAsDictionary().Count);

            using (var repository = creator())
            {
                //ensure it still gets resolved!
                var val = repository.GetPreValueAsString(Convert.ToInt32(id));
                Assert.AreEqual("test1", val);
            }
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }
    }
}