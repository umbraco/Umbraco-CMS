using System;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Serialization;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class PropertyGroupTests : BaseUmbracoConfigurationTest
    {
        [Test]
        public void Can_Deep_Clone()
        {
            var pg = new PropertyGroup(
                new PropertyTypeCollection(new[]
                {
                    new PropertyType("TestPropertyEditor", DataTypeDatabaseType.Nvarchar, "test")
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
                    },
                    new PropertyType("TestPropertyEditor", DataTypeDatabaseType.Nvarchar, "test2")
                    {
                        Id = 4,
                        CreateDate = DateTime.Now,
                        DataTypeDefinitionId = 6,
                        PropertyEditorAlias = "propTest",
                        Description = "testing2",
                        Key = Guid.NewGuid(),
                        Mandatory = true,
                        Name = "Test2",
                        PropertyGroupId = new Lazy<int>(() => 12),
                        SortOrder = 10,
                        UpdateDate = DateTime.Now,
                        ValidationRegExp = "yyyy",
                        DataTypeDatabaseType = DataTypeDatabaseType.Nvarchar
                    }
                }))
            {
                Id = 77,
                CreateDate = DateTime.Now,
                Key = Guid.NewGuid(),
                Name = "Group1",
                SortOrder = 555,
                UpdateDate = DateTime.Now,
                ParentId = 9
            };

            var clone = (PropertyGroup)pg.DeepClone();

            Assert.AreNotSame(clone, pg);
            Assert.AreEqual(clone, pg);
            Assert.AreEqual(clone.Id, pg.Id);
            Assert.AreEqual(clone.CreateDate, pg.CreateDate);
            Assert.AreEqual(clone.Key, pg.Key);
            Assert.AreEqual(clone.Name, pg.Name);
            Assert.AreEqual(clone.SortOrder, pg.SortOrder);
            Assert.AreEqual(clone.UpdateDate, pg.UpdateDate);
            Assert.AreEqual(clone.ParentId, pg.ParentId);
            Assert.AreNotSame(clone.PropertyTypes, pg.PropertyTypes);
            Assert.AreEqual(clone.PropertyTypes, pg.PropertyTypes);
            Assert.AreEqual(clone.PropertyTypes.Count, pg.PropertyTypes.Count);
            for (var i = 0; i < pg.PropertyTypes.Count; i++)
            {
                Assert.AreNotSame(clone.PropertyTypes[i], pg.PropertyTypes[i]);
                Assert.AreEqual(clone.PropertyTypes[i], pg.PropertyTypes[i]);
            }

            //This double verifies by reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(pg, null));
            }
        }

        [Test]
        public void Can_Serialize_Without_Error()
        {
            var ss = new SerializationService(new JsonNetSerializer());

            var pg = new PropertyGroup(
                new PropertyTypeCollection(new[]
                {
                    new PropertyType("TestPropertyEditor", DataTypeDatabaseType.Nvarchar, "test")
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
                    },
                    new PropertyType("TestPropertyEditor2", DataTypeDatabaseType.Nvarchar, "test2")
                    {
                        Id = 4,
                        CreateDate = DateTime.Now,
                        DataTypeDefinitionId = 6,
                        PropertyEditorAlias = "propTest",
                        Description = "testing2",
                        Key = Guid.NewGuid(),
                        Mandatory = true,
                        Name = "Test2",
                        PropertyGroupId = new Lazy<int>(() => 12),
                        SortOrder = 10,
                        UpdateDate = DateTime.Now,
                        ValidationRegExp = "yyyy",
                        DataTypeDatabaseType = DataTypeDatabaseType.Nvarchar
                    }
                }))
            {
                Id = 77,
                CreateDate = DateTime.Now,
                Key = Guid.NewGuid(),
                Name = "Group1",
                SortOrder = 555,
                UpdateDate = DateTime.Now,
                ParentId = 9
            };

            var result = ss.ToStream(pg);
            var json = result.ResultStream.ToJsonString();
            Console.WriteLine(json);
        }
    }
}