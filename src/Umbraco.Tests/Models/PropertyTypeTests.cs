using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Serialization;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class PropertyTypeTests : UmbracoTestBase
    {
        [Test]
        public void Can_Deep_Clone()
        {
            var pt = new PropertyType("TestPropertyEditor", ValueStorageType.Nvarchar, "test")
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
            };

            var clone = (PropertyType)pt.DeepClone();

            Assert.AreNotSame(clone, pt);
            Assert.AreEqual(clone, pt);
            Assert.AreEqual(clone.Id, pt.Id);
            Assert.AreEqual(clone.Alias, pt.Alias);
            Assert.AreEqual(clone.CreateDate, pt.CreateDate);
            Assert.AreEqual(clone.DataTypeId, pt.DataTypeId);
            Assert.AreEqual(clone.DataTypeId, pt.DataTypeId);
            Assert.AreEqual(clone.Description, pt.Description);
            Assert.AreEqual(clone.Key, pt.Key);
            Assert.AreEqual(clone.Mandatory, pt.Mandatory);
            Assert.AreEqual(clone.Name, pt.Name);
            Assert.AreEqual(clone.PropertyGroupId.Value, pt.PropertyGroupId.Value);
            Assert.AreEqual(clone.SortOrder, pt.SortOrder);
            Assert.AreEqual(clone.UpdateDate, pt.UpdateDate);
            Assert.AreEqual(clone.ValidationRegExp, pt.ValidationRegExp);
            Assert.AreEqual(clone.ValueStorageType, pt.ValueStorageType);

            //This double verifies by reflection (don't test properties marked with [DoNotClone]
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps.Where(p => p.GetCustomAttribute<DoNotCloneAttribute>(false) == null))
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(pt, null));
            }
        }

        [Test]
        public void Can_Serialize_Without_Error()
        {
            var ss = new SerializationService(new JsonNetSerializer());

            var pt = new PropertyType("TestPropertyEditor", ValueStorageType.Nvarchar, "test")
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
            };

            var result = ss.ToStream(pt);
            var json = result.ResultStream.ToJsonString();
            Debug.Print(json);
        }

    }
}
