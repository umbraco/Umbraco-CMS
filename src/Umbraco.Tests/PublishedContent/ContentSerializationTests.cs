﻿using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

namespace Umbraco.Tests.PublishedContent
{
    [TestFixture]
    public class ContentSerializationTests
    {
        [Test]
        public void GivenACacheModel_WhenItsSerializedAndDeserializedWithAnySerializer_TheResultsAreTheSame()
        {
            var jsonSerializer = new JsonContentNestedDataSerializer();
            var msgPackSerializer = new MsgPackContentNestedDataSerializer(Mock.Of<IPropertyCacheCompression>());

            var now = DateTime.Now;
            var cacheModel = new ContentCacheDataModel
            {
                PropertyData = new Dictionary<string, PropertyData[]>
                {
                    ["propertyOne"] = new[]
                    {
                        new PropertyData
                        {
                            Culture = "en-US",
                            Segment = "test",
                            Value = "hello world"
                        }
                    },
                    ["propertyTwo"] = new[]
                    {
                        new PropertyData
                        {
                            Culture = "en-US",
                            Segment = "test",
                            Value = "Lorem ipsum"
                        }
                    }
                },
                CultureData = new Dictionary<string, CultureVariation>
                {
                    ["en-US"] = new CultureVariation
                    {
                        Date = now,
                        IsDraft = false,
                        Name = "Home",
                        UrlSegment = "home"
                    }
                },
                UrlSegment = "home"
            };

            var content = Mock.Of<IReadOnlyContentBase>(x => x.ContentTypeId == 1);

            var json = jsonSerializer.Serialize(content, cacheModel).StringData;
            var msgPack = msgPackSerializer.Serialize(content, cacheModel).ByteData;

            Console.WriteLine(json);
            Console.WriteLine(msgPackSerializer.ToJson(msgPack));

            var jsonContent = jsonSerializer.Deserialize(content, json, null);
            var msgPackContent = msgPackSerializer.Deserialize(content, null, msgPack);


            CollectionAssert.AreEqual(jsonContent.CultureData.Keys, msgPackContent.CultureData.Keys);
            CollectionAssert.AreEqual(jsonContent.PropertyData.Keys, msgPackContent.PropertyData.Keys);
            CollectionAssert.AreEqual(jsonContent.CultureData.Values, msgPackContent.CultureData.Values, new CultureVariationComparer());
            CollectionAssert.AreEqual(jsonContent.PropertyData.Values, msgPackContent.PropertyData.Values, new PropertyDataComparer());
            Assert.AreEqual(jsonContent.UrlSegment, msgPackContent.UrlSegment);
        }

        public class CultureVariationComparer : Comparer<CultureVariation>
        {
            public override int Compare(CultureVariation x, CultureVariation y)
            {
                if (x == null && y == null) return 0;
                if (x == null && y != null) return -1;
                if (x != null && y == null) return 1;

                return x.Date.CompareTo(y.Date) | x.IsDraft.CompareTo(y.IsDraft) | x.Name.CompareTo(y.Name) | x.UrlSegment.CompareTo(y.UrlSegment);
            }
        }

        public class PropertyDataComparer : Comparer<PropertyData>
        {
            public override int Compare(PropertyData x, PropertyData y)
            {
                if (x == null && y == null) return 0;
                if (x == null && y != null) return -1;
                if (x != null && y == null) return 1;

                var xVal = x.Value?.ToString() ?? string.Empty;
                var yVal = y.Value?.ToString() ?? string.Empty;

                return x.Culture.CompareTo(y.Culture) | x.Segment.CompareTo(y.Segment) | xVal.CompareTo(yVal);
            }
        }

    }
}
