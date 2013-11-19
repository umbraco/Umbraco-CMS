using System;
using NUnit.Framework;
using umbraco.BusinessLogic;
using Umbraco.Tests.TestHelpers;
using umbraco.cms.businesslogic.datatype;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Reflection;
using Umbraco.Tests.BusinessLogic;

namespace Umbraco.Tests.ORM
{
    [TestFixture]
    public class cms_businesslogic_PreValue_Tests : BaseORMTest
    {
        protected override void EnsureData() { Ensure_PreValue_TestData(); }

        [Test(Description = "Verify if EnsureData() executes well")]
        public void _1st_Test_PreValue_EnsureData()
        {
            Assert.IsTrue(initialized);
            Assert.That(_dataTypeDefinition1, !Is.Null);
            Assert.That(_preValue, !Is.Null);
            
            Assert.That(TRAL.PreValue.CountDataTypeOfId(_dataTypeDefinition1.Id), Is.EqualTo(1));
            Assert.That(TRAL.PreValue.CountDataTypeNodes(_dataTypeDefinition1.Id), Is.EqualTo(1));
            Assert.That(TRAL.PreValue.CountDataTypeOfId(_dataTypeDefinition2.Id), Is.EqualTo(1));
            Assert.That(TRAL.PreValue.CountDataTypeNodes(_dataTypeDefinition2.Id), Is.EqualTo(1));

            EnsureAll_PreValue_TestRecordsAreDeleted();

            Assert.That(TRAL.PreValue.CountDataTypeOfId(_dataTypeDefinition1.Id), Is.EqualTo(0));
            Assert.That(TRAL.PreValue.CountDataTypeNodes(_dataTypeDefinition1.Id), Is.EqualTo(0));
            Assert.That(TRAL.PreValue.CountDataTypeOfId(_dataTypeDefinition2.Id), Is.EqualTo(0));
            Assert.That(TRAL.PreValue.CountDataTypeNodes(_dataTypeDefinition2.Id), Is.EqualTo(0));
        }

        [Test(Description = "Test 'public PreValue(int id)' constructor")]
        public void _2nd_Test_PreValue_Constructor_I()
        {
            var testPrevalue = new PreValue(_preValue.Id);
            assertPreValueSetup(testPrevalue, _preValue);
        }

        [Test(Description = "Test 'public PreValue(int DataTypeId, string Value)' constructor")]
        public void _3rd_Test_PreValue_Constructor_II()
        {
            var testPrevalue = new PreValue(_dataTypeDefinition1.Id, _preValue.Value);
            assertPreValueSetup(testPrevalue, _preValue);
        }

        private void assertPreValueSetup(PreValue testPreValue, umbraco.cms.businesslogic.datatype.PreValue.PreValueDto savedPreValue)
        {
            Assert.That(testPreValue.Id, Is.EqualTo(savedPreValue.Id), "Id test failed");
            Assert.That(testPreValue.SortOrder, Is.EqualTo(savedPreValue.SortOrder), "SortOrder test failed");
            Assert.That(testPreValue.Value, Is.EqualTo(savedPreValue.Value), "Value test failed");
            Assert.That(testPreValue.DataTypeId, Is.EqualTo(savedPreValue.DataTypeId), "DataTypeId Test failed");
        }

