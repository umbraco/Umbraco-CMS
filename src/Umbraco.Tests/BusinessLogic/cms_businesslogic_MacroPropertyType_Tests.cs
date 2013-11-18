//#define TRACE_EXECUTION_SPEED
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Core.Models.Rdbms;
using umbraco.cms.businesslogic.relation;
using umbraco.cms.businesslogic.macro;

namespace Umbraco.Tests.BusinessLogic
{
    [TestFixture]
    public class cms_businesslogic_MacroPropertyType_Tests : BaseORMTest
    {
        protected override void EnsureData() { Ensure_MacroPropertyType_TestData(); }

        [Test(Description = "Test EnsureData()")]
        public void _1st_Test_MacroPropertyType_EnsureData()
        {
            Assert.IsTrue(initialized);
            Assert.That(_macroPropertyType1, !Is.Null);

            EnsureAll_MacroPropertyType_TestRecordsAreDeleted();

            Assert.That(TRAL.GetDto<MacroPropertyTypeDto>(_macroPropertyType1.Id), Is.Null);
        }

        [Test(Description = "Test 'public MacroPropertyType(int Id)' constructor")]
        public void _2nd_Test_MacroPropertyType_Constructor_I_and_setup()
        {
            var testMacroPropertyType = new MacroPropertyType(_macroPropertyType1.Id);
            Assert.That(testMacroPropertyType, !Is.Null);
            assertMacroPropertyTypeSetup(testMacroPropertyType, _macroPropertyType1); 
        }

        private void assertMacroPropertyTypeSetup(MacroPropertyType testMacroPropertyType, MacroPropertyTypeDto savedMacropPropertyType)
        {
            Assert.That(testMacroPropertyType.Id, Is.EqualTo(savedMacropPropertyType.Id), "Id test failed");
            Assert.That(testMacroPropertyType.Alias, Is.EqualTo(savedMacropPropertyType.Alias), "Alias test failed");
            Assert.That(testMacroPropertyType.Assembly, Is.EqualTo(savedMacropPropertyType.RenderAssembly), "RenderAssembly test failed");
            Assert.That(testMacroPropertyType.Type, Is.EqualTo(savedMacropPropertyType.RenderType), "RenderType test failed");
            Assert.That(testMacroPropertyType.BaseType, Is.EqualTo(savedMacropPropertyType.BaseType), "BaseType test failed");
        }

        [Test(Description = "Test 'public MacroPropertyType(string Alias)' constructor")]
        public void _3rd_Test_MacroPropertyType_Constructor_II_and_Setup()
        {
            var testMacroPropertyType = new MacroPropertyType(_macroPropertyType1.Alias);
            Assert.That(testMacroPropertyType, !Is.Null);
            assertMacroPropertyTypeSetup(testMacroPropertyType, _macroPropertyType1); 
        }

        // TRACE_EXECUTION_SPEED result
        //1. 6:12:19 PM
        //2. 86 6:12:22 PM  !!!
        //3. 86 6:12:22 PM
        //4. 86 6:12:26 PM  !!!
        [Test(Description = "Test 'public static List<MacroPropertyType> GetAll' method")]
        public void Test_MacroPropertyType_GetAll()
        {
#if TRACE_EXECUTION_SPEED
            l("1. {0:T}", DateTime.Now);  
#endif
            var all = MacroPropertyType.GetAll; // ! 3 seconds for 86 property types
#if TRACE_EXECUTION_SPEED
            l("2. {0} {1:T}", all.Count, DateTime.Now);
#endif
            int count =  TRAL.Macro.CountAllPropertyTypes;
#if TRACE_EXECUTION_SPEED
            l("3. {0} {1:T}", count, DateTime.Now);
#endif
            Assert.That(all.Count, Is.EqualTo(count));

            all.ForEach(x =>  assertMacroPropertyTypeSetup(x, TRAL.GetDto<MacroPropertyTypeDto>(x.Id)));
#if TRACE_EXECUTION_SPEED
            l("4. {0} {1:T}", count, DateTime.Now);
#endif
        }
    }
}
