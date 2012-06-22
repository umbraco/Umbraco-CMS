using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeMock.ArrangeActAssert;
using umbraco.Test;

namespace umbraco.Linq.Core.Tests
{
    /// <summary>
    /// Tests to replicate the functionality of the CWS2 xslt's
    /// </summary>
    [TestClass, DeploymentItem("umbraco.config")]
    public class CwsReplicationTest
    {
        public CwsReplicationTest()
        {

        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod, Isolated]
        public void CwsReplicationTest_ListGalleries()
        {
            MockHelpers.SetupFakeHttpContext();
            using (var ctx = new MyUmbracoDataContext())
            {
                var galleryList = ctx.CWSGalleryLists.First(); //this would really be a Where(g => g.Id == umbracoContext.Current.PageId.Value) but since I'm not really on a page...

                var galleries = from g in galleryList.CWSGalleries
                                where g.CWSPhotos.Count() >= 1
                                orderby g.Name
                                select new
                                {
                                    g.Name,
                                    Url = umbraco.library.NiceUrl(g.Id),
                                    Thumbnail = (string.IsNullOrEmpty(g.GalleryThumbnail) ? "/Assets/Placeholders/gallery_placeholder.gif" : g.GalleryThumbnail),
                                    PhotoCount = g.CWSPhotos.Count(),
                                };

                Assert.AreEqual(3, galleries.Count());
            }
        }
    }
}
