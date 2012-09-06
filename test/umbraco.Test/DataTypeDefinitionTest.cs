using umbraco.cms.businesslogic.datatype;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using umbraco.BusinessLogic;
using umbraco.interfaces;
using umbraco.editorControls.textfield;
using umbraco.cms.businesslogic.web;
using System.Linq;
using umbraco.cms.businesslogic.propertytype;
using umbraco.editorControls;

namespace Umbraco.LegacyTests
{
    
    
    /// <summary>
    ///This is a test class for DataTypeDefinitionTest and is intended
    ///to contain all DataTypeDefinitionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DataTypeDefinitionTest
    {

        /// <summary>
        ///A test for MakeNew
        ///</summary>
        [TestMethod()]
        public void DataTypeDefinition_Make_New()
        {
            //create data tyep definition
            var dtd = DataTypeDefinition.MakeNew(m_User, "TEST" + Guid.NewGuid().ToString("N"));            
            Assert.IsTrue(dtd.Id > 0);
            Assert.IsInstanceOfType(dtd, typeof(DataTypeDefinition));

            //now delete it
            dtd.delete();
            Assert.IsFalse(DataTypeDefinition.IsNode(dtd.Id));
        }

        /// <summary>
        /// Create a data type definition, add some prevalues to it then delete it
        ///</summary>
        [TestMethod()]
        public void DataTypeDefinition_Assign_Data_Type_With_PreValues()
        {
            //System.Diagnostics.Debugger.Launch();

            //create datatype definition, assign data type
            var dtd = DataTypeDefinition.MakeNew(m_User, "TEST" + Guid.NewGuid().ToString("N"));
            Assert.IsTrue(dtd.Id > 0);
            Assert.IsInstanceOfType(dtd, typeof(DataTypeDefinition));
            IDataType dt = new TextFieldDataType();
            dt.DataTypeDefinitionId = dtd.Id; //need to set the data types data type definition id
            dtd.DataType = dt; //set our data type definition's data type to the text field... i know this is all too weird.
            
            Assert.AreEqual(dt.Id, dtd.DataType.Id);
            Assert.IsInstanceOfType(dtd.DataType, dt.GetType());
            Assert.AreEqual(dtd.Id, dt.DataTypeDefinitionId);

            //create the prevalues
            ((DefaultPrevalueEditor)dt.PrevalueEditor).Prevalue = "TEST" + Guid.NewGuid().ToString("N");
            dt.PrevalueEditor.Save();

            //verify that the prevalue is there
            Assert.AreEqual<int>(1, PreValues.GetPreValues(dtd.Id).Count);

            //now remove it
            dtd.delete();
            Assert.IsFalse(DataTypeDefinition.IsNode(dtd.Id));
        }

        /// <summary>
        /// Create a new definition, assign a data type to it,
        /// create a doc type and assign this new data type to it,
        /// then create a document from the doc type and set the value for the property.
        /// 
        /// Once the data is all setup, we'll delete the data type definition which should:
        /// 1. Remove all property type values associated with the property type of the dtd
        /// 2. Remove all property types from document types associated with the dtd
        /// 3. Remove the dtd
        /// 
        /// then we'll clean up the rest of the data.
        /// </summary>
        [TestMethod()]
        public void DataTypeDefinition_Assign_Data_Type_To_Doc_Type_Then_Create_Doc_And_Set_Value()
        {
            //create datatype definition, assign data type
            var dtd = DataTypeDefinition.MakeNew(m_User, "TEST" + Guid.NewGuid().ToString("N"));
            Assert.IsTrue(dtd.Id > 0);
            Assert.IsInstanceOfType(dtd, typeof(DataTypeDefinition));
            IDataType dt = new TextFieldDataType();
            dtd.DataType = dt;
            Assert.AreEqual(dt.Id, dtd.DataType.Id);
            Assert.IsInstanceOfType(dtd.DataType, dt.GetType());

            //create new doc type
            var docType = DocumentType.MakeNew(m_User, "TEST" + Guid.NewGuid().ToString("N"));
            
            //create the property with this new data type definition
            var alias = "TEST" + Guid.NewGuid().ToString("N");
            docType.AddPropertyType(dtd, alias, alias);
            Assert.AreEqual<int>(1, docType.PropertyTypes.Count());

            //create a new doc with the new doc type
            var doc = Document.MakeNew("TEST" + Guid.NewGuid().ToString("N"), docType, m_User, -1);

            //set the value of the property
            var prop = doc.getProperty(alias);
            var propType = prop.PropertyType;
            Assert.IsNotNull(prop);
            var val = "TEST" + Guid.NewGuid().ToString("N");
            prop.Value = val;
            Assert.AreEqual(val, prop.Value);
            
            //ok, now that all of the data is setup, we'll just delete the data type definition.
            dtd.delete();

            //make sure the property value is gone, check with sql
            Assert.AreEqual<int>(0, Application.SqlHelper.ExecuteScalar<int>(
                "SELECT COUNT(id) FROM cmsPropertyData WHERE propertytypeid=@propTypeId",
                Application.SqlHelper.CreateParameter("@propTypeId", propType.Id)));

            //make sure the property type is gone
            var hasError = false;
            try
            {
                var confirmPropType = new PropertyType(propType.Id);
            }
            catch (ArgumentException)
            {
                hasError = true;
            }
            Assert.IsTrue(hasError);

            //make sure the dtd is gone
            Assert.IsFalse(DataTypeDefinition.IsNode(dtd.Id));

            //now cleanup the rest
            doc.delete(true);
            Assert.IsFalse(Document.IsNode(doc.Id));
            docType.delete();
            Assert.IsFalse(DocumentType.IsNode(docType.Id));
        }

        #region Private methods/members
        private User m_User = new User(0); 
        #endregion

        #region Tests to write
        ///// <summary>
        /////A test for DataTypeDefinition Constructor
        /////</summary>
        //[TestMethod()]
        //public void DataTypeDefinitionConstructorTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    DataTypeDefinition target = new DataTypeDefinition(id);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for DataTypeDefinition Constructor
        /////</summary>
        //[TestMethod()]
        //public void DataTypeDefinitionConstructorTest1()
        //{
        //    int id = 0; // TODO: Initialize to an appropriate value
        //    DataTypeDefinition target = new DataTypeDefinition(id);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for Delete
        /////</summary>
        //[TestMethod()]
        //public void DeleteTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    DataTypeDefinition target = new DataTypeDefinition(id); // TODO: Initialize to an appropriate value
        //    target.Delete();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for GetAll
        /////</summary>
        //[TestMethod()]
        //public void GetAllTest()
        //{
        //    DataTypeDefinition[] expected = null; // TODO: Initialize to an appropriate value
        //    DataTypeDefinition[] actual;
        //    actual = DataTypeDefinition.GetAll();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetByDataTypeId
        /////</summary>
        //[TestMethod()]
        //public void GetByDataTypeIdTest()
        //{
        //    Guid DataTypeId = new Guid(); // TODO: Initialize to an appropriate value
        //    DataTypeDefinition expected = null; // TODO: Initialize to an appropriate value
        //    DataTypeDefinition actual;
        //    actual = DataTypeDefinition.GetByDataTypeId(DataTypeId);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetDataTypeDefinition
        /////</summary>
        //[TestMethod()]
        //public void GetDataTypeDefinitionTest()
        //{
        //    int id = 0; // TODO: Initialize to an appropriate value
        //    DataTypeDefinition expected = null; // TODO: Initialize to an appropriate value
        //    DataTypeDefinition actual;
        //    actual = DataTypeDefinition.GetDataTypeDefinition(id);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetDataTypeDefinition
        /////</summary>
        //[TestMethod()]
        //public void GetDataTypeDefinitionTest1()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    DataTypeDefinition expected = null; // TODO: Initialize to an appropriate value
        //    DataTypeDefinition actual;
        //    actual = DataTypeDefinition.GetDataTypeDefinition(id);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Import
        /////</summary>
        //[TestMethod()]
        //public void ImportTest()
        //{
        //    XmlNode xmlData = null; // TODO: Initialize to an appropriate value
        //    DataTypeDefinition expected = null; // TODO: Initialize to an appropriate value
        //    DataTypeDefinition actual;
        //    actual = DataTypeDefinition.Import(xmlData);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for IsDefaultData
        /////</summary>
        //[TestMethod()]
        //public void IsDefaultDataTest()
        //{
        //    object Data = null; // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = DataTypeDefinition.IsDefaultData(Data);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        

        ///// <summary>
        /////A test for MakeNew
        /////</summary>
        //[TestMethod()]
        //public void MakeNewTest1()
        //{
        //    User u = null; // TODO: Initialize to an appropriate value
        //    string Text = string.Empty; // TODO: Initialize to an appropriate value
        //    Guid UniqueId = new Guid(); // TODO: Initialize to an appropriate value
        //    DataTypeDefinition expected = null; // TODO: Initialize to an appropriate value
        //    DataTypeDefinition actual;
        //    actual = DataTypeDefinition.MakeNew(u, Text, UniqueId);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Save
        /////</summary>
        //[TestMethod()]
        //public void SaveTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    DataTypeDefinition target = new DataTypeDefinition(id); // TODO: Initialize to an appropriate value
        //    target.Save();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for ToXml
        /////</summary>
        //[TestMethod()]
        //public void ToXmlTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    DataTypeDefinition target = new DataTypeDefinition(id); // TODO: Initialize to an appropriate value
        //    XmlDocument xd = null; // TODO: Initialize to an appropriate value
        //    XmlElement expected = null; // TODO: Initialize to an appropriate value
        //    XmlElement actual;
        //    actual = target.ToXml(xd);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for delete
        /////</summary>
        //[TestMethod()]
        //public void deleteTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    DataTypeDefinition target = new DataTypeDefinition(id); // TODO: Initialize to an appropriate value
        //    target.delete();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        
        #endregion


        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion
    }
}
