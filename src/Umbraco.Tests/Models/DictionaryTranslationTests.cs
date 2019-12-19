using System;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Serialization;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class DictionaryTranslationTests
    {
        private IGlobalSettings GlobalSettings { get; } = SettingsForTests.GenerateMockGlobalSettings();

        [Test]
        public void Can_Deep_Clone()
        {
            var item = new DictionaryTranslation(new Language(GlobalSettings, "en-AU")
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
            //This is null because we are ignoring it from cloning due to caching/cloning issues - we don't really want
            // this entity attached to this item but we're stuck with it for now
            Assert.IsNull(clone.Language);
            Assert.AreEqual(clone.LanguageId, item.LanguageId);
            Assert.AreEqual(clone.Value, item.Value);

            //This double verifies by reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps.Where(x => x.Name != "Language"))
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(item, null));
            }
        }

        [Test]
        public void Can_Serialize_Without_Error()
        {
            var item = new DictionaryTranslation(new Language(GlobalSettings, "en-AU")
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

            var json = JsonConvert.SerializeObject(item);
            Debug.Print(json);
        }
    }
}
