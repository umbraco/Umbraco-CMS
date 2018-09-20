using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.Services
{
    /// <summary>
    /// Tests covering the DataTypeService
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class DataTypeServiceTests : TestWithSomeContentBase
    {
        [Test]
        public void DataTypeService_Can_Persist_New_DataTypeDefinition()
        {
            // Arrange
            var dataTypeService = ServiceContext.DataTypeService;

            // Act
            IDataType dataType = new DataType(new LabelPropertyEditor(Logger)) { Name = "Testing Textfield", DatabaseType = ValueStorageType.Ntext };
            dataTypeService.Save(dataType);

            // Assert
            Assert.That(dataType, Is.Not.Null);
            Assert.That(dataType.HasIdentity, Is.True);

            dataType = dataTypeService.GetDataType(dataType.Id);
            Assert.That(dataType, Is.Not.Null);
        }

        [Test]
        public void DataTypeService_Can_Delete_Textfield_DataType_And_Clear_Usages()
        {
            // Arrange
            var dataTypeService = ServiceContext.DataTypeService;
            var textfieldId = "Umbraco.Textbox";
            var dataTypeDefinitions = dataTypeService.GetByEditorAlias(textfieldId);

            // Act
            var definition = dataTypeDefinitions.First();
            var definitionId = definition.Id;
            dataTypeService.Delete(definition);

            var deletedDefinition = dataTypeService.GetDataType(definitionId);

            // Assert
            Assert.That(deletedDefinition, Is.Null);

            //Further assertions against the ContentType that contains PropertyTypes based on the TextField
            var contentType = ServiceContext.ContentTypeService.Get(NodeDto.NodeIdSeed+1);
            Assert.That(contentType.Alias, Is.EqualTo("umbTextpage"));
            Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Cannot_Save_DataType_With_Empty_Name()
        {
            // Arrange
            var dataTypeService = ServiceContext.DataTypeService;

            // Act
            var dataTypeDefinition = new DataType(new LabelPropertyEditor(Logger)) { Name = string.Empty, DatabaseType = ValueStorageType.Ntext };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => dataTypeService.Save(dataTypeDefinition));
        }
    }
}
