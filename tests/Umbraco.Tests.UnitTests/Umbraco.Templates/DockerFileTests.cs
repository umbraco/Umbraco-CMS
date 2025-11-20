// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Reflection;
using System.Runtime.Versioning;
using NUnit.Framework;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website;

[TestFixture]
public class DockerFileTests
{
    [Test]
    public void DockerFile_AspNet_Version_Matches_Target_Framework()
    {
        var targetFrameworkVersion = GetNetVersionFromCurrentTargetFramework();
        (string dockerFileAspNetVersion, string dockerFileSdkVersion) = GetNetVersionsFromDockerFile();
        Assert.AreEqual(dockerFileAspNetVersion, dockerFileSdkVersion);
        Assert.AreEqual(targetFrameworkVersion, dockerFileAspNetVersion);
    }

    private static string GetNetVersionFromCurrentTargetFramework()
    {
        var targetFrameworkAttribute = Assembly.GetExecutingAssembly()
            .GetCustomAttributes(typeof(TargetFrameworkAttribute), false)
            .SingleOrDefault() as TargetFrameworkAttribute;
        Assert.IsNotNull(targetFrameworkAttribute);

        return targetFrameworkAttribute.FrameworkName.Replace(".NETCoreApp,Version=v", string.Empty);
    }

    private static (string DockerFileAspNetVersion, string DockerFileSdkVersion) GetNetVersionsFromDockerFile()
    {
        const int SegmentsToRepoRoot = 5; // Number of directory segments from the test directory to the repository root.
        var testContextDirectoryParts = TestContext.CurrentContext.TestDirectory.Split(Path.DirectorySeparatorChar);
        var solutionRootDirectory = string.Join(Path.DirectorySeparatorChar, testContextDirectoryParts.Take(testContextDirectoryParts.Length - SegmentsToRepoRoot));
        var dockerFilePath = Path.Combine(solutionRootDirectory, "templates", "UmbracoProject", "Dockerfile");

        var dockerFileContent = File.ReadAllText(dockerFilePath);
        var dockerFileFromLines = dockerFileContent
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Where(x => x.StartsWith("FROM mcr."))
            .ToList();

        Assert.AreEqual(2, dockerFileFromLines.Count);

        var dockerFileAspNetVersion = GetVersionFromDockerFromLine(dockerFileFromLines[0]);
        var dockerFileSdkVersion = GetVersionFromDockerFromLine(dockerFileFromLines[1]);

        return (dockerFileAspNetVersion, dockerFileSdkVersion);
    }

    private static string GetVersionFromDockerFromLine(string line) => line.Split(' ')[1].Split(':')[1];
}
