using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{
    /// <summary>
    /// Tests covering the DataTypeService
    /// </summary>
    [TestFixture, RequiresSTA]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class DataTypeServiceTests : TestWithSomeContentBase
    {
        [Test]
        public void DataTypeService_Can_Persist_New_DataTypeDefinition()
        {
            // Arrange
            var dataTypeService = ServiceContext.DataTypeService;

            // Act
            IDataType dataType = new DataType(-1, "Test.TestEditor") { Name = "Testing Textfield", DatabaseType = DataTypeDatabaseType.Ntext };
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
        public void DataTypeService_Can_Persist_Dictionary_Based_Pre_Values()
        {
            // Arrange
            var dataTypeService = ServiceContext.DataTypeService;
            var textBoxAlias = Constants.PropertyEditors.TextboxAlias;

            // Act
            IDataType dataType = new DataType(-1, textBoxAlias) { Name = "Testing prevals", DatabaseType = DataTypeDatabaseType.Ntext };
            dataTypeService.Save(dataType);
            dataTypeService.SavePreValues(dataType, new Dictionary<string, PreValue>
                {
                    {"preVal1", new PreValue("Hello")},
                    {"preVal2", new PreValue("World")}
                });
            //re-get
            dataType = dataTypeService.GetDataType(dataType.Id);
            var preVals = dataTypeService.GetPreValuesCollectionByDataTypeId(dataType.Id);

            // Assert
            Assert.That(dataType, Is.Not.Null);
            Assert.That(dataType.HasIdentity, Is.True);
            Assert.AreEqual(true, preVals.IsDictionaryBased);
            Assert.AreEqual(2, preVals.PreValuesAsDictionary.Keys.Count);
            Assert.AreEqual("preVal1", preVals.PreValuesAsDictionary.Keys.First());
            Assert.AreEqual("preVal2", preVals.PreValuesAsDictionary.Keys.Last());
            Assert.AreEqual("Hello", preVals.PreValuesAsDictionary["preVal1"].Value);
            Assert.AreEqual("World", preVals.PreValuesAsDictionary["preVal2"].Value);
        }

        [Test]
        public void DataTypeService_Can_Persist_Dtd_And_Dictionary_Based_Pre_Values()
        {
            // Arrange
            var dataTypeService = ServiceContext.DataTypeService;
            var textBoxAlias = Constants.PropertyEditors.TextboxAlias;

            // Act
            IDataType dataType = new DataType(-1, textBoxAlias) { Name = "Testing prevals", DatabaseType = DataTypeDatabaseType.Ntext };
            dataTypeService.SaveDataTypeAndPreValues(dataType, new Dictionary<string, PreValue>
                {
                    {"preVal1", new PreValue("Hello")},
                    {"preVal2", new PreValue("World")}
                });
            //re-get
            dataType = dataTypeService.GetDataType(dataType.Id);
            var preVals = dataTypeService.GetPreValuesCollectionByDataTypeId(dataType.Id);

            // Assert
            Assert.That(dataType, Is.Not.Null);
            Assert.That(dataType.HasIdentity, Is.True);
            Assert.AreEqual(true, preVals.IsDictionaryBased);
            Assert.AreEqual(2, preVals.PreValuesAsDictionary.Keys.Count);
            Assert.AreEqual("preVal1", preVals.PreValuesAsDictionary.Keys.First());
            Assert.AreEqual("preVal2", preVals.PreValuesAsDictionary.Keys.Last());
            Assert.AreEqual("Hello", preVals.PreValuesAsDictionary["preVal1"].Value);
            Assert.AreEqual("World", preVals.PreValuesAsDictionary["preVal2"].Value);
        }

        [Test]
        public void DataTypeService_Can_Update_Pre_Values()
        {
            // Arrange
            var dataTypeService = ServiceContext.DataTypeService;
            var textBoxAlias = Constants.PropertyEditors.TextboxAlias;

            // Act
            IDataType dataType = new DataType(-1, textBoxAlias) { Name = "Testing prevals", DatabaseType = DataTypeDatabaseType.Ntext };
            dataTypeService.SaveDataTypeAndPreValues(dataType, new Dictionary<string, PreValue>
                {
                    {"preVal1", new PreValue("Hello")},
                    {"preVal2", new PreValue("World")}
                });
            //re-get
            dataType = dataTypeService.GetDataType(dataType.Id);
            var preVals = dataTypeService.GetPreValuesCollectionByDataTypeId(dataType.Id);

            //update them (ensure Ids are there!)
            var asDictionary = preVals.FormatAsDictionary();
            asDictionary["preVal1"].Value = "Hello2";
            asDictionary["preVal2"].Value = "World2";

            dataTypeService.SavePreValues(dataType, asDictionary);

            var preValsAgain = dataTypeService.GetPreValuesCollectionByDataTypeId(dataType.Id);

            // Assert

            Assert.AreEqual(preVals.PreValuesAsDictionary.Values.First().Id, preValsAgain.PreValuesAsDictionary.Values.First().Id);
            Assert.AreEqual(preVals.PreValuesAsDictionary.Values.Last().Id, preValsAgain.PreValuesAsDictionary.Values.Last().Id);
            Assert.AreEqual("preVal1", preValsAgain.PreValuesAsDictionary.Keys.First());
            Assert.AreEqual("preVal2", preValsAgain.PreValuesAsDictionary.Keys.Last());
            Assert.AreEqual("Hello2", preValsAgain.PreValuesAsDictionary["preVal1"].Value);
            Assert.AreEqual("World2", preValsAgain.PreValuesAsDictionary["preVal2"].Value);
        }

        [Test]
        public void DataTypeService_Can_Remove_Pre_Value()
        {
            // Arrange
            var dataTypeService = ServiceContext.DataTypeService;
            var textBoxAlias = Constants.PropertyEditors.TextboxAlias;

            // Act
            IDataType dataType = new DataType(-1, textBoxAlias) { Name = "Testing prevals", DatabaseType = DataTypeDatabaseType.Ntext };
            dataTypeService.SaveDataTypeAndPreValues(dataType, new Dictionary<string, PreValue>
                {
                    {"preVal1", new PreValue("Hello")},
                    {"preVal2", new PreValue("World")}
                });
            //re-get
            dataType = dataTypeService.GetDataType(dataType.Id);
            var preVals = dataTypeService.GetPreValuesCollectionByDataTypeId(dataType.Id);

            //update them (ensure Ids are there!)
            var asDictionary = preVals.FormatAsDictionary();
            asDictionary.Remove("preVal2");

            dataTypeService.SavePreValues(dataType, asDictionary);

            var preValsAgain = dataTypeService.GetPreValuesCollectionByDataTypeId(dataType.Id);

            // Assert

            Assert.AreEqual(1, preValsAgain.FormatAsDictionary().Count);
            Assert.AreEqual(preVals.PreValuesAsDictionary.Values.First().Id, preValsAgain.PreValuesAsDictionary.Values.First().Id);
            Assert.AreEqual("preVal1", preValsAgain.PreValuesAsDictionary.Keys.First());

        }

        [Test]
        public void DataTypeService_Can_Persist_Array_Based_Pre_Values()
        {
            // Arrange
            var dataTypeService = ServiceContext.DataTypeService;
            var textBoxAlias = Constants.PropertyEditors.TextboxAlias;

            // Act
            IDataType dataType = new DataType(-1, textBoxAlias) { Name = "Testing prevals", DatabaseType = DataTypeDatabaseType.Ntext };
            dataTypeService.Save(dataType);
            dataTypeService.SavePreValues(dataType.Id, new[] {"preVal1", "preVal2"});

            //re-get
            dataType = dataTypeService.GetDataType(dataType.Id);
            var preVals = dataTypeService.GetPreValuesCollectionByDataTypeId(dataType.Id);

            // Assert
            Assert.That(dataType, Is.Not.Null);
            Assert.That(dataType.HasIdentity, Is.True);
            Assert.AreEqual(false, preVals.IsDictionaryBased);
            Assert.AreEqual(2, preVals.PreValuesAsArray.Count());
            Assert.AreEqual("preVal1", preVals.PreValuesAsArray.First().Value);
            Assert.AreEqual("preVal2", preVals.PreValuesAsArray.Last().Value);
        }

        [Test]
        public void Cannot_Save_DataType_With_Empty_Name()
        {
            // Arrange
            var dataTypeService = ServiceContext.DataTypeService;

            // Act
            var dataTypeDefinition = new DataType(-1, "Test.TestEditor") { Name = string.Empty, DatabaseType = DataTypeDatabaseType.Ntext };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => dataTypeService.Save(dataTypeDefinition));
        }
    }
}
