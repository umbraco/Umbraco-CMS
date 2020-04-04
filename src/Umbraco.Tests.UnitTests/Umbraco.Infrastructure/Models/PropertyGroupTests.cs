using System;
using System.Diagnostics;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Common.Builders.Extensions;

namespace Umbraco.Tests.UnitTests.Umbraco.Infrastructure.Models
{
    [TestFixture]
    public class PropertyGroupTests
    {
        private readonly PropertyGroupBuilder _builder = new PropertyGroupBuilder();

        [Test]
        public void Can_Deep_Clone()
        {
            var pg = BuildPropertyGroup();

            var clone = (PropertyGroup)pg.DeepClone();

            Assert.AreNotSame(clone, pg);
            Assert.AreEqual(clone, pg);
            Assert.AreEqual(clone.Id, pg.Id);
            Assert.AreEqual(clone.CreateDate, pg.CreateDate);
            Assert.AreEqual(clone.Key, pg.Key);
            Assert.AreEqual(clone.Name, pg.Name);
            Assert.AreEqual(clone.SortOrder, pg.SortOrder);
            Assert.AreEqual(clone.UpdateDate, pg.UpdateDate);
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
            var pg = BuildPropertyGroup();

            var json = JsonConvert.SerializeObject(pg);
            Debug.Print(json);
        }

        private PropertyGroup BuildPropertyGroup()
        {
            return _builder
                .WithId(77)
                .WithCreateDate(DateTime.Now)
                .WithName("Group1")
                .WithSortOrder(555)
                .WithKey(Guid.NewGuid())
                .WithUpdateDate(DateTime.Now)
                .AddPropertyType()
                    .WithId(3)
                    .WithPropertyEditorAlias("TestPropertyEditor")
                    .WithValueStorageType(ValueStorageType.Nvarchar)
                    .WithAlias("test")
                    .WithName("Test")
                    .WithSortOrder(9)
                    .WithDataTypeId(5)
                    .WithCreateDate(DateTime.Now)
                    .WithUpdateDate(DateTime.Now)
                    .WithDescription("testing")
                    .WithKey(Guid.NewGuid())
                    .WithPropertyGroupId(11)
                    .WithMandatory(true)                    
                    .WithValidationRegExp("xxxx")
                    .Done()
                .AddPropertyType()
                    .WithId(4)
                    .WithPropertyEditorAlias("TestPropertyEditor2")
                    .WithValueStorageType(ValueStorageType.Nvarchar)
                    .WithAlias("test2")
                    .WithName("Test2")
                    .WithSortOrder(10)
                    .WithDataTypeId(6)
                    .WithCreateDate(DateTime.Now)
                    .WithUpdateDate(DateTime.Now)
                    .WithDescription("testing2")
                    .WithKey(Guid.NewGuid())
                    .WithPropertyGroupId(12)
                    .WithMandatory(true)                    
                    .WithValidationRegExp("yyyy")
                    .Done()
                .Build();
        }
    }
}
