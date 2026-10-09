// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Concurrent;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks
{
    [QuickRunWithMemoryDiagnoserConfig]
    public class MediaCropsBenchmark
    {
        private sealed class StubPublishedContent : IPublishedContent
        {
            public int Id => 1;
            public string Name => "Test";
            public string? UrlSegment => "test";
            public int SortOrder => 0;
            public int Level => 1;
            public string Path => "-1,1";
            public int? TemplateId => null;
            public int CreatorId => 0;
            public DateTime CreateDate => DateTime.MinValue;
            public int WriterId => 0;
            public DateTime UpdateDate => DateTime.MinValue;
            public IReadOnlyDictionary<string, PublishedCultureInfo> Cultures => new Dictionary<string, PublishedCultureInfo>();
            public PublishedItemType ItemType => PublishedItemType.Media;

            [Obsolete("Use extension methods.")]
            public IPublishedContent? Parent => null;

            [Obsolete("Use extension methods.")]
            public IEnumerable<IPublishedContent> Children => Enumerable.Empty<IPublishedContent>();

            public bool IsDraft(string? culture = null) => false;
            public bool IsPublished(string? culture = null) => true;

            public IPublishedContentType ContentType => null!;
            public Guid Key => Guid.Empty;
            public IEnumerable<IPublishedProperty> Properties => Enumerable.Empty<IPublishedProperty>();
            public IPublishedProperty? GetProperty(string alias) => null;
        }

        private sealed class StubPublishedValueFallback : IPublishedValueFallback
        {
            public bool TryGetValue(IPublishedProperty property, string? culture, string? segment, Fallback fallback, object? defaultValue, out object? value)
            { value = defaultValue; return false; }

            public bool TryGetValue<T>(IPublishedProperty property, string? culture, string? segment, Fallback fallback, T? defaultValue, out T? value)
            { value = defaultValue; return false; }

            public bool TryGetValue(IPublishedElement content, string alias, string? culture, string? segment, Fallback fallback, object? defaultValue, out object? value)
            { value = defaultValue; return false; }

            public bool TryGetValue<T>(IPublishedElement content, string alias, string? culture, string? segment, Fallback fallback, T? defaultValue, out T? value)
            { value = defaultValue; return false; }

            public bool TryGetValue(IPublishedContent content, string alias, string? culture, string? segment, Fallback fallback, object? defaultValue, out object? value, out IPublishedProperty? noValueProperty)
            { value = defaultValue; noValueProperty = null; return false; }

            public bool TryGetValue<T>(IPublishedContent content, string alias, string? culture, string? segment, Fallback fallback, T defaultValue, out T? value, out IPublishedProperty? noValueProperty)
            { value = defaultValue; noValueProperty = null; return false; }
        }

        // -------------------------------------------------------------------------
        // Shared state
        // -------------------------------------------------------------------------

        private static readonly IPublishedContent MediaItem = new StubPublishedContent();
        private static readonly IPublishedValueFallback Fallback = new StubPublishedValueFallback();
        private static readonly ImageCropperValue LocalCrops = new() { Src = "/media/test.jpg" };
        private static readonly IPublishedContent[] TenMediaItems = Enumerable.Range(0, 10).Select(_ => new StubPublishedContent()).ToArray();

        private static readonly ConcurrentDictionary<Type, ConstructorInvoker> _factories = new();

        // -------------------------------------------------------------------------
        // After: compiled Expression delegate (production code path)
        // -------------------------------------------------------------------------

        [Benchmark(Baseline = true, Description = "After: single item (compiled delegate, warm)")]
        public MediaWithCrops After_Single() =>
            CreateMediaWithCropsNew(_factories, MediaItem, Fallback, LocalCrops);

        [Benchmark(Description = "After: ten items (compiled delegate, warm)")]
        public MediaWithCrops After_Ten()
        {
            MediaWithCrops last = null!;
            foreach (IPublishedContent item in TenMediaItems)
            {
                last = CreateMediaWithCropsNew(_factories, item, Fallback, LocalCrops);
            }
            return last;
        }

        // -------------------------------------------------------------------------
        // Before: Activator.CreateInstance (original code path)
        // -------------------------------------------------------------------------

        [Benchmark(Description = "Before: single item (Activator.CreateInstance)")]
        public MediaWithCrops Before_Single() => CreateMediaWithCropsOld(MediaItem, Fallback, LocalCrops);

        [Benchmark(Description = "Before: ten items (Activator.CreateInstance)")]
        public MediaWithCrops Before_Ten()
        {
            MediaWithCrops last = null!;
            foreach (IPublishedContent item in TenMediaItems)
            {
                last = CreateMediaWithCropsOld(item, Fallback, LocalCrops);
            }
            return last;
        }

        // -------------------------------------------------------------------------
        // Old implementation
        // -------------------------------------------------------------------------

        private static MediaWithCrops CreateMediaWithCropsOld(
            IPublishedContent mediaItem,
            IPublishedValueFallback publishedValueFallback,
            ImageCropperValue localCrops)
        {
            Type mediaType = mediaItem.GetType();
            Type closedType = typeof(MediaWithCrops<>).MakeGenericType(mediaType);
            return (MediaWithCrops)Activator.CreateInstance(closedType, mediaItem, publishedValueFallback, localCrops)!;
        }

        // -------------------------------------------------------------------------
        // New implementation
        // -------------------------------------------------------------------------

        private static MediaWithCrops CreateMediaWithCropsNew(
            ConcurrentDictionary<Type, ConstructorInvoker> factories,
            IPublishedContent mediaItem,
            IPublishedValueFallback publishedValueFallback,
            ImageCropperValue localCrops)
        {
            ConstructorInvoker factory = factories.GetOrAdd(mediaItem.GetType(), static mediaType => CompileFactory(mediaType));
            return (MediaWithCrops)factory.Invoke(mediaItem, publishedValueFallback, localCrops);
        }

        private static ConstructorInvoker CompileFactory(Type mediaType)
        {
            Type closedType = typeof(MediaWithCrops<>).MakeGenericType(mediaType);
            ConstructorInfo ctor = closedType.GetConstructor(
                [mediaType, typeof(IPublishedValueFallback), typeof(ImageCropperValue)])!;
            return ConstructorInvoker.Create(ctor);
        }
    }
}
