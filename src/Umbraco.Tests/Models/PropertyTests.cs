using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class PropertyTests : UmbracoTestBase
    {
        [Test]
        public void Can_Deep_Clone()
        {
            // needs to be within collection to support publishing
            var ptCollection = new PropertyTypeCollection(true, new[] {new PropertyType("TestPropertyEditor", ValueStorageType.Nvarchar, "test")
            {
                Id = 3,
                CreateDate = DateTime.Now,
                DataTypeId = 5,
                PropertyEditorAlias = "propTest",
                Description = "testing",
                Key = Guid.NewGuid(),
                Mandatory = true,
                Name = "Test",
                PropertyGroupId = new Lazy<int>(() => 11),
                SortOrder = 9,
                UpdateDate = DateTime.Now,
                ValidationRegExp = "xxxx",
                ValueStorageType = ValueStorageType.Nvarchar
            }});

            var property = new Property(123, ptCollection[0])
            {
                CreateDate = DateTime.Now,
                Id = 4,
                Key = Guid.NewGuid(),
                UpdateDate = DateTime.Now
            };

            property.SetValue("hello");
            property.PublishValues();

            var clone = (Property)property.DeepClone();

            Assert.AreNotSame(clone, property);
            Assert.AreNotSame(clone.Values, property.Values);
            Assert.AreNotSame(property.PropertyType, clone.PropertyType);
            for (int i = 0; i < property.Values.Count; i++)
            {
                Assert.AreNotSame(property.Values.ElementAt(i), clone.Values.ElementAt(i));
            }
            
            
            //This double verifies by reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(property, null));
            }
        }
    }
}
