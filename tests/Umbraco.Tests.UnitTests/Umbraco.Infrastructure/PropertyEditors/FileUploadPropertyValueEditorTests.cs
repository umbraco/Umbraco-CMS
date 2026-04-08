using NUnit.Framework;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
public class FileUploadPropertyValueEditorTests
{
    [TestCase("pdf")]
    [TestCase("PDF")]
    [TestCase("Pdf")]
    public void IsAllowedInDataTypeConfiguration_Is_Case_Insensitive(string extension)
    {
        var configuration = new FileUploadConfiguration
        {
            FileExtensions = ["pdf", "doc", "docx"],
        };

        Assert.IsTrue(
            FileUploadPropertyValueEditor.IsAllowedInDataTypeConfiguration(extension, configuration),
            $"Extension '{extension}' should be allowed but was rejected");
    }

    [Test]
    public void IsAllowedInDataTypeConfiguration_Allows_All_When_No_Extensions_Configured()
    {
        var configuration = new FileUploadConfiguration
        {
            FileExtensions = Enumerable.Empty<string>(),
        };

        Assert.IsTrue(FileUploadPropertyValueEditor.IsAllowedInDataTypeConfiguration("anything", configuration));
    }

    [Test]
    public void IsAllowedInDataTypeConfiguration_Rejects_Non_Matching_Extension()
    {
        var configuration = new FileUploadConfiguration
        {
            FileExtensions = ["pdf", "doc"],
        };

        Assert.IsFalse(FileUploadPropertyValueEditor.IsAllowedInDataTypeConfiguration("jpg", configuration));
    }
}
