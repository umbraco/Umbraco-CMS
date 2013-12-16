using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Tests.Services
{
    /// <summary>
    /// Tests covering the DataTypeService
    /// </summary>
    [TestFixture, RequiresSTA]
    public class DataTypeServiceTests : BaseServiceTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void DataTypeService_Can_Persist_New_DataTypeDefinition()
        {
            // Arrange
            var dataTypeService = ServiceContext.DataTypeService;
            var textfieldId = new Guid("ec15c1e5-9d90-422a-aa52-4f7622c63bea");

            // Act
            var dataTypeDefinition = new DataTypeDefinition(-1, textfieldId) { Name = "Testing Textfield", DatabaseType = DataTypeDatabaseType.Ntext };
            dataTypeService.Save(dataTypeDefinition);

            // Assert
            Assert.That(dataTypeDefinition, Is.Not.Null);
            Assert.That(dataTypeDefinition.HasIdentity, Is.True);
        }

        [Test]
        public void DataTypeService_Can_Delete_Textfield_DataType_And_Clear_Usages()
        {
            // Arrange
            var dataTypeService = ServiceContext.DataTypeService;
            var textfieldId = new Guid("ec15c1e5-9d90-422a-aa52-4f7622c63bea");
            var dataTypeDefinitions = dataTypeService.GetDataTypeDefinitionByControlId(textfieldId);
            
            // Act
            var definition = dataTypeDefinitions.First();
            var definitionId = definition.Id;
            dataTypeService.Delete(definition);

            var deletedDefinition = dataTypeService.GetDataTypeDefinitionById(definitionId);

            // Assert
            Assert.That(deletedDefinition, Is.Null);

            //Further assertions against the ContentType that contains PropertyTypes based on the TextField
            var contentType = ServiceContext.ContentTypeService.GetContentType(NodeDto.NodeIdSeed);
            Assert.That(contentType.Alias, Is.EqualTo("umbTextpage"));
            Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(1));
        }
    }
}