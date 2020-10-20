﻿using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Common.Builders.Extensions;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Models
{
    [TestFixture]
    public class MacroTests
    {
        private MacroBuilder _builder;

        [SetUp]
        public void SetUp()
        {
            _builder = new MacroBuilder();
        }

        [Test]
        public void Can_Deep_Clone()
        {
            var macro = _builder
                .WithId(1)
                .WithUseInEditor(true)
                .WithCacheDuration(3)
                .WithAlias("test")
                .WithName("Test")
                .WithSource("~/script.cshtml")
                .WithCacheByMember(true)
                .WithDontRender(true)
                .AddProperty()
                    .WithId(6)
                    .WithAlias("rewq")
                    .WithName("REWQ")
                    .WithSortOrder(1)
                    .WithEditorAlias("asdfasdf")
                    .Done()
                .Build();

            var clone = (Macro)macro.DeepClone();

            Assert.AreNotSame(clone, macro);
            Assert.AreEqual(clone, macro);
            Assert.AreEqual(clone.Id, macro.Id);

            Assert.AreEqual(clone.Properties.Count, macro.Properties.Count);

            for (var i = 0; i < clone.Properties.Count; i++)
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
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(macro, null));

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
