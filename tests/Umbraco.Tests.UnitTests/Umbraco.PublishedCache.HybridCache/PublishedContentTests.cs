using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// Tests for <see cref="PublishedContent"/>, focused on the lazy thread-safe property
/// initialization.
/// </summary>
/// <remarks>
/// Functional correctness of <c>Properties</c> / <c>GetProperty</c> is already covered by integration tests
/// through <c>PublishedContentFactory</c>; this fixture verifies the lazy behaviour and thread-safety contract
/// introduced by the move from eager construction to <c>Interlocked.CompareExchange</c>-guarded lazy build.
/// </remarks>
[TestFixture]
public class PublishedContentTests
{
    [Test]
    public void Properties_PropertyCountMatchesContentType()
    {
        PublishedContent content = CreatePublishedContent(propertyCount: 5);

        Assert.That(content.Properties.Count(), Is.EqualTo(5));
    }

    [Test]
    public void Properties_AliasesMatchContentTypePropertyTypes()
    {
        PublishedContent content = CreatePublishedContent(propertyCount: 3);

        var aliases = content.Properties.Select(p => p.Alias).ToArray();

        Assert.That(aliases, Is.EqualTo(new[] { "prop0", "prop1", "prop2" }));
    }

    [Test]
    public void GetProperty_KnownAlias_ReturnsPropertyWithSameAlias()
    {
        PublishedContent content = CreatePublishedContent(propertyCount: 3);

        IPublishedProperty? property = content.GetProperty("prop1");

        Assert.That(property, Is.Not.Null);
        Assert.That(property!.Alias, Is.EqualTo("prop1"));
    }

    [Test]
    public void GetProperty_UnknownAlias_ReturnsNull()
    {
        PublishedContent content = CreatePublishedContent(propertyCount: 3);

        Assert.That(content.GetProperty("nonexistent"), Is.Null);
    }

    [Test]
    public void GetProperty_RepeatedCalls_ReturnSamePropertyInstance()
    {
        PublishedContent content = CreatePublishedContent(propertyCount: 3);

        IPublishedProperty? first = content.GetProperty("prop0");
        IPublishedProperty? second = content.GetProperty("prop0");

        // The lazy-init contract: once the property array is built, every caller sees the
        // same canonical PublishedProperty instance.
        Assert.That(second, Is.SameAs(first));
    }

    [Test]
    public void Properties_ConcurrentFirstAccess_AllThreadsSeeSameInstances()
    {
        // The race we're guarding against: multiple threads enter EnsureProperties() before
        // any has finished BuildProperties(). Without Interlocked.CompareExchange they could
        // each store a different array, and a property looked up via thread A could be a
        // different instance than the same property looked up via thread B — value-cache
        // state on the property would diverge as a result.
        const int threadCount = 32;
        PublishedContent content = CreatePublishedContent(propertyCount: 5);

        var observed = new IPublishedProperty[threadCount][];
        var startGate = new ManualResetEventSlim(false);
        var threads = new Thread[threadCount];

        for (var t = 0; t < threadCount; t++)
        {
            var localT = t;
            threads[t] = new Thread(() =>
            {
                startGate.Wait();
                observed[localT] = content.Properties.ToArray();
            });
            threads[t].Start();
        }

        startGate.Set();
        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        IPublishedProperty[] reference = observed[0];
        for (var i = 1; i < threadCount; i++)
        {
            Assert.That(observed[i], Has.Length.EqualTo(reference.Length));
            for (var j = 0; j < reference.Length; j++)
            {
                // Reference equality: every thread observes the same canonical PublishedProperty
                // instances, never a duplicate from a CompareExchange loser.
                Assert.That(observed[i][j], Is.SameAs(reference[j]));
            }
        }
    }

    private static PublishedContent CreatePublishedContent(int propertyCount)
    {
        IPublishedModelFactory modelFactory = new NoopPublishedModelFactory();
        var converters = new PropertyValueConverterCollection(() => Enumerable.Empty<IPropertyValueConverter>());

        var jsonSerializer = new SystemTextConfigurationEditorJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        var dataType = new DataType(new VoidEditor(Mock.Of<IDataValueEditorFactory>()), jsonSerializer) { Id = 1 };
        var dataTypeServiceMock = new Mock<IDataTypeService>();

        dataTypeServiceMock.Setup(x => x.GetAllAsync(It.IsAny<Guid[]>())).ReturnsAsync(new[] { dataType });

        var typeFactory = new PublishedContentTypeFactory(modelFactory, converters, dataTypeServiceMock.Object, Mock.Of<IIdKeyMap>());

        IEnumerable<IPublishedPropertyType> CreatePropertyTypes(IPublishedContentType ct)
        {
            for (var i = 0; i < propertyCount; i++)
            {
                yield return typeFactory.CreatePropertyType(ct, $"prop{i}", dataType.Id, ContentVariation.Nothing);
            }
        }

        IPublishedContentType contentType = new PublishedContentType(
            Guid.NewGuid(),
            1000,
            "test",
            PublishedItemType.Content,
            Enumerable.Empty<string>(),
            CreatePropertyTypes,
            ContentVariation.Nothing);

        var properties = new Dictionary<string, PropertyData[]>();
        for (var i = 0; i < propertyCount; i++)
        {
            properties[$"prop{i}"] =
            [
                new PropertyData
                {
                    Culture = string.Empty,
                    Segment = string.Empty,
                    Value = $"value-{i}",
                },
            ];
        }

        var contentData = new ContentData(
            "Test",
            null,
            1,
            DateTime.UtcNow,
            -1,
            0,
            true,
            properties,
            null);

        var contentNode = new ContentNode(
            1,
            Guid.NewGuid(),
            0,
            DateTime.UtcNow,
            -1,
            contentType,
            draftData: null,
            publishedData: contentData);

        return new PublishedContent(
            contentNode,
            preview: false,
            new ElementsDictionaryAppCache(),
            new ThreadCultureVariationContextAccessor(),
            Mock.Of<IPropertyRenderingContextAccessor>());
    }
}
