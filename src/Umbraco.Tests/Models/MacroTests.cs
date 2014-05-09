using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class MacroTests
    {
        [SetUp]
        public void Init()
        {
            var config = SettingsForTests.GetDefault();
            SettingsForTests.ConfigureSettings(config);
        }

        [Test]
        public void Can_Deep_Clone()
        {
            var macro = new Macro(1, true, 3, "test", "Test", "blah", "blah", "xslt", false, true, true, "script");

            var clone = (Macro)macro.DeepClone();

            Assert.AreNotSame(clone, macro);
            Assert.AreEqual(clone, macro);
            Assert.AreEqual(clone.Id, macro.Id);

            Assert.AreNotSame(clone.Properties, macro.Properties);
            Assert.AreNotSame(clone.AddedProperties, macro.AddedProperties);
            Assert.AreNotSame(clone.RemovedProperties, macro.RemovedProperties);

            //This double verifies by reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(macro, null));
            }

            //need to ensure the event handlers are wired

            var asDirty = (ICanBeDirty)clone;

            Assert.IsFalse(asDirty.IsPropertyDirty("Properties"));
            clone.Properties.Add(new MacroProperty(3, "asdf", "SDF", 3, "asdfasdf"));
            Assert.IsTrue(asDirty.IsPropertyDirty("Properties"));

        }

    }
}