using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Common.Builders.Extensions;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Models
{ 
    [TestFixture]
    public class PropertyTests
    {
        private PropertyBuilder _builder;

        [SetUp]
        public void SetUp()
        {
            _builder = new PropertyBuilder();
        }

        [Test]
        public void Can_Deep_Clone()
        {
            var property = BuildProperty();

            property.SetValue("hello");
            property.PublishValues();

            var clone = (Property)property.DeepClone();

            Assert.AreNotSame(clone, property);
            Assert.AreNotSame(clone.Values, property.Values);
            Assert.AreNotSame(property.PropertyType, clone.PropertyType);
            for (var i = 0; i < property.Values.Count; i++)
            {
                Assert.AreNotSame(property.Values.ElementAt(i), clone.Values.ElementAt(i));
            }

            // This double verifies by reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(property, null));
            }
        }

        private IProperty BuildProperty()
        {
            return _builder
                .WithId(4)
                .AddPropertyType()
                    .WithId(3)
                    .WithPropertyEditorAlias("TestPropertyEditor")
                    .WithAlias("test")
                    .WithName("Test")
                    .WithSortOrder(9)
                    .WithDataTypeId(5)
                    .WithDescription("testing")
                    .WithPropertyGroupId(11)
                    .WithMandatory(true)
                    .WithValidationRegExp("xxxx")
                    .Done()
                .Build();
        }
    }
}
