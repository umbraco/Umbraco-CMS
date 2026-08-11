// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations.Upgrade.V_19_0_0;

[TestFixture]
public class RemoveLegacyExamineIndexFilesTests
{
    private string _tempDataPath;
    private string _examineIndexesPath;

    [SetUp]
    public void SetUp()
    {
        _tempDataPath = Path.Combine(Path.GetTempPath(), "RemoveLegacyExamineIndexFilesTests_" + Guid.NewGuid());
        _examineIndexesPath = Path.Combine(_tempDataPath, "ExamineIndexes");
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDataPath))
        {
            Directory.Delete(_tempDataPath, recursive: true);
        }
    }

    [Test]
    public async Task Removes_Existing_Examine_Index_Folder()
    {
        var indexFolder = Path.Combine(_examineIndexesPath, "Umb_PublishedContent");
        Directory.CreateDirectory(indexFolder);
        File.WriteAllText(Path.Combine(indexFolder, "segments.gen"), "test");
        File.WriteAllText(Path.Combine(indexFolder, "_0.cfs"), "test");

        await RunMigration();

        Assert.That(Directory.Exists(_examineIndexesPath), Is.False);
    }

    [Test]
    public void Does_Nothing_When_No_Examine_Index_Folder_Exists()
    {
        Assert.DoesNotThrowAsync(RunMigration);

        Assert.That(Directory.Exists(_examineIndexesPath), Is.False);
    }

    [Test]
    public async Task Does_Not_Delete_Unrelated_Sibling_Folders()
    {
        var untouchedFolder = Path.Combine(_tempDataPath, "FileUploads");
        Directory.CreateDirectory(untouchedFolder);
        File.WriteAllText(Path.Combine(untouchedFolder, "upload.tmp"), "test");
        Directory.CreateDirectory(_examineIndexesPath);

        await RunMigration();

        Assert.That(Directory.Exists(_examineIndexesPath), Is.False);
        Assert.That(Directory.Exists(untouchedFolder), Is.True);
    }

    private async Task RunMigration()
    {
        var hostingEnvironment = new Mock<IHostingEnvironment>();
        hostingEnvironment.Setup(x => x.MapPathContentRoot(It.IsAny<string>())).Returns(_tempDataPath);

        var context = new MigrationContext(new TestPlan(), Mock.Of<IUmbracoDatabase>(), Mock.Of<ILogger<MigrationContext>>());

        var migration = new RemoveLegacyExamineIndexFiles(
            context,
            hostingEnvironment.Object,
            Mock.Of<ILogger<RemoveLegacyExamineIndexFiles>>());

        await migration.RunAsync();
    }

    private class TestPlan : MigrationPlan
    {
        public TestPlan()
            : base("Test")
        {
        }
    }
}
