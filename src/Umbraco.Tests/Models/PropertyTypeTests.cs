using System;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Serialization;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class PropertyTypeTests : BaseUmbracoConfigurationTest
    {
        [Test]
        public void Can_Deep_Clone()
        {
            var pt = new PropertyType("TestPropertyEditor", DataTypeDatabaseType.Nvarchar, "test")
            {
                Id = 3,
                CreateDate = DateTime.Now,
                DataTypeDefinitionId = 5,
                PropertyEditorAlias = "propTest",
                Description = "testing",
                Key = Guid.NewGuid(),
                Mandatory = true,
                Name = "Test",
                PropertyGroupId = new Lazy<int>(() => 11),
                SortOrder = 9,
                UpdateDate = DateTime.Now,
                ValidationRegExp = "xxxx",
                DataTypeDatabaseType = DataTypeDatabaseType.Nvarchar                                
            };

            var clone = (PropertyType)pt.DeepClone();

            Assert.AreNotSame(clone, pt);
            Assert.AreEqual(clone, pt);
            Assert.AreEqual(clone.Id, pt.Id);
            Assert.AreEqual(clone.Alias, pt.Alias);
            Assert.AreEqual(clone.CreateDate, pt.CreateDate);
            Assert.AreEqual(clone.DataTypeDefinitionId, pt.DataTypeDefinitionId);
            Assert.AreEqual(clone.DataTypeId, pt.DataTypeId);
            Assert.AreEqual(clone.Description, pt.Description);
            Assert.AreEqual(clone.Key, pt.Key);
            Assert.AreEqual(clone.Mandatory, pt.Mandatory);
            Assert.AreEqual(clone.Name, pt.Name);
            Assert.AreEqual(clone.PropertyGroupId.Value, pt.PropertyGroupId.Value);
            Assert.AreEqual(clone.SortOrder, pt.SortOrder);
            Assert.AreEqual(clone.UpdateDate, pt.UpdateDate);
            Assert.AreEqual(clone.ValidationRegExp, pt.ValidationRegExp);
            Assert.AreEqual(clone.DataTypeDatabaseType, pt.DataTypeDatabaseType);

            //This double verifies by reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(pt, null));
            }
        }

        [Test]
        public void Can_Serialize_Without_Error()
        {
            var ss = new SerializationService(new JsonNetSerializer());

            var pt = new PropertyType("TestPropertyEditor", DataTypeDatabaseType.Nvarchar, "test")
            {
                Id = 3,
                CreateDate = DateTime.Now,
                DataTypeDefinitionId = 5,
                PropertyEditorAlias = "propTest",
                Description = "testing",
                Key = Guid.NewGuid(),
                Mandatory = true,
                Name = "Test",
                PropertyGroupId = new Lazy<int>(() => 11),
                SortOrder = 9,
                UpdateDate = DateTime.Now,
                ValidationRegExp = "xxxx",
                DataTypeDatabaseType = DataTypeDatabaseType.Nvarchar
            };

            var result = ss.ToStream(pt);
            var json = result.ResultStream.ToJsonString();
            Console.WriteLine(json);
        }

    }
}