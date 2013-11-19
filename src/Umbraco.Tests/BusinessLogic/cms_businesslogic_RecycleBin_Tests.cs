using System.Collections.Generic;
using NUnit.Framework;
using umbraco.BusinessLogic;
using Umbraco.Tests.TestHelpers;
using umbraco.cms.businesslogic.datatype;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Reflection;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.web;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core;
using Umbraco.Tests.BusinessLogic;

namespace Umbraco.Tests.ORM
{
    /// <remarks>
    /// Current test suite just tests RecycleBin's class PetaPOCO communication against empty test database
    /// TODO: Populate test database with RecycleBin related test data. Implement additional tests.
    /// </remarks> 
    [TestFixture]
    public class cms_businesslogic_RecycleBin_Tests : BaseORMTest
    {
        protected override void EnsureData() { Ensure_RecycleBin_TestData(); }

        [Test(Description = "Verify if EnsureData() executes well")]
        public void Test_RecycleBin_EnsureData()
        {
            Assert.IsTrue(initialized);

            Assert.That(_recycleBinNode1, !Is.Null);
            Assert.That(_recycleBinNode2, !Is.Null);
            Assert.That(_recycleBinNode3, !Is.Null);
            Assert.That(_recycleBinNode4, !Is.Null);
            Assert.That(_recycleBinNode5, !Is.Null);

            EnsureAll_RecycleBin_TestRecordsAreDeleted();

            Assert.That(TRAL.GetDto<NodeDto>(_recycleBinNode1.Id), Is.Null);
            Assert.That(TRAL.GetDto<NodeDto>(_recycleBinNode2.Id), Is.Null);
            Assert.That(TRAL.GetDto<NodeDto>(_recycleBinNode3.Id), Is.Null);
            Assert.That(TRAL.GetDto<NodeDto>(_recycleBinNode4.Id), Is.Null);
            Assert.That(TRAL.GetDto<NodeDto>(_recycleBinNode5.Id), Is.Null);

        }

        const string countSQL = @"select count(id) from umbracoNode where nodeObjectType = @nodeObjectType and path like '%,{0},%'";

        [Test(Description = "Test 'static int Count(RecycleBinType type)' method")]
        public void Test_RecycleBin_Count_Media()
        {
            int testCount = RecycleBin.Count(umbraco.cms.businesslogic.RecycleBin.RecycleBinType.Media);
            int savedCount = TRAL.RecycleBin.MediaItemsCount;

            Assert.That(testCount, Is.EqualTo(savedCount));
        }

        [Test(Description = "Test 'static int Count(RecycleBinType type)' method for Constants.System.RecycleBinContent")]
        public void Test_RecycleBin_Count_Content()
        {
            int testCount = RecycleBin.Count(umbraco.cms.businesslogic.RecycleBin.RecycleBinType.Content);
            int savedCount = TRAL.RecycleBin.ContentItemsCount;

            Assert.That(testCount, Is.EqualTo(savedCount));
        }


        [Test(Description = "Test 'umbraco.BusinessLogic.console.IconI[] Children' property for Constants.System.RecycleBinMedia")]
        public void Test_RecycleBin_Media_Children()
        {
            var recycleBin = new RecycleBin(RecycleBin.RecycleBinType.Media);
            int savedCount = TRAL.RecycleBin.ChildrenMediaItemsCount;

            Assert.That(recycleBin.Children.Length, Is.EqualTo(savedCount));
        }

        [Test(Description = "Test 'umbraco.BusinessLogic.console.IconI[] Children' property for Constants.System.RecycleBinContent")]
        public void Test_RecycleBin_Content_Children()
        {
            var recycleBin = new RecycleBin(RecycleBin.RecycleBinType.Content);
            int savedCount = TRAL.RecycleBin.ChildrenContentItemsCount;

            Assert.That(recycleBin.Children.Length, Is.EqualTo(savedCount));
        }
    }
}
