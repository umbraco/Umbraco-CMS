using System;
using System.Diagnostics;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class DataTypeTests
    {
        [Test]
        public void Can_Deep_Clone()
        {
            var dtd = new DataType(new VoidEditor(Mock.Of<ILogger>()), 9)
            {
                CreateDate = DateTime.Now,
                CreatorId = 5,
                DatabaseType = ValueStorageType.Nvarchar,
                Id = 4,
                Key = Guid.NewGuid(),
                Level = 7,
                Name = "Test",
                ParentId = 9,
                Path = "-1,2",
                SortOrder = 8,
                Trashed = true,
                UpdateDate = DateTime.Now
            };
            var clone = (DataType) dtd.DeepClone();

            Assert.AreNotSame(clone, dtd);
            Assert.AreEqual(clone, dtd);
            Assert.AreEqual(clone.CreateDate, dtd.CreateDate);
            Assert.AreEqual(clone.CreatorId, dtd.CreatorId);
            Assert.AreEqual(clone.DatabaseType, dtd.DatabaseType);
            Assert.AreEqual(clone.Id, dtd.Id);
            Assert.AreEqual(clone.Key, dtd.Key);
            Assert.AreEqual(clone.Level, dtd.Level);
            Assert.AreEqual(clone.Name, dtd.Name);
            Assert.AreEqual(clone.ParentId, dtd.ParentId);
            Assert.AreEqual(clone.Path, dtd.Path);
            Assert.AreEqual(clone.SortOrder, dtd.SortOrder);
            Assert.AreEqual(clone.Trashed, dtd.Trashed);
            Assert.AreEqual(clone.UpdateDate, dtd.UpdateDate);

            //This double verifies by reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(dtd, null));
            }
        }

        [Test]
        public void Can_Serialize_Without_Error()
        {
            var ss = new SerializationService(new JsonNetSerializer());

            var dtd = new DataType(new VoidEditor(Mock.Of<ILogger>()), 9)
            {
                CreateDate = DateTime.Now,
                CreatorId = 5,
                DatabaseType = ValueStorageType.Nvarchar,
                Id = 4,
                Key = Guid.NewGuid(),
                Level = 7,
                Name = "Test",
                ParentId = 9,
                Path = "-1,2",
                SortOrder = 8,
                Trashed = true,
                UpdateDate = DateTime.Now
            };

            var result = ss.ToStream(dtd);
            var json = result.ResultStream.ToJsonString();
            Debug.Print(json);
        }

    }
}
