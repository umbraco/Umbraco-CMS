using System;
using NUnit.Framework;
using umbraco.BusinessLogic;
using Umbraco.Tests.TestHelpers;
using umbraco.cms.businesslogic.datatype;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Reflection;

namespace Umbraco.Tests.BusinessLogic
{
    [TestFixture]
    public class cms_cms_businesslogic_PreValue_Tests : BaseDatabaseFactoryTest
    {

        #region Helper methods
        protected override DatabaseBehavior DatabaseTestBehavior
        {
            get { return DatabaseBehavior.NewSchemaPerFixture; }
        }

        private void l(string format, params object[] args)
        {
            System.Console.WriteLine(format, args);
        }

        private bool _traceTestCompletion = false;
        private int _testNumber;
        private void traceCompletion(string finished = "Finished")
        {
            if (!_traceTestCompletion) return;
            StackTrace stackTrace = new StackTrace();
            MethodBase methodBase = stackTrace.GetFrame(1).GetMethod();
            string message = string.Format("***** {0:000}. {1} - {2} *****\n", ++_testNumber, methodBase.Name, finished);
            System.Console.Write(message);
        }
        #endregion

        #region EnsureData()
        public override void Initialize()
        {
            base.Initialize();
            EnsureData();
        }
        
        private bool initialized;

        private UserType _userType;
        private User _user;
        private DataTypeDefinition _dataTypeDefinition;
        private PreValue _preValue;
        
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void EnsureData()
        {
            if (!initialized)
            {
                _userType = UserType.MakeNew("Tester", "CADMOSKTPIURZ:5F", "Test User");
                _user = User.MakeNew("TEST", "TEST", "abcdefg012345", _userType);
                _dataTypeDefinition = DataTypeDefinition.MakeNew(_user, "Nvarchar");
            }

            if ((int)PreValues.Database.ExecuteScalar<int>("select count(*) from cmsDataTypePreValues where datatypenodeid = @0", _dataTypeDefinition.Id) == 0)
            {
                initialized = false;

                string value = ",code,undo,redo,cut,copy,mcepasteword,stylepicker,bold,italic,bullist,numlist,outdent,indent,mcelink,unlink,mceinsertanchor,mceimage,umbracomacro,mceinserttable,umbracoembed,mcecharmap,|1|1,2,3,|0|500,400|1049,|true|";

                PreValue.Database.Execute(
                    "insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,'')",
                    new { dtdefid = _dataTypeDefinition.Id, value = value });
                var id = PreValue.Database.ExecuteScalar<int>("SELECT MAX(id) FROM cmsDataTypePreValues");

                _preValue = new PreValue();
                _preValue.Id = id;
                _preValue.DataTypeId = _dataTypeDefinition.Id;
                _preValue.Value = value;
            }

            initialized = true;
        }
        #endregion

        #region Tests
        const int TEST_ID_VALUE = 12345;
        const string TEST_VALUE_VALUE = "test value";

        [Test(Description = "Verify if EnsureData() executes well")]
        public void Test_EnsureData()
        {
            Assert.IsTrue(initialized);
            Assert.That(_userType, !Is.Null);
            Assert.That(_dataTypeDefinition, !Is.Null);
            Assert.That(_preValue, !Is.Null);
            Assert.That(_dataTypeDefinition.Id, !Is.EqualTo(0));
            Assert.That(_preValue.Id, !Is.EqualTo(0));
            traceCompletion();
        }

        [Test(Description = "Fetch PreValue object instance created by Database.Execute(...) in EnsureData() method")]
        public void Test_Get()
        {
            // PreValue class doesn't have any explicit GetById(...) methods - a PreValue object instance is created from a saved in the database record
            // by using a set of constructors, with the most simple one being PreValue(int id). 
            // This PreValue(int id) constructor's functionality is tested here.
            
            // Assert.That fetched from a database record by Id PreValue object instance has the same properties as an in-memory test PreValue object instance
            var savedPrevalue = new PreValue(_preValue.Id);
            Assert.That(_preValue.Id, Is.EqualTo(savedPrevalue.Id));
            Assert.That(_preValue.SortOrder, Is.EqualTo(savedPrevalue.SortOrder));
            Assert.That(_preValue.Value, Is.EqualTo(savedPrevalue.Value));
            Assert.That(_preValue.DataTypeId, Is.EqualTo(savedPrevalue.DataTypeId));
            traceCompletion();
        }

