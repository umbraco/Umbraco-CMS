using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Examine.Lucene.UmbracoExamine.ContentIndexRebuild;

internal abstract partial class ContentIndexRebuildTestBase
{
    /// <summary>
    ///     Set this environment variable to <c>true</c> to write the actual output over the expected snapshot files in the
    ///     source tree instead of asserting against them.
    /// </summary>
    private const string UpdateSnapshotsVariable = "UMBRACO_TEST_UPDATE_SNAPSHOTS";

    protected static void AssertMatchesSnapshot(string snapshotName, string actual, [CallerFilePath] string callerFilePath = "")
    {
        if (string.Equals(Environment.GetEnvironmentVariable(UpdateSnapshotsVariable), "true", StringComparison.OrdinalIgnoreCase))
        {
            var path = Path.Combine(Path.GetDirectoryName(callerFilePath)!, "Snapshots", $"{snapshotName}.txt");
            File.WriteAllText(path, actual);
            Assert.Warn($"Snapshot {snapshotName} was updated at {path}.");
            return;
        }

        var expected = ReadSnapshot(snapshotName);
        if (expected == actual)
        {
            return;
        }

        var actualPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{snapshotName}.actual.txt");
        File.WriteAllText(actualPath, actual);
        TestContext.AddTestAttachment(actualPath);

        string[] expectedLines = expected.Split('\n');
        string[] actualLines = actual.Split('\n');
        var firstDifference = Enumerable.Range(0, Math.Max(expectedLines.Length, actualLines.Length))
            .First(i => i >= expectedLines.Length || i >= actualLines.Length || expectedLines[i] != actualLines[i]);

        Assert.Fail(
            $"Index output differs from snapshot {snapshotName} at line {firstDifference + 1}.{Environment.NewLine}" +
            $"Expected: {(firstDifference < expectedLines.Length ? expectedLines[firstDifference] : "<end of file>")}{Environment.NewLine}" +
            $"Actual:   {(firstDifference < actualLines.Length ? actualLines[firstDifference] : "<end of file>")}{Environment.NewLine}" +
            $"Full output: {actualPath}. Set {UpdateSnapshotsVariable}=true to update the snapshot if the change is intended.");
    }

    private static string ReadSnapshot(string snapshotName)
    {
        var resourceName = $"ContentIndexRebuild.Snapshots.{snapshotName}.txt";
        using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            Assert.Fail($"Snapshot resource {resourceName} was not found. Set {UpdateSnapshotsVariable}=true to create it.");
        }

        using var reader = new StreamReader(stream!);
        return reader.ReadToEnd().Replace("\r\n", "\n");
    }
}
