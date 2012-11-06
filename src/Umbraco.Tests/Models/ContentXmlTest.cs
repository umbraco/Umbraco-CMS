using System;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.ObjectResolution;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using umbraco.editorControls.tinyMCE3;
using umbraco.interfaces;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class ContentXmlTest : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            //this ensures its reset
            PluginManager.Current = new PluginManager();

            //for testing, we'll specify which assemblies are scanned for the PluginTypeResolver
            PluginManager.Current.AssembliesToScan = new[]
				{
                    typeof(IDataType).Assembly,
                    typeof(tinyMCE3dataType).Assembly
				};

            DataTypesResolver.Current = new DataTypesResolver(
                PluginManager.Current.ResolveDataTypes());

            base.Initialize();
        }

        [TearDown]
        public override void TearDown()
        {
            DatabaseContext.Database.Dispose();

            TestHelper.ClearDatabase();

            //reset the app context
            ApplicationContext.Current = null;
            Resolution.IsFrozen = false;

            string path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", null);
        }
        [Test]
        public void Can_Generate_Xml_Representation_Of_Content()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
            ServiceContext.ContentTypeService.Save(contentType);

            var content = MockedContent.CreateTextpageContent(contentType, "Root Home", -1);
            ServiceContext.ContentService.Save(content, 0);

            var nodeName = content.ContentType.Alias.ToUmbracoAlias(StringAliasCaseType.CamelCase, true);

            // Act
            XElement element = content.ToXml();

            // Assert
            Assert.That(element, Is.Not.Null);
            Assert.That(element.Name.LocalName, Is.EqualTo(nodeName));
        } 
    }
}