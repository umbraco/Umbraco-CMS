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
    public class cms_businesslogic_RelationType_Tests : BaseORMTest
    {
        protected override void EnsureData() { Ensure_RelationType_TestData(); }


        [Test(Description = "Verify if EnsureData() and related helper test methods execute well")]
        public void _1st_Test_RelationType_EnsureData()
        {
            Assert.IsTrue(initialized);

            Assert.That(_relationType1, !Is.Null);
            Assert.That(_relationType2, !Is.Null);

            EnsureAll_RelationType_TestRecordsAreDeleted();

            // see the next test code line: Assert.Throws(typeof(ArgumentException), delegate { RelationType.GetById(_relationType.Id); });
            Assert.That(TRAL.GetDto<RelationTypeDto>(_relationType1.Id), Is.Null);
            Assert.That(TRAL.GetDto<RelationTypeDto>(_relationType2.Id), Is.Null);
        }

        [Test(Description = "Test 'public RelationType(int id)' and 'private void PopulateFromDTO(RelationTypeDto dto)' methods")]
        public void _2nd_Test_RelationType_Constructor_and_PopulateFromDto()
        {
            var testRelationType = new RelationType(_relationType1.Id);
            assertRelationTypeSetup(testRelationType, TRAL.GetDto<RelationTypeDto>(testRelationType.Id));   
        }


        private void assertRelationTypeSetup(RelationType testRelationType, RelationTypeDto savedRelationType)
        {
            Assert.That(testRelationType.Id, Is.EqualTo(savedRelationType.Id));
            Assert.That(testRelationType.Dual, Is.EqualTo(savedRelationType.Dual));
            Assert.That(testRelationType.Name, Is.EqualTo(savedRelationType.Name));
            Assert.That(testRelationType.Alias, Is.EqualTo(savedRelationType.Alias));
        }

        [Test(Description = "Test 'public static RelationType GetById(int id)' method")]
        public void Test_RelationType_GetById()
        {
            var testRelationType = RelationType.GetById(_relationType1.Id);
            assertRelationTypeSetup(testRelationType, TRAL.GetDto<RelationTypeDto>(testRelationType.Id));
        }
     
        [Test(Description = "Test 'public static RelationType GetByAlias(string Alias)' method")]
        public void Test_RelationType_GetByAlias()
        {
            var testRelationType = RelationType.GetByAlias(_relationType1.Alias);
            assertRelationTypeSetup(testRelationType, TRAL.GetDto<RelationTypeDto>(testRelationType.Id));
        }

        [Test(Description = "Test 'public static IEnumerable<RelationType> GetAll()' method")]
        public void Test_RelationType_GetAll()
        {
            int expectedValue = TRAL.Relation.CountAllRelationTypes; 
            var relationTypes = RelationType.GetAll().ToArray<RelationType>();

            Assert.That(relationTypes.Length, Is.EqualTo(expectedValue));  // 1 default + 2 created in this test suite
        }


        [Test(Description = "Test 'public string Name' property ")]
        public void Test_RelationType_SetName()
        {
            var newValue = "New Name";
            var expectedValue = newValue;
            TRAL.Setter_Persists_Ext<RelationType, string, string>(
                    n => n.Name,
                    n => n.Name = newValue,
                    "umbracoRelationType",
                    "name",
                    expectedValue,
                    "Id",
                    _relationType1.Id
                );
        }

        [Test(Description = "Test 'public string Alias' property set")]
        public void Test_RelationType_SetAlias()
        {
            var newValue = "newAlias" + uniqueAliasSuffix;
            var expectedValue = newValue;
            TRAL.Setter_Persists_Ext<RelationType, string, string>(
                    n => n.Alias,
                    n => n.Alias = newValue,
                    "umbracoRelationType",
                    "alias",
                    expectedValue,
                    "Id",
                    _relationType1.Id
                );
        }

        [Test(Description = "Test 'public bool Dual' property set")]
        public void Test_RelationType_SetDual()
        {
            var newValue = !_relationType1.Dual; 
            var expectedValue = newValue;
            TRAL.Setter_Persists_Ext<RelationType, bool, bool>(
                    n => n.Dual,
                    n => n.Dual = newValue,
                    "umbracoRelationType",
                    "dual",
                    expectedValue,
                    "Id",
                    _relationType1.Id
                );
        }
 
    }
}
