using System.Diagnostics;
using System.Globalization;
using Examine;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Examine.Lucene.UmbracoExamine.ContentIndexRebuild;

/// <summary>
///     Measures rebuilding a content index from multilingual content with nested block editor values.
/// </summary>
/// <remarks>
///     <para>
///         Run on demand, for example:
///         <c>dotnet test tests/Umbraco.Tests.Integration -c Release --filter "FullyQualifiedName~ContentIndexRebuildPerformanceTests"</c>.
///         Set <c>Tests__Database__DatabaseType=LocalDb</c> to measure against SQL Server.
///     </para>
///     <para>
///         The rebuild is measured in two phases per batch: loading the documents (<c>IContentService.GetPagedDescendants</c>)
///         and building their value sets (<c>IValueSetBuilder.GetValueSets</c>), materialised as a remote index provider would.
///         Results are written to the test output and appended to <c>content-index-rebuild-metrics.csv</c> in the directory
///         given by the <c>UMBRACO_PERF_OUTPUT</c> environment variable, or the test work directory.
///     </para>
/// </remarks>
[TestFixture]
[Explicit("Performance measurement, run on demand.")]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ContentIndexRebuildPerformanceTests : ContentIndexRebuildTestBase
{
    private const int MeasuredRuns = 3;

    private static readonly int[] _batchSizes = [10, 50, 1000];

    private ICoreScopeProvider CoreScopeProvider => GetRequiredService<ICoreScopeProvider>();

    [TestCase(1, 200, 100)]
    [TestCase(5, 100, 100)]
    [TestCase(24, 40, 100)]
    [TestCase(24, 40, 1000)]
    public async Task Rebuild_Internal_Index(int cultureCount, int documentCount, int textLength)
    {
        int[] indexedCounts = await MeasureAsync(cultureCount, documentCount, textLength, publishedValuesOnly: false);
        Assert.That(indexedCounts, Has.All.EqualTo(documentCount));
    }

    [TestCase(24, 40, 100)]
    public async Task Rebuild_Published_Index(int cultureCount, int documentCount, int textLength)
    {
        // Every seeded document is published.
        int[] indexedCounts = await MeasureAsync(cultureCount, documentCount, textLength, publishedValuesOnly: true);
        Assert.That(indexedCounts, Has.All.EqualTo(documentCount));
    }

    /// <summary>
    ///     Seeds content and measures rebuilding an index from it at each batch size.
    /// </summary>
    /// <returns>The number of documents each rebuild indexed, by batch size.</returns>
    private async Task<int[]> MeasureAsync(int cultureCount, int documentCount, int textLength, bool publishedValuesOnly)
    {
        var indexedCounts = new List<int>();
        var seeding = Stopwatch.StartNew();
        await SeedAsync(cultureCount, documentCount, textLength);
        TestContext.Out.WriteLine($"Seeded {documentCount} documents in {cultureCount} cultures in {seeding.Elapsed.TotalSeconds:F1}s.");

        IValueSetBuilder<IContent> valueSetBuilder = publishedValuesOnly
            ? GetRequiredService<IPublishedContentValueSetBuilder>()
            : GetRequiredService<IContentValueSetBuilder>();

        foreach (var batchSize in _batchSizes)
        {
            // Warm up caches and JIT before measuring.
            RunPhases(valueSetBuilder, batchSize, publishedValuesOnly, measureMemory: false);

            RebuildMetrics best = Enumerable.Range(0, MeasuredRuns)
                .Select(_ => RunPhases(valueSetBuilder, batchSize, publishedValuesOnly, measureMemory: false))
                .MinBy(x => x.LoadTime + x.BuildTime)!;
            RebuildMetrics memory = RunPhases(valueSetBuilder, batchSize, publishedValuesOnly, measureMemory: true);

            CapturedIndex captured = null!;
            TimeSpan populate = Enumerable.Range(0, MeasuredRuns)
                .Select(_ =>
                {
                    var stopwatch = Stopwatch.StartNew();
                    captured = Rebuild(publishedValuesOnly, batchSize);
                    return stopwatch.Elapsed;
                })
                .Min();

            Report(new MetricsRow(
                publishedValuesOnly ? "published" : "internal",
                cultureCount,
                documentCount,
                textLength,
                batchSize,
                captured.ValueSets.Count(),
                best.FieldCount,
                best.LoadTime,
                best.BuildTime,
                populate,
                best.LoadAllocatedBytes,
                best.BuildAllocatedBytes,
                memory.PeakRetainedBytes));
            indexedCounts.Add(captured.ValueSets.Count());
        }

        return indexedCounts.ToArray();
    }

    /// <summary>
    ///     Pages through all content as <see cref="ContentIndexPopulator" /> does, timing the load and the value set build separately.
    /// </summary>
    private RebuildMetrics RunPhases(IValueSetBuilder<IContent> valueSetBuilder, int batchSize, bool publishedValuesOnly, bool measureMemory)
    {
        IQuery<IContent> publishedQuery = GetRequiredService<IUmbracoDatabaseFactory>().SqlContext.Query<IContent>().Where(x => x.Published);
        var metrics = new RebuildMetrics();
        var pageIndex = 0;
        IContent[] content;

        do
        {
            var allocatedBefore = GC.GetTotalAllocatedBytes(true);
            var load = Stopwatch.StartNew();
            content = publishedValuesOnly
                ? ContentService.GetPagedDescendants(-1, pageIndex, batchSize, out _, publishedQuery, Ordering.By("Path")).ToArray()
                : ContentService.GetPagedDescendants(-1, pageIndex, batchSize, out _).ToArray();
            load.Stop();
            var allocatedAfterLoad = GC.GetTotalAllocatedBytes(true);

            var build = Stopwatch.StartNew();
            List<ValueSet> valueSets = valueSetBuilder.GetValueSets(content).ToList();
            build.Stop();
            var allocatedAfterBuild = GC.GetTotalAllocatedBytes(true);

            metrics.LoadTime += load.Elapsed;
            metrics.BuildTime += build.Elapsed;
            metrics.LoadAllocatedBytes += allocatedAfterLoad - allocatedBefore;
            metrics.BuildAllocatedBytes += allocatedAfterBuild - allocatedAfterLoad;
            metrics.FieldCount += valueSets.Sum(x => x.Values.Count);

            if (measureMemory)
            {
                // Both the loaded page and its value sets are reachable here, as they are while a batch is handed to an index.
                metrics.PeakRetainedBytes = Math.Max(metrics.PeakRetainedBytes, GC.GetTotalMemory(forceFullCollection: true));
                GC.KeepAlive(content);
                GC.KeepAlive(valueSets);
            }

            pageIndex++;
        }
        while (content.Length == batchSize);

        return metrics;
    }

    /// <summary>
    ///     Seeds alternating culture variant and invariant documents. Variant documents carry culture variant and block level
    ///     variant blocks nested three levels deep, and a rich text with blocks, and have a draft that differs from the
    ///     published version. Invariant documents carry nested blocks and a block grid.
    /// </summary>
    private async Task SeedAsync(int cultureCount, int documentCount, int textLength)
    {
        string[] cultures = await CreateLanguagesAsync(cultureCount);
        IndexingSchema schema = await CreateSchemaAsync();

        // The rebuild reads from the database only, so the caches and URLs that notification handlers maintain aren't needed.
        using ICoreScope scope = CoreScopeProvider.CreateCoreScope();
        using IDisposable suppressNotifications = scope.Notifications.Suppress();
        for (var i = 0; i < documentCount; i++)
        {
            var name = $"Document {i:D5}";
            if (i % 2 == 0)
            {
                CreateVariantDocument(
                    schema,
                    name,
                    cultures,
                    content =>
                    {
                        foreach (var culture in cultures)
                        {
                            content.SetValue("title", Text($"{name} title {culture}", textLength), culture);
                            content.SetValue("variantBlocks", BlockListJson(schema, $"{name} variantBlocks {culture}", new BlockSpec(3, 2, null, textLength)), culture);
                            content.SetValue("body", RichTextJson(schema, $"{name} body {culture}", new BlockSpec(1, 2, null, textLength)), culture);
                        }

                        content.SetValue("invariantBlocks", BlockListJson(schema, $"{name} invariantBlocks", new BlockSpec(3, 2, cultures, textLength)));
                    },
                    culturesToPublish: cultures,
                    setDraftValues: content => content.SetValue("title", Text($"{name} title draft", textLength), cultures[0]));
            }
            else
            {
                CreateInvariantDocument(
                    schema,
                    name,
                    content =>
                    {
                        content.SetValue("title", Text($"{name} title", textLength));
                        content.SetValue("invariantBlocks", BlockListJson(schema, $"{name} invariantBlocks", new BlockSpec(3, 2, null, textLength)));
                        content.SetValue("grid", BlockGridJson(schema, $"{name} grid", new BlockSpec(1, 2, null, textLength)));
                    },
                    publish: true);
            }
        }

        scope.Complete();
    }

    private static void Report(MetricsRow row)
    {
        TestContext.Out.WriteLine(row.ToString());

        var directory = Environment.GetEnvironmentVariable("UMBRACO_PERF_OUTPUT") ?? TestContext.CurrentContext.WorkDirectory;
        var path = Path.Combine(directory, "content-index-rebuild-metrics.csv");
        if (System.IO.File.Exists(path) is false)
        {
            System.IO.File.WriteAllText(path, MetricsRow.CsvHeader + Environment.NewLine);
        }

        System.IO.File.AppendAllText(path, row.ToCsv() + Environment.NewLine);
    }

    private sealed class RebuildMetrics
    {
        public TimeSpan LoadTime { get; set; }

        public TimeSpan BuildTime { get; set; }

        public long LoadAllocatedBytes { get; set; }

        public long BuildAllocatedBytes { get; set; }

        public long PeakRetainedBytes { get; set; }

        public long FieldCount { get; set; }
    }

    private sealed record MetricsRow(
        string Index,
        int Cultures,
        int Documents,
        int TextLength,
        int BatchSize,
        int ValueSets,
        long Fields,
        TimeSpan Load,
        TimeSpan Build,
        TimeSpan Populate,
        long LoadAllocated,
        long BuildAllocated,
        long PeakRetained)
    {
        public const string CsvHeader =
            "timestamp,index,cultures,documents,textLength,batchSize,valueSets,fields,loadMs,buildMs,populateMs,loadAllocatedMB,buildAllocatedMB,peakRetainedMB,loadMsPerDocument,buildMsPerDocument,buildAllocatedMBPerDocument";

        public string ToCsv() => string.Join(
            ',',
            DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            Index,
            Cultures,
            Documents,
            TextLength,
            BatchSize,
            ValueSets,
            Fields,
            Load.TotalMilliseconds.ToString("F0", CultureInfo.InvariantCulture),
            Build.TotalMilliseconds.ToString("F0", CultureInfo.InvariantCulture),
            Populate.TotalMilliseconds.ToString("F0", CultureInfo.InvariantCulture),
            Megabytes(LoadAllocated),
            Megabytes(BuildAllocated),
            Megabytes(PeakRetained),
            PerDocument(Load.TotalMilliseconds),
            PerDocument(Build.TotalMilliseconds),
            PerDocument(BuildAllocated / 1024d / 1024d));

        public override string ToString()
            => $"{Index} index, {Cultures} cultures, {Documents} documents, text {TextLength}, batch {BatchSize}: " +
               $"{ValueSets} value sets, {Fields} fields; load {Load.TotalMilliseconds:F0} ms ({Megabytes(LoadAllocated)} MB allocated), " +
               $"build {Build.TotalMilliseconds:F0} ms ({Megabytes(BuildAllocated)} MB allocated), populate {Populate.TotalMilliseconds:F0} ms, " +
               $"peak retained {Megabytes(PeakRetained)} MB";

        private string PerDocument(double value) => (value / Documents).ToString("F2", CultureInfo.InvariantCulture);

        private static string Megabytes(long bytes) => (bytes / 1024d / 1024d).ToString("F1", CultureInfo.InvariantCulture);
    }
}
