using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class FileServiceTests : TestWithSomeContentBase
    {
        [Test]
        public void Create_Template_Then_Assign_Child()
        {
            var child = ServiceContext.FileService.CreateTemplateWithIdentity("Child", "child", "test");
            var parent = ServiceContext.FileService.CreateTemplateWithIdentity("Parent", "parent", "test");

            child.SetMasterTemplate(parent);
            ServiceContext.FileService.SaveTemplate(child);

            child = ServiceContext.FileService.GetTemplate(child.Id);

            Assert.AreEqual(parent.Alias, child.MasterTemplateAlias);

        }

        [Test]
        public void Create_Template_With_Child_Then_Unassign()
        {
            var parent = ServiceContext.FileService.CreateTemplateWithIdentity("Parent", "parent", "test");
            var child = ServiceContext.FileService.CreateTemplateWithIdentity("Child", "child", "test", parent);

            child.SetMasterTemplate(null);
            ServiceContext.FileService.SaveTemplate(child);

            child = ServiceContext.FileService.GetTemplate(child.Id);

            Assert.AreEqual(null, child.MasterTemplateAlias);
        }

        [Test]
        public void Can_Query_Template_Children()
        {
            var parent = ServiceContext.FileService.CreateTemplateWithIdentity("Parent", "parent", "test");
            var child1 = ServiceContext.FileService.CreateTemplateWithIdentity("Child1", "child1", "test", parent);
            var child2 = ServiceContext.FileService.CreateTemplateWithIdentity("Child2", "child2", "test", parent);

            var children = ServiceContext.FileService.GetTemplates(parent.Id).Select(x => x.Id).ToArray();

            Assert.IsTrue(children.Contains(child1.Id));
            Assert.IsTrue(children.Contains(child2.Id));
        }

        [Test]
        public void Create_Template_With_Custom_Alias()
        {
            var template = ServiceContext.FileService.CreateTemplateWithIdentity("Test template", "customTemplateAlias", "test");

            ServiceContext.FileService.SaveTemplate(template);

            template = ServiceContext.FileService.GetTemplate(template.Id);

            Assert.AreEqual("Test template", template.Name);
            Assert.AreEqual("customTemplateAlias", template.Alias);
        }

    }
}
