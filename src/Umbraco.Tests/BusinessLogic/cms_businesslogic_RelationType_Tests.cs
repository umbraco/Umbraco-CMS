using System.Collections.Generic;
using NUnit.Framework;
using umbraco.BusinessLogic;
using Umbraco.Tests.TestHelpers;
using umbraco.cms.businesslogic.datatype;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Reflection;
using umbraco.cms.businesslogic;
using System;
using System.Xml;
using System.Linq;
using Umbraco.Web;
using Umbraco.Core.Persistence;
using Umbraco.Core.Models.Rdbms;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.relation;

namespace Umbraco.Tests.BusinessLogic
{
    [TestFixture]
    public class cms_businesslogic_RelationType_Tests : BaseDatabaseFactoryTest
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
        private UmbracoContext context;
        private UmbracoDatabase database;

        private RelationType _relationType1;
        private RelationType _relationType2;

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void EnsureData()
        {
            if (!initialized)
            {
                CreateContext();

                _relationType1 = insertTestRelationType(1);                
                _relationType2 = insertTestRelationType(2); 
            }

            initialized = true;
        }

        private const string TEST_RELATION_TYPE_NAME = "Test Relation Type";
        private const string TEST_RELATION_TYPE_ALIAS = "testRelationTypeAlias";
        private RelationType insertTestRelationType(int testRelationTypeNumber)
        {
            database.Execute("insert into [umbracoRelationType] ([dual], [parentObjectType], [childObjectType], [name], [alias]) values " +
                            "(@dual, @parentObjectType, @childObjectType, @name, @alias)",
                            new { dual = 1, parentObjectType = Guid.NewGuid(), childObjectType = Guid.NewGuid(),
                                  name = string.Format("{0}_{1}", TEST_RELATION_TYPE_NAME, testRelationTypeNumber),
                                  alias = string.Format("{0}_{1}", TEST_RELATION_TYPE_ALIAS, testRelationTypeNumber),
                            });
            int relationTypeId = database.ExecuteScalar<int>("select max(id) from [umbracoRelationType]");
            return new RelationType(relationTypeId);
        }

        private void EnsureAllTestRecordsAreDeleted()
        {
           if (_relationType1 != null) database.Execute("delete from [umbracoRelationType] where (Id = @0)", _relationType1.Id);
           if (_relationType2 != null) database.Execute("delete from [umbracoRelationType] where (Id = @0)", _relationType2.Id);

           initialized = false; 
        }

        private void CreateContext()
        {
            context = GetUmbracoContext("http://localhost", 0);
            database = context.Application.DatabaseContext.Database;
        }
 
        #endregion

        #region Tests
        [Test(Description = "Verify if EnsureData() and related helper test methods execute well")]
        public void Test_EnsureData()
        {
            Assert.IsTrue(initialized);

            //int count2 = database.ExecuteScalar<int>("select count(*) from [umbracoRelationType]");
            //l("Count = {0}", count2); // = 1 + 2

            Assert.That(_relationType1, !Is.Null);
            Assert.That(_relationType2, !Is.Null);

            EnsureAllTestRecordsAreDeleted();

            //int count21 = database.ExecuteScalar<int>("select count(*) from [umbracoRelationType]");
            //l("Count = {0}", count21); // = 1 + 0

            // see the next test code line: Assert.Throws(typeof(ArgumentException), delegate { RelationType.GetById(_relationType.Id); });
            Assert.That(RelationType.GetById(_relationType1.Id), Is.Null);
            Assert.That(RelationType.GetById(_relationType2.Id), Is.Null);

            traceCompletion();
        }

