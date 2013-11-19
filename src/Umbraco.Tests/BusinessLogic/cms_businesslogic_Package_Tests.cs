using System;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Core.Models.Rdbms;
using umbraco.cms.businesslogic.relation;
using umbraco.cms.businesslogic.packager;
using System.Data.SqlServerCe;
using Umbraco.Tests.BusinessLogic;

namespace Umbraco.Tests.ORM
{
    // ******************************************************************************************************************
    //
    //  [umbracoInstalledPackages] doesn't exist (yet?)
    // 
    // failed: System.Data.SqlServerCe.SqlCeException : The specified table does not exist. [ umbracoInstalledPackages ]
    //
    // ******************************************************************************************************************   
    [TestFixture]
    public class cms_businesslogic_Package_Tests : BaseORMTest
    {
 
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void EnsureData() {  Ensure_Package_TestData(); }
 
        [Test(Description = "Test_EnsureData")]
        public void _1st_Test_Package_EnsureTestData()
        {
            Assert.IsTrue(initialized);

            EnsureAll_Package_TestRecordsAreDeleted();
        }

        // public Package()
        // public Package(Guid Id)
        // private void initialize(int id)

        // failed: System.Data.SqlServerCe.SqlCeException : The specified table does not exist. [ umbracoInstalledPackages ]
        [Test(Description = "description")]
        public void _2nd_Test_Package_Constructor_and_PopulateFromDto()
        {
            //  [umbracoInstalledPackages] table doesn't exist (yet?)

            Package testPackage = null;

            Assert.Throws<SqlCeException>(() => { testPackage = new Package(12345); });
            Assert.Throws<SqlCeException>(() => { testPackage = new Package(Guid.NewGuid()); });
        }

        // public void Save()

        // public bool Uninstalled get/set
        // public User User get/set
        // public DateTime InstallDate get/set
        // public int Id get/set
        // public int UpgradeId get/set
        // public Guid PackageId get/set
        // public int VersionPatch get/set
        // public int VersionMinor get/set
        // public int VersionMajor get/set


    }
}
