using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using umbraco.DataLayer;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.BusinessLogic
{
    [TestFixture]
    public class cms_businesslogic_ContentTypeTests : BaseDatabaseFactoryTest
    {
        private ISqlHelper sqlHelper;
        private bool initialized;
        private DocumentType testContentType;

        protected ISqlHelper SqlHelper
        {
            get
            {
                return sqlHelper ?? (sqlHelper = Application.SqlHelper);
            }
        }

        protected override DatabaseBehavior DatabaseTestBehavior
        {
            get
            {
                //return DatabaseBehavior.NewSchemaPerFixture;
                return DatabaseBehavior.NoDatabasePerFixture;
            }
        }

        private void EnsureData()
        {
            if (initialized) return;

            testContentType = DocumentType.MakeNew(new User(0), "Test");
            DocumentType.MakeNew(new User(0), "Test 2");

            initialized = true;
        }

        [Test]
        public void GetAll_ReturnsAllContentTypes()
        {
            EnsureData();
            var all = testContentType.GetAll();

            Assert.AreEqual(2, all.Count());
            Assert.AreEqual("Test", all.First().Alias);
            Assert.AreEqual("Test2", all.Last().Alias);
        }
    }
}