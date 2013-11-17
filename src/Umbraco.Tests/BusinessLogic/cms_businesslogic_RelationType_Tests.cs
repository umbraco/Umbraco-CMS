using System;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Core.Models.Rdbms;
using umbraco.cms.businesslogic.relation;

namespace Umbraco.Tests.BusinessLogic
{
    [TestFixture]
    public class cms_businesslogic_RelationType_Tests : BaseDatabaseFactoryTestWithContext
    {
 
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
            Assert.That(getTestRelationTypeDto(_relationType1.Id), Is.Null);
            Assert.That(getTestRelationTypeDto(_relationType2.Id), Is.Null);
        }

        [Test(Description = "Test 'public RelationType(int id)' and 'private void PopulateFromDTO(RelationTypeDto dto)' methods")]
        public void Test_RelationType_Constructor_and_PopulateFromDto()
        {
            var testRelationType = new RelationType(_relationType1.Id);

            Assert.That(testRelationType.Id, Is.EqualTo(_relationType1.Id));
            Assert.That(testRelationType.Dual, Is.EqualTo(_relationType1.Dual));
            Assert.That(testRelationType.Name, Is.EqualTo(_relationType1.Name));
            Assert.That(testRelationType.Alias, Is.EqualTo(_relationType1.Alias));
        }

        [Test(Description = "Test 'public static RelationType GetById(int id)' method")]
        public void Test_RelationType_GetById()
        {
            var testRelationType = RelationType.GetById(_relationType1.Id);

            Assert.That(testRelationType.Id, Is.EqualTo(_relationType1.Id));
            Assert.That(testRelationType.Dual, Is.EqualTo(_relationType1.Dual));
            Assert.That(testRelationType.Name, Is.EqualTo(_relationType1.Name));
            Assert.That(testRelationType.Alias, Is.EqualTo(_relationType1.Alias));
        }
     
        [Test(Description = "Test 'public static RelationType GetByAlias(string Alias)' method")]
        public void Test_RelationType_GetByAlias()
        {
            var testRelationType = RelationType.GetByAlias(_relationType1.Alias);

            Assert.That(testRelationType.Id, Is.EqualTo(_relationType1.Id));
            Assert.That(testRelationType.Dual, Is.EqualTo(_relationType1.Dual));
            Assert.That(testRelationType.Name, Is.EqualTo(_relationType1.Name));
            Assert.That(testRelationType.Alias, Is.EqualTo(_relationType1.Alias));
        }

        [Test(Description = "Test 'public static IEnumerable<RelationType> GetAll()' method")]
        public void Test_RelationType_GetAll()
        {
            int expectedValue = independentDatabase.Query<RelationTypeDto>(
                      "SELECT id, [dual], name, alias FROM umbracoRelationType").ToArray().Length;  
            var relationTypes = RelationType.GetAll().ToArray<RelationType>();

            Assert.That(relationTypes.Length, Is.EqualTo(expectedValue));  // 1 default + 2 created in this test suite
        }


        [Test(Description = "Test 'public string Name' property ")]
        public void Test_RelationType_SetName()
        {
            var relationType1 = new RelationType(_relationType1.Id);
            Assert.That(relationType1.Name, Is.EqualTo(_relationType1.Name));

            relationType1.Name = "New Name";

            var relationType2 = getTestRelationTypeDto(_relationType1.Id);
            Assert.That(relationType2.Name, Is.EqualTo(relationType1.Name));
            Assert.That(relationType2.Name, Is.EqualTo("New Name"));
        }

        [Test(Description = "Test 'public string Alias' property set")]
        public void Test_RelationType_SetAlias()
        {
            var relationType1 = new RelationType(_relationType1.Id);
            Assert.That(relationType1.Alias, Is.EqualTo(_relationType1.Alias));

            relationType1.Alias = "newAlias";

            var relationType2 = getTestRelationTypeDto(_relationType1.Id);
            Assert.That(relationType2.Alias, Is.EqualTo(relationType1.Alias));
            Assert.That(relationType2.Alias, Is.EqualTo("newAlias"));
        }

        [Test(Description = "Test 'public bool Dual' property set")]
        public void Test_RelationType_SetDual()
        {
            var relationType1 = new RelationType(_relationType1.Id);
            Assert.That(relationType1.Dual, Is.EqualTo(_relationType1.Dual));

            bool dual = !relationType1.Dual;
            relationType1.Dual = dual;

            var relationType2 = getTestRelationTypeDto(_relationType1.Id);
            Assert.That(relationType2.Dual, Is.EqualTo(relationType1.Dual));
            Assert.That(relationType2.Dual, Is.EqualTo(dual));
        }
        #endregion

        #region EnsureData
        private RelationTypeDto _relationType1;
        private RelationTypeDto _relationType2;

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void EnsureData()
        {
            if (!initialized)
            {
                _relationType1 = insertTestRelationType(uniqueRelationName);
                _relationType2 = insertTestRelationType(uniqueRelationName);
            }

            initialized = true;
        }

        protected string uniqueRelationName
        {
            get
            {
                return string.Format("Relation Name {0}", Guid.NewGuid().ToString());
            }
        }

        private RelationTypeDto getTestRelationTypeDto(int relationTypeId)
        {
            return getPersistedTestDto<RelationTypeDto>(relationTypeId);
        }

        private const string TEST_RELATION_TYPE_NAME = "Test Relation Type";
        private const string TEST_RELATION_TYPE_ALIAS = "testRelationTypeAlias";
        private RelationTypeDto insertTestRelationType(string testRelationName)
        {
            independentDatabase.Execute("insert into [umbracoRelationType] ([dual], [parentObjectType], [childObjectType], [name], [alias]) values " +
                            "(@dual, @parentObjectType, @childObjectType, @name, @alias)",
                            new
                            {
                                dual = 1,
                                parentObjectType = Guid.NewGuid(),
                                childObjectType = Guid.NewGuid(),
                                name = testRelationName,
                                alias = testRelationName.Replace(" ", "")
                            });
            int relationTypeId = independentDatabase.ExecuteScalar<int>("select max(id) from [umbracoRelationType]");
            return getTestRelationTypeDto(relationTypeId);
        }

        private void EnsureAllTestRecordsAreDeleted()
        {
            if (_relationType1 != null) independentDatabase.Execute("delete from [umbracoRelationType] where (Id = @0)", _relationType1.Id);
            if (_relationType2 != null) independentDatabase.Execute("delete from [umbracoRelationType] where (Id = @0)", _relationType2.Id);

            initialized = false;
        }

        #endregion

    }
}
