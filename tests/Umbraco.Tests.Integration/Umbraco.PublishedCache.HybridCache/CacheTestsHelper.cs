using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

internal static class CacheTestsHelper
{
    internal static void AssertPage(IContent baseContent, IPublishedContent? comparisonContent, bool isPublished = true)
    {
        Assert.Multiple(() =>
        {
            Assert.That(comparisonContent, Is.Not.Null);
            if (baseContent.ContentType.VariesByCulture())
            {
                foreach (var culture in baseContent.CultureInfos ?? Enumerable.Empty<ContentCultureInfos>())
                {
                    if (comparisonContent.Cultures.TryGetValue(culture.Culture, out var publishedCulture) is false)
                    {
                        continue;
                    }

                    Assert.That(publishedCulture.Name, Is.EqualTo(culture.Name));
                }
            }
            else
            {
                Assert.That(comparisonContent.Name, Is.EqualTo(baseContent.Name));
            }

            Assert.That(comparisonContent.IsPublished(), Is.EqualTo(isPublished));
        });

        AssertProperties(baseContent.Properties, comparisonContent!.Properties);
    }

    internal static void AssertProperties(IPropertyCollection propertyCollection,
        IEnumerable<IPublishedProperty> publishedProperties)
    {
        foreach (var prop in propertyCollection)
        {
            AssertProperty(prop, publishedProperties.First(x => x.Alias == prop.Alias));
        }
    }

    internal static void AssertProperty(IProperty property, IPublishedProperty publishedProperty)
    {
        Assert.Multiple(() =>
        {
            Assert.That(publishedProperty.Alias, Is.EqualTo(property.Alias));
            Assert.That(publishedProperty.PropertyType.Alias, Is.EqualTo(property.PropertyType.Alias));
        });
    }
}
