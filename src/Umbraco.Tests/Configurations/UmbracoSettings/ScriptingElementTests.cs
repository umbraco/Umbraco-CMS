using System;
using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class ScriptingElementDefaultTests : ScriptingElementTests
    {
        protected override bool TestingDefaults
        {
            get { return true; }
        }

        [Test]
        public override void DataTypeModelStaticMappings()
        {
            Assert.AreEqual(0, Section.Scripting.Razor.DataTypeModelStaticMappings.Count);
        }

    }

    [TestFixture]
    public class ScriptingElementTests : UmbracoSettingsTests
    {
        [Test]
        public void NotDynamicXmlDocumentElements()
        {
            Assert.IsTrue(Section.Scripting.Razor.NotDynamicXmlDocumentElements
                                 .All(x => "p,div,ul,span".Split(',').Contains(x.Value)));
        }

        [Test]
        public virtual void DataTypeModelStaticMappings()
        {
            var mappings = Section.Scripting.Razor.DataTypeModelStaticMappings.ToArray();
            Assert.IsTrue(mappings[0].DataTypeGuid == Guid.Parse("A3DB4034-BCB0-4E69-B3EE-DD4E6ECA74C2"));
            Assert.IsTrue(mappings[0] == "MyName.1");

            Assert.IsTrue(mappings[1].DocumentTypeAlias == "textPage2");
            Assert.IsTrue(mappings[1].NodeTypeAlias == "propertyAlias2");
            Assert.IsTrue(mappings[1] == "MyName.2");

            Assert.IsTrue(mappings[2].DataTypeGuid == Guid.Parse("BD14E709-45BE-431C-B228-6255CDEDFCD5"));
            Assert.IsTrue(mappings[2].DocumentTypeAlias == "textPage3");
            Assert.IsTrue(mappings[2].NodeTypeAlias == "propertyAlias3");
            Assert.IsTrue(mappings[2] == "MyName.3");

            Assert.IsTrue(mappings[3].DataTypeGuid == Guid.Parse("FCE8187E-0366-4833-953A-E5ECA11AA23A"));
            Assert.IsTrue(mappings[3].DocumentTypeAlias == "textPage4");
            Assert.IsTrue(mappings[3] == "MyName.4");

            Assert.IsTrue(mappings[4].DataTypeGuid == Guid.Parse("9139315A-6681-4C45-B89F-BE48D30F9AB9"));
            Assert.IsTrue(mappings[4].NodeTypeAlias == "propertyAlias5");
            Assert.IsTrue(mappings[4] == "MyName.5");

            Assert.IsTrue(mappings[5].NodeTypeAlias == "propertyAlias6");
            Assert.IsTrue(mappings[5] == "MyName.6");
        }
    }
}