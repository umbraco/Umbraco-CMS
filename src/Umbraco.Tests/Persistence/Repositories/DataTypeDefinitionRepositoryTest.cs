using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
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

        [Test]
        public void Can_Instantiate_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            var repository = new DataTypeDefinitionRepository(unitOfWork);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Perform_Get_On_DataTypeDefinitionRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new DataTypeDefinitionRepository(unitOfWork);

            // Act
            var dataTypeDefinition = repository.Get(-42);

            // Assert
            Assert.That(dataTypeDefinition, Is.Not.Null);
            Assert.That(dataTypeDefinition.HasIdentity, Is.True);
            Assert.That(dataTypeDefinition.Name, Is.EqualTo("Dropdown"));
        }

        [Test]
        public void Can_Perform_GetAll_On_DataTypeDefinitionRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new DataTypeDefinitionRepository(unitOfWork);

            // Act
            var dataTypeDefinitions = repository.GetAll();

            // Assert
            Assert.That(dataTypeDefinitions, Is.Not.Null);
            Assert.That(dataTypeDefinitions.Any(), Is.True);
            Assert.That(dataTypeDefinitions.Any(x => x == null), Is.False);
            Assert.That(dataTypeDefinitions.Count(), Is.EqualTo(24));
        }

        [Test]
        public void Can_Perform_GetAll_With_Params_On_DataTypeDefinitionRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new DataTypeDefinitionRepository(unitOfWork);

            // Act
            var dataTypeDefinitions = repository.GetAll(-40, -41, -42);

            // Assert
            Assert.That(dataTypeDefinitions, Is.Not.Null);
            Assert.That(dataTypeDefinitions.Any(), Is.True);
            Assert.That(dataTypeDefinitions.Any(x => x == null), Is.False);
            Assert.That(dataTypeDefinitions.Count(), Is.EqualTo(3));
        }

        [Test]
        public void Can_Perform_GetByQuery_On_DataTypeDefinitionRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new DataTypeDefinitionRepository(unitOfWork, NullCacheProvider.Current);

            // Act
            var query = Query<IDataTypeDefinition>.Builder.Where(x => x.ControlId == new Guid(Constants.PropertyEditors.RadioButtonList));
            var result = repository.GetByQuery(query);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Any(), Is.True);
            Assert.That(result.FirstOrDefault().Name, Is.EqualTo("Radiobox"));
        }

        [Test]
        public void Can_Perform_Count_On_DataTypeDefinitionRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new DataTypeDefinitionRepository(unitOfWork);

            // Act
            var query = Query<IDataTypeDefinition>.Builder.Where(x => x.Name.StartsWith("D"));
            int count = repository.Count(query);

            // Assert
            Assert.That(count, Is.EqualTo(4));
        }

        [Test]
        public void Can_Perform_Add_On_DataTypeDefinitionRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new DataTypeDefinitionRepository(unitOfWork);

            var dataTypeDefinition = new DataTypeDefinition(-1, new Guid("0FE4B127-D48C-4807-8371-67FC2A0E27D7"))
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

        [Test]
        public void Can_Perform_Update_On_DataTypeDefinitionRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new DataTypeDefinitionRepository(unitOfWork);

            var dataTypeDefinition = new DataTypeDefinition(-1, new Guid("0FE4B127-D48C-4807-8371-67FC2A0E27D7"))
                                         {
                                             DatabaseType = DataTypeDatabaseType.Integer,
                                             Name = "AgeDataType",
                                             CreatorId = 0
                                         };
            repository.AddOrUpdate(dataTypeDefinition);
            unitOfWork.Commit();

            // Act
            var newId = Guid.NewGuid();
            var definition = repository.Get(dataTypeDefinition.Id);
            definition.Name = "AgeDataType Updated";
            definition.ControlId = newId;
            repository.AddOrUpdate(definition);
            unitOfWork.Commit();

            var definitionUpdated = repository.Get(dataTypeDefinition.Id);

            // Assert
            Assert.That(definitionUpdated, Is.Not.Null);
            Assert.That(definitionUpdated.Name, Is.EqualTo("AgeDataType Updated"));
            Assert.That(definitionUpdated.ControlId, Is.EqualTo(newId));
        }

        [Test]
        public void Can_Perform_Delete_On_DataTypeDefinitionRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new DataTypeDefinitionRepository(unitOfWork);

            var dataTypeDefinition = new DataTypeDefinition(-1, new Guid("0FE4B127-D48C-4807-8371-67FC2A0E27D7"))
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

        [Test]
        public void Can_Perform_Exists_On_DataTypeDefinitionRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new DataTypeDefinitionRepository(unitOfWork);

            // Act
            var exists = repository.Exists(1042);//Macro Container
            var doesntExist = repository.Exists(-80);

            // Assert
            Assert.That(exists, Is.True);
            Assert.That(doesntExist, Is.False);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }
    }
}