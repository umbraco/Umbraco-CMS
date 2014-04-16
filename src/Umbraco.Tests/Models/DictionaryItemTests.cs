using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Serialization;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class DictionaryItemTests
    {
        [Test]
        public void Can_Deep_Clone()
        {
            var item = new DictionaryItem("blah")
            {
                CreateDate = DateTime.Now,
                Id = 8,
                ItemKey = "blah",
                Key = Guid.NewGuid(),
                ParentId = Guid.NewGuid(),
                UpdateDate = DateTime.Now,
                Translations = new[]
                {
                    new DictionaryTranslation(new Language("en-AU")
                    {
                        CreateDate = DateTime.Now,
                        CultureName = "en",
                        Id = 11,
                        IsoCode = "AU",
                        Key = Guid.NewGuid(),
                        UpdateDate = DateTime.Now
                    }, "colour")
                    {
                        CreateDate = DateTime.Now,
                        Id = 88,
                        Key = Guid.NewGuid(),
                        UpdateDate = DateTime.Now
                    },
                    new DictionaryTranslation(new Language("en-US")
                    {
                        CreateDate = DateTime.Now,
                        CultureName = "en",
                        Id = 12,
                        IsoCode = "US",
                        Key = Guid.NewGuid(),
                        UpdateDate = DateTime.Now
                    }, "color")
                    {
                        CreateDate = DateTime.Now,
                        Id = 89,
                        Key = Guid.NewGuid(),
                        UpdateDate = DateTime.Now
                    },
                }
            };

            var clone = (DictionaryItem)item.DeepClone();

            Assert.AreNotSame(clone, item);
            Assert.AreEqual(clone, item);
            Assert.AreEqual(clone.CreateDate, item.CreateDate);
            Assert.AreEqual(clone.Id, item.Id);
            Assert.AreEqual(clone.ItemKey, item.ItemKey);
            Assert.AreEqual(clone.Key, item.Key);
            Assert.AreEqual(clone.ParentId, item.ParentId);
            Assert.AreEqual(clone.UpdateDate, item.UpdateDate);
            Assert.AreEqual(clone.Translations.Count(), item.Translations.Count());
            for (var i = 0; i < item.Translations.Count(); i++)
            {
                Assert.AreNotSame(clone.Translations.ElementAt(i), item.Translations.ElementAt(i));
                Assert.AreEqual(clone.Translations.ElementAt(i), item.Translations.ElementAt(i));
            }

            //This double verifies by reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(item, null));
            }
        }

        [Test]
        public void Can_Serialize_Without_Error()
        {
            var ss = new SerializationService(new JsonNetSerializer());

            var item = new DictionaryItem("blah")
            {
                CreateDate = DateTime.Now,
                Id = 8,
                ItemKey = "blah",
                Key = Guid.NewGuid(),
                ParentId = Guid.NewGuid(),
                UpdateDate = DateTime.Now,
                Translations = new[]
                {
                    new DictionaryTranslation(new Language("en-AU")
                    {
                        CreateDate = DateTime.Now,
                        CultureName = "en",
                        Id = 11,
                        IsoCode = "AU",
                        Key = Guid.NewGuid(),
                        UpdateDate = DateTime.Now
                    }, "colour")
                    {
                        CreateDate = DateTime.Now,
                        Id = 88,
                        Key = Guid.NewGuid(),
                        UpdateDate = DateTime.Now
                    },
                    new DictionaryTranslation(new Language("en-US")
                    {
                        CreateDate = DateTime.Now,
                        CultureName = "en",
                        Id = 12,
                        IsoCode = "US",
                        Key = Guid.NewGuid(),
                        UpdateDate = DateTime.Now
                    }, "color")
                    {
                        CreateDate = DateTime.Now,
                        Id = 89,
                        Key = Guid.NewGuid(),
                        UpdateDate = DateTime.Now
                    },
                }
            };

            var result = ss.ToStream(item);
            var json = result.ResultStream.ToJsonString();
            Console.WriteLine(json);
        }
    }
}