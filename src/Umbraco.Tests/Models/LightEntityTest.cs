using System;
using System.Diagnostics;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Serialization;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class LightEntityTest
    {
        [Test]
        public void Can_Serialize_Without_Error()
        {
            var ss = new SerializationService(new JsonNetSerializer());

            var item = new DocumentEntitySlim()
            {
                Id = 3,
                ContentTypeAlias = "test1",
                CreatorId = 4,
                Key = Guid.NewGuid(),
                UpdateDate = DateTime.Now,
                CreateDate = DateTime.Now,
                Name = "Test",
                ParentId = 5,
                SortOrder = 6,
                Path = "-1,23",
                Level = 7,
                ContentTypeIcon = "icon",
                ContentTypeThumbnail = "thumb",
                HasChildren = true,
                Edited = true,
                Published = true,
                NodeObjectType = Guid.NewGuid()
            };
            item.AdditionalData.Add("test1", 3);
            item.AdditionalData.Add("test2", "valuie");
            item.AdditionalData.Add("test3", new EntitySlim.PropertySlim("TestPropertyEditor", "test"));
            item.AdditionalData.Add("test4", new EntitySlim.PropertySlim("TestPropertyEditor2", "test2"));

            var result = ss.ToStream(item);
            var json = result.ResultStream.ToJsonString();
            Debug.Print(json); // FIXME: compare with v7
        }
    }
}
