using System;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Serialization;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class DictionaryTranslationTests
    {
        [Test]
        public void Can_Deep_Clone()
        {
            var item = new DictionaryTranslation(new Language("en-AU")
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
            };

            var clone = (DictionaryTranslation) item.DeepClone();

            Assert.AreNotSame(clone, item);
            Assert.AreEqual(clone, item);
            Assert.AreEqual(clone.CreateDate, item.CreateDate);
            Assert.AreEqual(clone.Id, item.Id);
            Assert.AreEqual(clone.Key, item.Key);
            Assert.AreEqual(clone.UpdateDate, item.UpdateDate);
            Assert.AreNotSame(clone.Language, item.Language);
            Assert.AreEqual(clone.Language, item.Language);
            Assert.AreEqual(clone.Value, item.Value);

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

            var item = new DictionaryTranslation(new Language("en-AU")
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
            };

            var result = ss.ToStream(item);
            var json = result.ResultStream.ToJsonString();
            Console.WriteLine(json);
        }
    }
}