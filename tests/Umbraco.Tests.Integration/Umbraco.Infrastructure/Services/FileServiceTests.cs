// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class FileServiceTests : UmbracoIntegrationTest
    {
        private IFileService FileService => GetRequiredService<IFileService>();

        [Test]
        public void Create_Template_Then_Assign_Child()
        {
            ITemplate child = FileService.CreateTemplateWithIdentity("Child", "child", "test");
            ITemplate parent = FileService.CreateTemplateWithIdentity("Parent", "parent", "test");

            child.SetMasterTemplate(parent);
            FileService.SaveTemplate(child);

            child = FileService.GetTemplate(child.Id);

            Assert.AreEqual(parent.Alias, child.MasterTemplateAlias);
        }

        [Test]
        public void Create_Template_With_Child_Then_Unassign()
        {
            ITemplate parent = FileService.CreateTemplateWithIdentity("Parent", "parent", "test");
            ITemplate child = FileService.CreateTemplateWithIdentity("Child", "child", "test", parent);

            child.SetMasterTemplate(null);
            FileService.SaveTemplate(child);

            child = FileService.GetTemplate(child.Id);

            Assert.AreEqual(null, child.MasterTemplateAlias);
        }

        [Test]
        public void Can_Query_Template_Children()
        {
            ITemplate parent = FileService.CreateTemplateWithIdentity("Parent", "parent", "test");
            ITemplate child1 = FileService.CreateTemplateWithIdentity("Child1", "child1", "test", parent);
            ITemplate child2 = FileService.CreateTemplateWithIdentity("Child2", "child2", "test", parent);

            int[] children = FileService.GetTemplates(parent.Id).Select(x => x.Id).ToArray();

            Assert.IsTrue(children.Contains(child1.Id));
            Assert.IsTrue(children.Contains(child2.Id));
        }

        [Test]
        public void Create_Template_With_Custom_Alias()
        {
            ITemplate template = FileService.CreateTemplateWithIdentity("Test template", "customTemplateAlias", "test");

            FileService.SaveTemplate(template);

            template = FileService.GetTemplate(template.Id);

            Assert.AreEqual("Test template", template.Name);
            Assert.AreEqual("customTemplateAlias", template.Alias);
        }
    }
}
