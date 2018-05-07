using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class MacroTests
    {
        [SetUp]
        public void Init()
        {
            var config = SettingsForTests.GetDefaultUmbracoSettings();
            SettingsForTests.ConfigureSettings(config);
        }

        [Test]
        public void Can_Deep_Clone()
        {
            var macro = new Macro(1, Guid.NewGuid(), true, 3, "test", "Test", false, true, true, "~/script.cshtml", MacroTypes.PartialView);
            macro.Properties.Add(new MacroProperty(6, Guid.NewGuid(), "rewq", "REWQ", 1, "asdfasdf"));

            var clone = (Macro)macro.DeepClone();

            Assert.AreNotSame(clone, macro);
            Assert.AreEqual(clone, macro);
            Assert.AreEqual(clone.Id, macro.Id);

            Assert.AreEqual(clone.Properties.Count, macro.Properties.Count);

            for (int i = 0; i < clone.Properties.Count; i++)
            {
                Assert.AreEqual(clone.Properties[i], macro.Properties[i]);
                Assert.AreNotSame(clone.Properties[i], macro.Properties[i]);
            }

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
            clone.Properties.Add(new MacroProperty(3, Guid.NewGuid(), "asdf", "SDF", 3, "asdfasdf"));
            Assert.IsTrue(asDirty.IsPropertyDirty("Properties"));
            Assert.AreEqual(1, clone.AddedProperties.Count());
            clone.Properties.Remove("rewq");
            Assert.AreEqual(1, clone.RemovedProperties.Count());

        }

    }
}
