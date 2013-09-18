using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Repositories
{
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
            var dataTypeDefinitionRepository = new DataTypeDefinitionRepository(unitOfWork, NullCacheProvider.Current);
            return dataTypeDefinitionRepository;
        }

        [Test]
        public void Can_Instantiate_Repository_From_Resolver()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            var repository = RepositoryResolver.Current.ResolveByType<IDataTypeDefinitionRepository>(unitOfWork);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Perform_Get_On_DataTypeDefinitionRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
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
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
                var dataTypeDefinitions = repository.GetAll();

                // Assert
                Assert.That(dataTypeDefinitions, Is.Not.Null);
                Assert.That(dataTypeDefinitions.Any(), Is.True);
                Assert.That(dataTypeDefinitions.Any(x => x == null), Is.False);
                Assert.That(dataTypeDefinitions.Count(), Is.EqualTo(24));
            }
        }

        [Test]
        public void Can_Perform_GetAll_With_Params_On_DataTypeDefinitionRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
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
            var provider = new PetaPocoUnitOfWorkProvider();
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
            var provider = new PetaPocoUnitOfWorkProvider();
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
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

            var dataTypeDefinition = new DataTypeDefinition(-1, "Test.TestEditor")
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
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

            var dataTypeDefinition = new DataTypeDefinition(-1, "Test.blah")
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
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

            var dataTypeDefinition = new DataTypeDefinition(-1, "Test.TestEditor")
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
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
                var exists = repository.Exists(1042); //Macro Container
                var doesntExist = repository.Exists(-80);

                // Assert
                Assert.That(exists, Is.True);
                Assert.That(doesntExist, Is.False);
            }
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }
    }
}