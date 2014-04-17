using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Services
{
    /// <summary>
    /// Tests covering the DataTypeService
    /// </summary>
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
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

            // Act
            var dataTypeDefinition = new DataTypeDefinition(-1, "Test.TestEditor") { Name = "Testing Textfield", DatabaseType = DataTypeDatabaseType.Ntext };
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
            var textfieldId = "Umbraco.Textbox";
            var dataTypeDefinitions = dataTypeService.GetDataTypeDefinitionByPropertyEditorAlias(textfieldId);
            
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

        [Test]
        public void DataTypeService_Can_Persist_Dictionary_Based_Pre_Values()
        {
            // Arrange
            var dataTypeService = ServiceContext.DataTypeService;
            var textfieldId = new Guid("ec15c1e5-9d90-422a-aa52-4f7622c63bea");

            // Act
            IDataTypeDefinition dataTypeDefinition = new DataTypeDefinition(-1, textfieldId) { Name = "Testing prevals", DatabaseType = DataTypeDatabaseType.Ntext };
            dataTypeService.Save(dataTypeDefinition);
            dataTypeService.SavePreValues(dataTypeDefinition, new Dictionary<string, PreValue>
                {
                    {"preVal1", new PreValue("Hello")},
                    {"preVal2", new PreValue("World")}
                });
            //re-get
            dataTypeDefinition = dataTypeService.GetDataTypeDefinitionById(dataTypeDefinition.Id);
            var preVals = dataTypeService.GetPreValuesCollectionByDataTypeId(dataTypeDefinition.Id);

            // Assert
            Assert.That(dataTypeDefinition, Is.Not.Null);
            Assert.That(dataTypeDefinition.HasIdentity, Is.True);
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
            var textfieldId = new Guid("ec15c1e5-9d90-422a-aa52-4f7622c63bea");

            // Act
            IDataTypeDefinition dataTypeDefinition = new DataTypeDefinition(-1, textfieldId) { Name = "Testing prevals", DatabaseType = DataTypeDatabaseType.Ntext };
            dataTypeService.SaveDataTypeAndPreValues(dataTypeDefinition, new Dictionary<string, PreValue>
                {
                    {"preVal1", new PreValue("Hello")},
                    {"preVal2", new PreValue("World")}
                });
            //re-get
            dataTypeDefinition = dataTypeService.GetDataTypeDefinitionById(dataTypeDefinition.Id);
            var preVals = dataTypeService.GetPreValuesCollectionByDataTypeId(dataTypeDefinition.Id);

            // Assert
            Assert.That(dataTypeDefinition, Is.Not.Null);
            Assert.That(dataTypeDefinition.HasIdentity, Is.True);
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
            var textfieldId = new Guid("ec15c1e5-9d90-422a-aa52-4f7622c63bea");

            // Act
            IDataTypeDefinition dataTypeDefinition = new DataTypeDefinition(-1, textfieldId) { Name = "Testing prevals", DatabaseType = DataTypeDatabaseType.Ntext };
            dataTypeService.SaveDataTypeAndPreValues(dataTypeDefinition, new Dictionary<string, PreValue>
                {
                    {"preVal1", new PreValue("Hello")},
                    {"preVal2", new PreValue("World")}
                });
            //re-get
            dataTypeDefinition = dataTypeService.GetDataTypeDefinitionById(dataTypeDefinition.Id);
            var preVals = dataTypeService.GetPreValuesCollectionByDataTypeId(dataTypeDefinition.Id);

            //update them (ensure Ids are there!)
            var asDictionary = preVals.FormatAsDictionary();
            asDictionary["preVal1"].Value = "Hello2";
            asDictionary["preVal2"].Value = "World2";

            dataTypeService.SavePreValues(dataTypeDefinition, asDictionary);

            var preValsAgain = dataTypeService.GetPreValuesCollectionByDataTypeId(dataTypeDefinition.Id);

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
            var textfieldId = new Guid("ec15c1e5-9d90-422a-aa52-4f7622c63bea");

            // Act
            IDataTypeDefinition dataTypeDefinition = new DataTypeDefinition(-1, textfieldId) { Name = "Testing prevals", DatabaseType = DataTypeDatabaseType.Ntext };
            dataTypeService.SaveDataTypeAndPreValues(dataTypeDefinition, new Dictionary<string, PreValue>
                {
                    {"preVal1", new PreValue("Hello")},
                    {"preVal2", new PreValue("World")}
                });
            //re-get
            dataTypeDefinition = dataTypeService.GetDataTypeDefinitionById(dataTypeDefinition.Id);
            var preVals = dataTypeService.GetPreValuesCollectionByDataTypeId(dataTypeDefinition.Id);

            //update them (ensure Ids are there!)
            var asDictionary = preVals.FormatAsDictionary();
            asDictionary.Remove("preVal2");

            dataTypeService.SavePreValues(dataTypeDefinition, asDictionary);

            var preValsAgain = dataTypeService.GetPreValuesCollectionByDataTypeId(dataTypeDefinition.Id);

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
            var textfieldId = new Guid("ec15c1e5-9d90-422a-aa52-4f7622c63bea");

            // Act
            IDataTypeDefinition dataTypeDefinition = new DataTypeDefinition(-1, textfieldId) { Name = "Testing prevals", DatabaseType = DataTypeDatabaseType.Ntext };
            dataTypeService.Save(dataTypeDefinition);
            dataTypeService.SavePreValues(dataTypeDefinition.Id, new[] {"preVal1", "preVal2"});
            
            //re-get            
            var preVals = dataTypeService.GetPreValuesCollectionByDataTypeId(dataTypeDefinition.Id);

            // Assert
            Assert.That(dataTypeDefinition, Is.Not.Null);
            Assert.That(dataTypeDefinition.HasIdentity, Is.True);
            Assert.AreEqual(false, preVals.IsDictionaryBased);
            Assert.AreEqual(2, preVals.PreValuesAsArray.Count());
            Assert.AreEqual("preVal1", preVals.PreValuesAsArray.First().Value);
            Assert.AreEqual("preVal2", preVals.PreValuesAsArray.Last().Value);            
        }
    }
}