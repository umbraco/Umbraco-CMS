using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Tests.Shared.Builders;

namespace Umbraco.Tests.UnitTests.Umbraco.Infrastructure.Models
{
    [TestFixture]
    public class DictionaryItemTests
    {
        private readonly DictionaryItemBuilder _builder = new DictionaryItemBuilder();

        [Test]
        public void Can_Deep_Clone()
        {
            var item = _builder
                    .AddTranslation()
                        .WithValue("Colour")
                    .Done()
                    .AddTranslation()
                        .WithValue("Color")
                    .Done()
                .Build();

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
            var item = _builder
                .AddTranslation()
                .WithValue("Colour")
                .Done()
                .AddTranslation()
                .WithValue("Color")
                .Done()
                .Build();

            Assert.DoesNotThrow(() => JsonConvert.SerializeObject(item));
        }
    }
}
