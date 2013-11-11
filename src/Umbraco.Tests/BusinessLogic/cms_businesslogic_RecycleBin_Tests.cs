using System.Collections.Generic;
using NUnit.Framework;
using umbraco.BusinessLogic;
using Umbraco.Tests.TestHelpers;
using umbraco.cms.businesslogic.datatype;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Reflection;
using umbraco.cms.businesslogic;

namespace Umbraco.Tests.BusinessLogic
{
    /// <remarks>
    /// Current test suite just tests RecycleBin's class PetaPOCO communication against empty test database
    /// TODO: Populate test database with RecycleBin related test data. Implement additional tests.
    /// </remarks> 
    [TestFixture]
    public class cms_businesslogic_RecycleBin_Tests : BaseDatabaseFactoryTestWithContext
    {

        #region EnsureData()
        public override void Initialize()
        {
            base.Initialize();
            EnsureData(); 
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void EnsureData()
        {
            //TODO: implement population of Recycle Bin and related test data into the test database
            initialized = true;
        }

        private void EnsureAllRecycleBinRecordsAreDeleted()
        {
            //TODO: implement deletion of all Recycle Bin related test data from the test database
        }        
        #endregion

        #region Tests
        //[Test(Description = "Verify if EnsureData() executes well")]
        //public void Test_EnsureData()
        //{
        //    Assert.IsTrue(initialized);
        //}

        [Test(Description = "Test 'static int Count(RecycleBinType type)' method")]
        public void Test_Count()
        {
            EnsureAllRecycleBinRecordsAreDeleted();
            Assert.That(RecycleBin.Count(umbraco.cms.businesslogic.RecycleBin.RecycleBinType.Content), Is.EqualTo(0));
            Assert.That(RecycleBin.Count(umbraco.cms.businesslogic.RecycleBin.RecycleBinType.Media), Is.EqualTo(0));
        }

        [Test(Description = "Test 'umbraco.BusinessLogic.console.IconI[] Children' property")]
        public void Test_Children()
        {
            EnsureAllRecycleBinRecordsAreDeleted();
            var recycleBin = new RecycleBin(RecycleBin.RecycleBinType.Content); 
            Assert.That(recycleBin.Children.Length, Is.EqualTo(0));
        }

        #endregion
            
    }
}
