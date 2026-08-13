using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using Microsoft.Data.Sqlite;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

namespace Umbraco.Tests.Benchmarks;

/// <summary>
/// Measures the cost of resolving a unique node name on create (<c>EnsureUniqueNodeName</c>) against a
/// flat tree, i.e. one parent with many direct children — the shape of a large media library.
/// </summary>
/// <remarks>
/// <para>
/// Models the two fetch strategies against a real SQLite database seeded with <see cref="SiblingCount"/>
/// siblings under a single parent, then feeds the result to the production
/// <see cref="SimilarNodeName.GetUniqueName(System.Collections.Generic.IEnumerable{SimilarNodeName}, int, string?)"/>:
/// <list type="bullet">
///   <item><see cref="FetchAllSiblings"/> — the original query that materialises every sibling.</item>
///   <item><see cref="FetchPrefixMatched"/> — the query narrowed to name-prefix matches (issue #23446).</item>
/// </list>
/// The prefix-matched query cost is expected to stay flat as <see cref="SiblingCount"/> grows, whereas the
/// all-siblings query scales linearly (rows transferred + objects allocated + in-memory scan).
/// </para>
/// <para>
/// Run with: <c>dotnet run -c Release --project tests/Umbraco.Tests.Benchmarks -- --filter "*MediaUniqueName*"</c>.
/// Uses <see cref="InProcessEmitToolchain"/> to avoid the per-benchmark MSBuild compile.
/// </para>
/// </remarks>
[Config(typeof(InProcessRunConfig))]
public class MediaUniqueNameBenchmarks
{
    private const int ParentId = Constants.System.Root;

    // A proposed name that does not collide with any seeded sibling — the overwhelmingly common case on
    // upload / folder-create, and the one where the all-siblings fetch does the most wasted work.
    private const string ProposedName = "a-brand-new-unique-name";

    private static readonly string MediaObjectType = Constants.ObjectTypes.Media.ToString();

    private SqliteConnection _connection = null!;

    private sealed class InProcessRunConfig : ManualConfig
    {
        public InProcessRunConfig()
        {
            AddJob(Job.Default
                .WithLaunchCount(1)
                .WithWarmupCount(3)
                .WithIterationCount(8)
                .WithToolchain(InProcessEmitToolchain.Instance));
            AddDiagnoser(MemoryDiagnoser.Default);
        }
    }

    [Params(1_000, 10_000, 30_000)]
    public int SiblingCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        using (SqliteCommand create = _connection.CreateCommand())
        {
            create.CommandText =
                """
                CREATE TABLE umbracoNode (
                    id INTEGER PRIMARY KEY,
                    parentId INTEGER NOT NULL,
                    nodeObjectType TEXT NOT NULL,
                    text TEXT NULL
                );
                CREATE INDEX IX_umbracoNode_parentId_nodeObjectType ON umbracoNode (parentId, nodeObjectType, text);
                """;
            create.ExecuteNonQuery();
        }

        using SqliteTransaction transaction = _connection.BeginTransaction();
        using (SqliteCommand insert = _connection.CreateCommand())
        {
            insert.Transaction = transaction;
            insert.CommandText =
                "INSERT INTO umbracoNode (id, parentId, nodeObjectType, text) VALUES (@id, @parentId, @objectType, @text)";
            SqliteParameter id = insert.Parameters.Add("@id", SqliteType.Integer);
            insert.Parameters.AddWithValue("@parentId", ParentId);
            insert.Parameters.AddWithValue("@objectType", MediaObjectType);
            SqliteParameter text = insert.Parameters.Add("@text", SqliteType.Text);

            for (var i = 0; i < SiblingCount; i++)
            {
                id.Value = i + 1;
                text.Value = $"media-item-{i:D6}";
                insert.ExecuteNonQuery();
            }
        }

        transaction.Commit();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _connection.Dispose();
        SqliteConnection.ClearAllPools();
    }

    [Benchmark(Baseline = true)]
    public string? FetchAllSiblings()
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText =
            "SELECT id, text FROM umbracoNode WHERE nodeObjectType = @objectType AND parentId = @parentId";
        command.Parameters.AddWithValue("@objectType", MediaObjectType);
        command.Parameters.AddWithValue("@parentId", ParentId);

        return SimilarNodeName.GetUniqueName(Read(command), 0, ProposedName);
    }

    [Benchmark]
    public string? FetchPrefixMatched()
    {
        var prefix = GetSafeLikePrefix(SimilarNodeName.GetBaseText(ProposedName));

        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText =
            "SELECT id, text FROM umbracoNode WHERE nodeObjectType = @objectType AND parentId = @parentId AND upper(text) LIKE upper(@prefix)";
        command.Parameters.AddWithValue("@objectType", MediaObjectType);
        command.Parameters.AddWithValue("@parentId", ParentId);
        command.Parameters.AddWithValue("@prefix", prefix + "%");

        return SimilarNodeName.GetUniqueName(Read(command), 0, ProposedName);
    }

    private static List<SimilarNodeName> Read(SqliteCommand command)
    {
        var siblings = new List<SimilarNodeName>();
        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            siblings.Add(new SimilarNodeName
            {
                Id = reader.GetInt32(0),
                Name = reader.IsDBNull(1) ? null : reader.GetString(1),
            });
        }

        return siblings;
    }

    // Mirrors ContentRepositoryBase.GetSafeLikePrefix so this benchmark builds the same prefix as
    // production; keep the two in sync if the truncation logic changes.
    private static string GetSafeLikePrefix(string baseText)
    {
        var index = baseText.IndexOf('[');
        return index < 0 ? baseText : baseText[..index];
    }
}
