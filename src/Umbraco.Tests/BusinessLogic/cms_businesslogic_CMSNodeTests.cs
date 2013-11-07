using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using umbraco.cms.businesslogic;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.BusinessLogic
{
    [TestFixture]
    public class cms_businesslogic_CMSNodeTests : BaseDatabaseFactoryTest
    {
        private const int TextStringDataTypeId = -88;
        private readonly Guid textStringDataTypeUniqueId = new Guid("0CC0EBA1-9960-42C9-BF9B-60E150B429AE");
        private readonly Guid dataTypeObjectTypeId = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c");

        protected override DatabaseBehavior DatabaseTestBehavior
        {
            get { return DatabaseBehavior.NewSchemaPerFixture; }
        }

        [Test]
        public void Ctor_Int_PopulatesNode()
        {
            var node = new CMSNode(TextStringDataTypeId);
            AssertTextStringDataTypeNode(node);
        }

        [Test]
        public void Ctor_Int_NoSetup_AssignsIdOnly()
        {
            var node = new CMSNode(TextStringDataTypeId, true);
            AssertTextStringDataTypeIdOnly(node);
        }

        [Test]
        public void Ctor_Int_NoSetup_False_PopulatesNode()
        {
            var node = new CMSNode(TextStringDataTypeId, false);
            AssertTextStringDataTypeNode(node);
        }

        [Test]
        public void Ctor_Guid_PopulatesNode()
        {
            var node = new CMSNode(textStringDataTypeUniqueId);
            AssertTextStringDataTypeNode(node);
        }

        [Test]
        public void Ctor_Guid_NoSetup_AssignsIdOnly()
        {
            var node = new CMSNode(textStringDataTypeUniqueId, true);
            AssertTextStringDataTypeIdOnly(node);
        }

        [Test]
        public void Ctor_Guid_NoSetup_False_PopulatesNode()
        {
            var node = new CMSNode(textStringDataTypeUniqueId, false);
            AssertTextStringDataTypeNode(node);
        }

        private void AssertTextStringDataTypeNode(CMSNode node)
        {
            Assert.AreEqual(TextStringDataTypeId, node.Id);
            Assert.AreEqual(textStringDataTypeUniqueId, node.UniqueId);
            Assert.AreEqual(dataTypeObjectTypeId, node.nodeObjectType);
            Assert.AreEqual(1, node.Level);
            Assert.AreEqual("-1,-88", node.Path);
            Assert.AreEqual(-1, node.ParentId);
            Assert.AreEqual("Textstring", node.Text);
            Assert.AreEqual(32, node.sortOrder);
            Assert.AreEqual(0, node.User.Id);
            Assert.AreEqual(DateTime.Today, node.CreateDateTime.Date);
            Assert.IsFalse(node.IsTrashed);
        }

        private static void AssertTextStringDataTypeIdOnly(CMSNode node)
        {
            Assert.AreEqual(TextStringDataTypeId, node.Id);
            Assert.AreEqual(Guid.Empty, node.UniqueId); // ??
            Assert.AreEqual(Guid.Empty, node.nodeObjectType);
            Assert.AreEqual(0, node.Level);
            Assert.IsNull(node.Path);
            Assert.AreEqual(0, node.ParentId);
            Assert.IsNull(node.Text);
            Assert.AreEqual(0, node.sortOrder);
            Assert.AreEqual(0, node.User.Id);
            Assert.AreEqual(DateTime.MinValue, node.CreateDateTime);
            Assert.IsFalse(node.IsTrashed);
            
        }
    }
}
