using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using umbraco.cms.businesslogic;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using ContentType = Umbraco.Core.Models.ContentType;

namespace Umbraco.Tests.BusinessLogic
{
    [TestFixture]
    public class cms_businesslogic_CMSNodeTests : BaseDatabaseFactoryTest
    {
        private const int TextStringDataTypeId = -88;
        private const string RootObjectTypeId = "EA7D8624-4CFE-4578-A871-24AA946BF34D";
        private const string MemberObjectTypeId = "9B5416FB-E72F-45A9-A07B-5A9A2709CE43";
        private const string RecycleBin1ObjectTypeId = "CF3D8E34-1C1C-41E9-AE56-878B57B32113";
        private const string MediaTypeObjectTypeId = "4EA4382B-2F5A-4C2B-9587-AE9B3CF3602E";
        private const string DocumentTypeObjectTypeId = "A2CB7800-F571-4787-9638-BC48539A0EFB";
        private const string RecycleBin2ObjectTypeId = "01BB7FF2-24DC-4C0C-95A2-C24EF72BBAC8";
        private const string DataTypeObjectTypeId = "30a2a501-1978-4ddb-a57b-f7efed43ba3c";
        private readonly Guid textStringDataTypeUniqueId = new Guid("0CC0EBA1-9960-42C9-BF9B-60E150B429AE");
        private readonly Guid dataTypeObjectTypeId = new Guid(DataTypeObjectTypeId);
        private bool initialized;
        private ContentType testContentType1;
        private ContentType testContentType2;
        private ContentType testContentType3;
        private UmbracoContext context;

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

        [Test]
        [TestCase(RootObjectTypeId, 1, "Root")]
        [TestCase(MemberObjectTypeId, 1, "Member")]
        [TestCase(RecycleBin1ObjectTypeId, 1, "Recycle bin 1")]
        [TestCase(MediaTypeObjectTypeId, 3, "MediaType")]
        [TestCase(DocumentTypeObjectTypeId, 3, "DocumentType")]
        [TestCase(RecycleBin2ObjectTypeId, 1, "Recycle Bin 2")]
        [TestCase(DataTypeObjectTypeId, 24, "DataType")]
        public void CountByObjectType_ReturnsCount(string objectTypeId, int expected, string description)
        {
            EnsureTestDocumentTypes();
            var actual = CMSNode.CountByObjectType(new Guid(objectTypeId));
            Assert.AreEqual(expected, actual, "There should be {0} of nodeObjectType {1}", expected, description);
        }

        [Test]
        [TestCase(RootObjectTypeId, -1, 1, "Root is its own child")]
        [TestCase(DocumentTypeObjectTypeId, -1, 2, "Two root document types")]
        public void CountLeafNodes_Roots_ReturnsCount(string objectTypeId, int parentId, int expected, string description)
        {
            EnsureTestDocumentTypes();
            var actual = CMSNode.CountLeafNodes(parentId, new Guid(objectTypeId));
            Assert.AreEqual(expected, actual, description);
        }

        [Test]
        public void CountLeafNodes_DocumentTypeChild_ReturnsCount()
        {
            EnsureTestDocumentTypes();
            var actual = CMSNode.CountLeafNodes(testContentType1.Id, new Guid(DocumentTypeObjectTypeId));
            Assert.AreEqual(1, actual);
        }

        [Test]
        public void CountSubs_ReturnsCountOfAncestorsOrSomething()
        {
            const int expectedRootCount = 34;
            const int expectedGrandChildCount = 2;
            const int expectedChildCount = 1;

            EnsureTestDocumentTypes();
            Assert.AreEqual(expectedRootCount, CMSNode.CountSubs(-1));
            Assert.AreEqual(expectedGrandChildCount, CMSNode.CountSubs(testContentType1.Id));
            Assert.AreEqual(expectedChildCount, CMSNode.CountSubs(testContentType2.Id));
        }

        [Test]
        public void Delete_DeletesRowFromUmbracoNode()
        {
            EnsureTestDocumentTypes();
            var database = context.Application.DatabaseContext.Database;
            var originalCount = database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode");
            try
            {
                var node = new CMSNode(testContentType2.Id);
                database.Execute("DELETE cmsContentType WHERE nodeId = @NodeId", new { NodeId = node.Id });
                node.delete();
                Assert.AreEqual(0, database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode WHERE id = @id", new{id = node.Id}));;
                var newCount = database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode");
                Assert.AreEqual(originalCount - 1, newCount);
            }
            finally
            {
                foreach(var node in new[] { testContentType1, testContentType2, testContentType3 })
                    database.Execute("DELETE cmsContentType WHERE nodeId = @NodeId", new {NodeId = node.Id});
                foreach (var node in new[] { testContentType3, testContentType1, testContentType2 })
                    database.Execute("DELETE umbracoNode WHERE id = @Id", new {node.Id });
                initialized = false;
            }
        }

        private void EnsureTestDocumentTypes()
        {
            if (initialized) return;
            testContentType1 = new ContentType(-1) {Alias="Test1"};
            testContentType2 = new ContentType(-1) {Alias="Test2"};
            context = GetUmbracoContext("http://localhost", 0);
            var contentTypeService = context.Application.Services.ContentTypeService;
            contentTypeService.Save(testContentType1);
            contentTypeService.Save(testContentType2);
            testContentType3 = new ContentType(testContentType1.Id) {Alias="Test1.1"};
            contentTypeService.Save(testContentType3);
            initialized = true;
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