        [Test(Description = "Test 'public RelationType(int id)' and 'private void PopulateFromDTO(RelationTypeDto dto)' methods")]
        public void Test_RelationType_Constructor_and_PopulateFromDto()
        {
            var testRelationType = new RelationType(_relationType1.Id);

            Assert.That(testRelationType.Id, Is.EqualTo(_relationType1.Id));
            Assert.That(testRelationType.Dual, Is.EqualTo(_relationType1.Dual));
            Assert.That(testRelationType.Name, Is.EqualTo(_relationType1.Name));
            Assert.That(testRelationType.Alias, Is.EqualTo(_relationType1.Alias));    

            traceCompletion();
        }

        [Test(Description = "Test 'public static RelationType GetById(int id)' method")]
        public void Test_RelationType_GetById()
        {
            var testRelationType = RelationType.GetById(_relationType1.Id);

            Assert.That(testRelationType.Id, Is.EqualTo(_relationType1.Id));
            Assert.That(testRelationType.Dual, Is.EqualTo(_relationType1.Dual));
            Assert.That(testRelationType.Name, Is.EqualTo(_relationType1.Name));
            Assert.That(testRelationType.Alias, Is.EqualTo(_relationType1.Alias));

            traceCompletion();
        }
     
        [Test(Description = "Test 'public static RelationType GetByAlias(string Alias)' method")]
        public void Test_RelationType_GetByAlias()
        {
            var testRelationType = RelationType.GetByAlias(_relationType1.Alias);

            Assert.That(testRelationType.Id, Is.EqualTo(_relationType1.Id));
            Assert.That(testRelationType.Dual, Is.EqualTo(_relationType1.Dual));
            Assert.That(testRelationType.Name, Is.EqualTo(_relationType1.Name));
            Assert.That(testRelationType.Alias, Is.EqualTo(_relationType1.Alias));

            traceCompletion();
        }

        [Test(Description = "Test 'public static IEnumerable<RelationType> GetAll()' method")]
        public void Test_RelationType_GetAll()
        {
            var relationTypes = RelationType.GetAll().ToArray<RelationType>();

            Assert.That(relationTypes.Length, Is.EqualTo(3));  // 1 default + 2 created in this test suite

            traceCompletion();
        }


        [Test(Description = "Test 'public string Name' property set")]
        public void Test_RelationType_SetName()
        {
            string oldName = _relationType1.Name;

            try
            {
                var relationType1 = new RelationType(_relationType1.Id);
                Assert.That(relationType1.Name, Is.EqualTo(_relationType1.Name));

                relationType1.Name = "New Name";

                var relationType2 = new RelationType(_relationType1.Id);
                Assert.That(relationType2.Name, Is.EqualTo(relationType1.Name));
            }
            finally
            {
                _relationType1.Name = oldName; 
            }

            traceCompletion();
        }

        [Test(Description = "Test 'public string Alias' property set")]
        public void Test_RelationType_SetAlias()
        {
            string oldAlias = _relationType1.Alias;

            try
            {
                var relationType1 = new RelationType(_relationType1.Id);
                Assert.That(relationType1.Alias, Is.EqualTo(_relationType1.Alias));

                relationType1.Alias = "newAlias";

                var relationType2 = new RelationType(_relationType1.Id);
                Assert.That(relationType2.Alias, Is.EqualTo(relationType1.Alias));
            }
            finally
            {
                _relationType1.Alias = oldAlias;
            }

            traceCompletion();
        }

        [Test(Description = "Test 'public bool Dual' property set")]
        public void Test_RelationType_SetDual()
        {
            var oldDual = _relationType1.Dual;

            try
            {
                var relationType1 = new RelationType(_relationType1.Id);
                Assert.That(relationType1.Dual, Is.EqualTo(_relationType1.Dual));

                relationType1.Dual = !relationType1.Dual;

                var relationType2 = new RelationType(_relationType1.Id);
                Assert.That(relationType2.Dual, Is.EqualTo(relationType1.Dual));
            }
            finally
            {
                _relationType1.Dual = oldDual;
            }

            traceCompletion();
        }

        #endregion

        #region Private Helper classes and methods
        // NONE
        #endregion
    }
}
