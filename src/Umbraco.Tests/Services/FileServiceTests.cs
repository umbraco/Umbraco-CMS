using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Services
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture, RequiresSTA]
    public class FileServiceTests : BaseServiceTest
    {

        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void Create_Template_Then_Assign_Child()
        {
            var child = ServiceContext.FileService.CreateTemplateWithIdentity("child", "test");
            var parent = ServiceContext.FileService.CreateTemplateWithIdentity("parent", "test");

            child.SetMasterTemplate(parent);
            ServiceContext.FileService.SaveTemplate(child);

            child = ServiceContext.FileService.GetTemplate(child.Id);

            Assert.AreEqual(parent.Alias, child.MasterTemplateAlias);

        }

        [Test]
        public void Create_Template_With_Child_Then_Unassign()
        {
            var parent = ServiceContext.FileService.CreateTemplateWithIdentity("parent", "test");
            var child = ServiceContext.FileService.CreateTemplateWithIdentity("child", "test", parent);

            child.SetMasterTemplate(null);
            ServiceContext.FileService.SaveTemplate(child);

            child = ServiceContext.FileService.GetTemplate(child.Id);

            Assert.AreEqual(null, child.MasterTemplateAlias);
        }

        [Test]
        public void Can_Query_Template_Children()
        {
            var parent = ServiceContext.FileService.CreateTemplateWithIdentity("parent", "test");
            var child1 = ServiceContext.FileService.CreateTemplateWithIdentity("child1", "test", parent);
            var child2 = ServiceContext.FileService.CreateTemplateWithIdentity("child2", "test", parent);

            var children = ServiceContext.FileService.GetTemplates(parent.Id).Select(x => x.Id).ToArray();

            Assert.IsTrue(children.Contains(child1.Id));
            Assert.IsTrue(children.Contains(child2.Id));
        }

    }
}
