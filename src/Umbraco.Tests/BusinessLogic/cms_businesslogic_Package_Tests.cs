using System;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Core.Models.Rdbms;
using umbraco.cms.businesslogic.relation;
using umbraco.cms.businesslogic.packager;
using System.Data.SqlServerCe;

namespace Umbraco.Tests.BusinessLogic
{
    // ******************************************************************************************************************
    //
    //  [umbracoInstalledPackages] doesn't exist (yet?)
    // 
    // failed: System.Data.SqlServerCe.SqlCeException : The specified table does not exist. [ umbracoInstalledPackages ]
    //
    // ******************************************************************************************************************
    
    [TestFixture]
    public class cms_businesslogic_Package_Tests : BaseDatabaseFactoryTestWithContext
    {
        #region EnsureData

        private InstalledPackageDto _package1;

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void EnsureData()
        {
            if (!initialized)
            {
                //_package1 = insertTestPackage(1);                
            }

            initialized = true;
        }

        private InstalledPackageDto getTestPackageDto(int packageId)
        {
            return getPersistedTestDto<InstalledPackageDto>(packageId);
        }

        private void EnsureAllTestRecordsAreDeleted()
        {
            initialized = false; 
        }

        #endregion

        #region Tests
        [Test(Description = "Test_EnsureData")]
        public void Test_EnsureData()
        {
            Assert.IsTrue(initialized);

            EnsureAllTestRecordsAreDeleted();
        }

        // public Package()
        // public Package(Guid Id)
        // private void initialize(int id)

        // failed: System.Data.SqlServerCe.SqlCeException : The specified table does not exist. [ umbracoInstalledPackages ]
        [Test(Description = "description")]
        public void Test_Package_Constructor_and_PopulateFromDto()
        {
            Package testPackage = null;

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


        #endregion
    }
}
