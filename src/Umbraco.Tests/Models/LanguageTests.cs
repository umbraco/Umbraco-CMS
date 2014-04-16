using System;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Serialization;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class LanguageTests
    {
        [Test]
        public void Can_Deep_Clone()
        {
            var item = new Language("en-AU")
            {
                CreateDate = DateTime.Now,
                CultureName = "AU",
                Id = 11,
                IsoCode = "en",
                Key = Guid.NewGuid(),
                UpdateDate = DateTime.Now               
            };

            var clone = (Language) item.DeepClone();
            Assert.AreNotSame(clone, item);
            Assert.AreEqual(clone, item);
            Assert.AreEqual(clone.CreateDate, item.CreateDate);
            Assert.AreEqual(clone.CultureName, item.CultureName);
            Assert.AreEqual(clone.Id, item.Id);
            Assert.AreEqual(clone.IsoCode, item.IsoCode);
            Assert.AreEqual(clone.Key, item.Key);
            Assert.AreEqual(clone.UpdateDate, item.UpdateDate);

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

            var item = new Language("en-AU")
            {
                CreateDate = DateTime.Now,
                CultureName = "AU",
                Id = 11,
                IsoCode = "en",
                Key = Guid.NewGuid(),
                UpdateDate = DateTime.Now
            };

            var result = ss.ToStream(item);
            var json = result.ResultStream.ToJsonString();
            Console.WriteLine(json);
        }
    }
}