        [Test(Description = "Test constructors and initialize() method for the new not-persisted PreValue object instances")]
        [TestCase(3, Description = "test public PreValue(int Id) constructor - trying to fetch not existing instance by Id")]
        [TestCase(2, Description = "Test accesing Id and other properties of a PreValue instance created by default constructor")]
        [TestCase(1, Description = "Test 'public PreValue(int Id, int SortOrder, string Value) ' constructor")]
        public void _4th_Test_PreValue_Constructors_Various_Calls(int testCase)
        {
            PreValue newPreValue = null;

            switch (testCase)
            {
                case 1:
                    // test public PreValue(int Id, int SortOrder, string Value) constructor - create new object instance to be used for .Save
                    string value = "value " + uniqueValue;
                    newPreValue = new PreValue(NON_EXISTENT_TEST_ID_VALUE, 1, value);
                    Assert.That(newPreValue.Id, Is.EqualTo(NON_EXISTENT_TEST_ID_VALUE));
                    Assert.That(newPreValue.SortOrder, Is.EqualTo(1));
                    Assert.That(newPreValue.Value, Is.EqualTo(value));
                    break;
                case 2:
                    // test parametersless constructor public PreValue()
                    newPreValue = new PreValue();
                    Assert.Throws(typeof(InvalidOperationException), delegate { int test = newPreValue.Id; });
                    Assert.That(newPreValue.SortOrder, Is.EqualTo(0));
                    Assert.That(newPreValue.Value, Is.Null);
                    break;

                case 3:

                    // test public PreValue(int Id) constructor - trying to fetch not existing instance by Id
                    Assert.Throws(typeof(ArgumentException), delegate { newPreValue = new PreValue(NON_EXISTENT_TEST_ID_VALUE); });

                    // test public PreValue(int DataTypeId, string Value) constructor - trying to fetch not existing instance by Data Type Definition Id and Value
                    newPreValue = null;
                    Assert.Throws(typeof(ArgumentException), delegate { newPreValue = new PreValue(_dataTypeDefinition1.Id, "value " + uniqueValue); });
                    break;
            }
        }

        [Test(Description = "Test 'static PreValue MakeNew(int dataTypeDefId, string value)' method")]
        public void Test_PreValue_MakeNew()
        {
            var newPreValue = PreValue.MakeNew(_dataTypeDefinition1.Id, "value " + uniqueValue);
            Assert.That(newPreValue, !Is.Null);
 
            var savedPrevalue = TRAL.GetDto<umbraco.cms.businesslogic.datatype.PreValue.PreValueDto>(newPreValue.Id);
            assertPreValueSetup(newPreValue, savedPrevalue);
        }

        [Test(Description = "Test 'public void Delete()' method")]
        [TestCase(2, Description = "")]
        [TestCase(1, Description = "Test that new non-saved Prevalue instance can't be deleted")]
        public void Test_PreValue_Delete(int testCase)
        {
            switch (testCase)
            {
                case 1:
                    {
                        var newPreValue = new PreValue();
                        // new non-saved Prevalue instance can't be deleted
                        Assert.Throws(typeof(ArgumentNullException), delegate { newPreValue.Delete(); });
                    }
                    break;
                case 2:
                    {
                        var savedPrevalue1 = TRAL.GetDto<umbraco.cms.businesslogic.datatype.PreValue.PreValueDto>(_preValue.Id);
                        Assert.That(savedPrevalue1, !Is.Null);

                        var preValue = new PreValue(_preValue.Id);
                        Assert.That(preValue, !Is.Null);

                        preValue.Delete();

                        var savedPrevalue2 = TRAL.GetDto<umbraco.cms.businesslogic.datatype.PreValue.PreValueDto>(_preValue.Id);
                        Assert.That(savedPrevalue2, Is.Null); 
                    }
                    break;
            }

            initialized = false;
        }

        [Test(Description = "Test 'public void Save(' method")]
        public void Test_PreValue_Save()
        {
            const int ZERO_ID = 0; // for the .Save(...) method to create a new PreValue record the Id value should be equal to Zero

            var newPreValue = new PreValue(ZERO_ID, 1, "value " + uniqueValue);
            newPreValue.DataTypeId = _dataTypeDefinition1.Id; 
            newPreValue.Save();

            // saved PreValue object instance gets not null ID
            Assert.That(newPreValue.Id, !Is.Null);    

            //  fetched by Id Prevalue object instance has the same properties as an in-memory just saved PreValue object instance
            var savedNewPreValue = TRAL.GetDto<umbraco.cms.businesslogic.datatype.PreValue.PreValueDto>(newPreValue.Id);
            assertPreValueSetup(newPreValue, savedNewPreValue);


        }

    }
}
