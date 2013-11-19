using System.Collections.Generic;
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
    public class cms_businesslogic_PreValues_Tests : BaseORMTest
    {
        protected override void EnsureData() { Ensure_PreValues_TestData(); }

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

            Assert.That(TRAL.GetDto<umbraco.cms.businesslogic.datatype.PreValue.PreValueDto>(_preValue.Id), Is.Null);
            Assert.That(TRAL.PreValue.CountDataTypeOfId(_dataTypeDefinition1.Id), Is.EqualTo(0));
            Assert.That(TRAL.PreValue.CountDataTypeNodes(_dataTypeDefinition1.Id), Is.EqualTo(0));
            Assert.That(TRAL.PreValue.CountDataTypeOfId(_dataTypeDefinition2.Id), Is.EqualTo(0));
            Assert.That(TRAL.PreValue.CountDataTypeNodes(_dataTypeDefinition2.Id), Is.EqualTo(0));
        }

        [Test(Description = "Test 'public static CountOfPreValues(int dataTypeDefId)' method")]
        public void Test_PreValues_CountOfPreValues()
        {
            Assert.That(PreValues.CountOfPreValues(_dataTypeDefinition1.Id), Is.EqualTo(_dataTypeDefinition1_PrevaluesTestCount));
            Assert.That(PreValues.CountOfPreValues(_dataTypeDefinition2.Id), Is.EqualTo(_dataTypeDefinition2_PrevaluesTestCount));
        }

        [Test(Description = "Test 'public static DeleteByDataTypeDefinition(int dataTypeDefId)' method")]
        public void Test_PreValues_DeleteByDataTypeDefinition()
        {
            Assert.That(TRAL.PreValue.CountPreValuesByDataTypeId(_dataTypeDefinition1.Id), Is.EqualTo(_dataTypeDefinition1_PrevaluesTestCount));

            PreValues.DeleteByDataTypeDefinition(_dataTypeDefinition1.Id);

            Assert.That(TRAL.PreValue.CountPreValuesByDataTypeId(_dataTypeDefinition1.Id), Is.EqualTo(0));

            Assert.That(TRAL.PreValue.CountPreValuesByDataTypeId(_dataTypeDefinition2.Id), Is.EqualTo(_dataTypeDefinition2_PrevaluesTestCount));

            PreValues.DeleteByDataTypeDefinition(_dataTypeDefinition2.Id);
            Assert.That(TRAL.PreValue.CountPreValuesByDataTypeId(_dataTypeDefinition2.Id), Is.EqualTo(0));

            initialized = false;
        }

        [Test(Description = "Test 'public static GetPreValues(int DataTypeId)' method")]
        public void Test_PreValues_GetPreValues()
        {
            Assert.That(PreValues.GetPreValues(_dataTypeDefinition1.Id).Count, Is.EqualTo(TRAL.PreValue.CountPreValuesByDataTypeId(_dataTypeDefinition1.Id)));
            Assert.That(PreValues.GetPreValues(_dataTypeDefinition2.Id).Count, Is.EqualTo(TRAL.PreValue.CountPreValuesByDataTypeId(_dataTypeDefinition2.Id)));
        }
            
    }
}