        [Test(Description = "Test constructors and initialize() method for the new non-database PreValue object instances")]
        public void Test_initialize()
        {
            Assert.IsNotNull(_preValue);

            // test parametersless constructor public PreValue()
            var newPreValue1 = new PreValue();

            Assert.Throws(typeof(InvalidOperationException), delegate { l("ID = {0}", newPreValue1.Id); });
            Assert.That(newPreValue1.SortOrder, Is.EqualTo(0));
            Assert.That(newPreValue1.Value, Is.Null);

            // test public PreValue(int Id) constructor - trying to fetch not existing instance by Id
            PreValue newPreValue2 = null;
            Assert.Throws(typeof(ArgumentException), delegate { newPreValue2 = new PreValue(TEST_ID_VALUE); });

            // test public PreValue(int DataTypeId, string Value) constructor - trying to fetch not existing instance by Data Type Definition Id and Value
            PreValue newPreValue3 = null;
            Assert.Throws(typeof(ArgumentException), delegate { newPreValue3 = new PreValue(_dataTypeDefinition.Id, TEST_VALUE_VALUE); });

            // test public PreValue(int Id, int SortOrder, string Value) constructor - create new object instance to be used for .Save
            var newPreValue4 = new PreValue(TEST_ID_VALUE, 1, TEST_VALUE_VALUE);
            Assert.That(newPreValue4.Id, Is.EqualTo(TEST_ID_VALUE));
            Assert.That(newPreValue4.SortOrder, Is.EqualTo(1));
            Assert.That(newPreValue4.Value, Is.EqualTo(TEST_VALUE_VALUE));

            traceCompletion();
        }

        [Test(Description = "Test PreValue(int Id) and PreValue(int DataTypeId, string Value) constructors and initialize() method for saved in the database PreValue object instances")]
        public void Test_get_Values()
        {
            Assert.IsNotNull(_preValue);
           
            // test public PreValue(int Id) constructor - fetch existing instance by Id
            var newPreValue1 = new PreValue(_preValue.Id);
            Assert.That(newPreValue1.Id, Is.EqualTo(_preValue.Id));
            Assert.That(newPreValue1.SortOrder, Is.EqualTo(_preValue.SortOrder));
            Assert.That(newPreValue1.Value, Is.EqualTo(_preValue.Value) );

            // test public PreValue(int DataTypeId, string Value) constructor - fetch existing instance by Data Type Definition Id and Value
            var newPreValue2 = new PreValue(_dataTypeDefinition.Id, _preValue.Value);
            Assert.That(newPreValue2.Id, Is.EqualTo(_preValue.Id));
            Assert.That(newPreValue2.SortOrder, Is.EqualTo(_preValue.SortOrder));
            Assert.That(newPreValue2.Value, Is.EqualTo(_preValue.Value));

            traceCompletion();
        }

        [Test(Description="Test MakeNew(...) static method" )]
        public void Test_MakeNew()
        {
            var newPreValue = PreValue.MakeNew(_dataTypeDefinition.Id, TEST_VALUE_VALUE);
            var savedPrevalue = new PreValue(_dataTypeDefinition.Id, TEST_VALUE_VALUE);

            Assert.That(newPreValue.Id, Is.EqualTo(savedPrevalue.Id));
            Assert.That(newPreValue.SortOrder, Is.EqualTo(savedPrevalue.SortOrder));
            Assert.That(newPreValue.Value, Is.EqualTo(savedPrevalue.Value));
            
            traceCompletion();
        }

        [Test(Description = "Test Delete() method")]
        public void Test_Delete()
        {
            var newPreValue1 = new PreValue();

            // new non-saved Prevalue instance can't be deleted
            Assert.Throws(typeof(ArgumentNullException), delegate { newPreValue1.Delete(); });

            var newPreValue = PreValue.MakeNew(_dataTypeDefinition.Id, TEST_VALUE_VALUE);
            int id = newPreValue.Id; 
            newPreValue.Delete();

            // PreValue deleted - it can't be now fetched PreValue(int id) constructor
            PreValue savedPrevalue = null;
            Assert.Throws(typeof(ArgumentException), delegate { savedPrevalue = new PreValue(id); });

            traceCompletion();
        }

        [Test(Description = "Test Save() method")]
        public void Test_Save()
        {
            const int ZERO_ID = 0; // for the .Save(...) method to create a new PreValue record the Id value should be equal to Zero

            var newPreValue = new PreValue(ZERO_ID, 1, TEST_VALUE_VALUE);
            newPreValue.DataTypeId = _dataTypeDefinition.Id; 
            newPreValue.Save();

            // saved PreValue object instance gets not null ID
            Assert.That(newPreValue.Id, !Is.Null);    

            //  fetched by Id Prevalue object instance has the same properties as an in-memory just saved PreValue object instance
            var savedPrevalue = new PreValue(newPreValue.Id);
            Assert.That(newPreValue.Id, Is.EqualTo(savedPrevalue.Id));
            Assert.That(newPreValue.SortOrder, Is.EqualTo(savedPrevalue.SortOrder));
            Assert.That(newPreValue.Value, Is.EqualTo(savedPrevalue.Value));
            Assert.That(newPreValue.DataTypeId, Is.EqualTo(savedPrevalue.DataTypeId));

            traceCompletion();
        }
        #endregion

    }
}
