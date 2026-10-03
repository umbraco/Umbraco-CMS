using NUnit.Framework;
using Umbraco.Cms.Search.Extension.FileToText.FileIndexing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Extension.FileToText.Tests.FileValueHandlerTests;

[TestFixture]
public class TextFileValueHandlerTests
{
    [TestCase(".txt", true)]
    [TestCase(".TXT", true)]
    [TestCase(".pdf", false)]
    [TestCase(".exe", false)]
    [TestCase("", false)]
    public void Can_Handle_File_Extension(string extension, bool expectedCanHandle)
    {
        var fileValueHandler = new TextFileValueHandler();
        Assert.That(fileValueHandler.CanHandle(extension), Is.EqualTo(expectedCanHandle));
    }

    [Test]
    public async Task Can_Read_File()
    {
        var fileValueHandler = new TextFileValueHandler();
        await using FileStream stream = File.OpenRead(TestFileHelper.LoremIpsumFilePath(".txt"));
        var text = await fileValueHandler.GetFileContentsAsync(stream, CancellationToken.None);

        Assert.That(text, Does.StartWith(TestFileHelper.LoremIpsumStart));
    }
}
