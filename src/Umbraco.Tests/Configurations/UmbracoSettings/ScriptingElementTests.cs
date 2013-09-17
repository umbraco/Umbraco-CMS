using System;
using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class ScriptingElementTests : UmbracoSettingsTests
    {
        [Test]
        public void NotDynamicXmlDocumentElements()
        {
            Assert.IsTrue(SettingsSection.Scripting.NotDynamicXmlDocumentElements
                                 .All(x => "p,div,ul,span".Split(',').Contains(x.Element)));
        }

        [Test]
        public virtual void DataTypeModelStaticMappings()
        {
            var mappings = SettingsSection.Scripting.DataTypeModelStaticMappings.ToArray();
            Assert.IsTrue(mappings[0].DataTypeGuid == Guid.Parse("A3DB4034-BCB0-4E69-B3EE-DD4E6ECA74C2"));
            Assert.IsTrue(mappings[0].MappingName == "MyName.1");

            Assert.IsTrue(mappings[1].NodeTypeAlias == "textPage2");
            Assert.IsTrue(mappings[1].PropertyTypeAlias == "propertyAlias2");
            Assert.IsTrue(mappings[1].MappingName == "MyName.2");

            Assert.IsTrue(mappings[2].DataTypeGuid == Guid.Parse("BD14E709-45BE-431C-B228-6255CDEDFCD5"));
            Assert.IsTrue(mappings[2].NodeTypeAlias == "textPage3");
            Assert.IsTrue(mappings[2].PropertyTypeAlias == "propertyAlias3");
            Assert.IsTrue(mappings[2].MappingName == "MyName.3");

            Assert.IsTrue(mappings[3].DataTypeGuid == Guid.Parse("FCE8187E-0366-4833-953A-E5ECA11AA23A"));
            Assert.IsTrue(mappings[3].NodeTypeAlias == "textPage4");
            Assert.IsTrue(mappings[3].MappingName == "MyName.4");

            Assert.IsTrue(mappings[4].DataTypeGuid == Guid.Parse("9139315A-6681-4C45-B89F-BE48D30F9AB9"));
            Assert.IsTrue(mappings[4].PropertyTypeAlias == "propertyAlias5");
            Assert.IsTrue(mappings[4].MappingName == "MyName.5");

            Assert.IsTrue(mappings[5].PropertyTypeAlias == "propertyAlias6");
            Assert.IsTrue(mappings[5].MappingName == "MyName.6");
        }
    }
}