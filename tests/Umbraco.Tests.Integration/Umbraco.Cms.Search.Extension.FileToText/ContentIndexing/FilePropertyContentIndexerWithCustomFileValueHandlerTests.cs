using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Search.Extension.FileToText.FileIndexing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Extension.FileToText.Tests.ContentIndexing;

internal class FilePropertyContentIndexerWithCustomFileValueHandlerTests : FilePropertyContentIndexerTestBase
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);
        services.AddSingleton<IFileValueHandler, MyPdfFileValueHandler>();
    }

    [Test]
    public async Task Can_Override_Default_Handler()
    {
        IMedia media = await CreateLoremIpsumFileMedia(".pdf");

        IndexField[] fields = (await FilePropertyContentIndexer.GetIndexFieldsAsync(media, [], false, CancellationToken.None)).ToArray();
        Assert.That(fields, Has.Length.EqualTo(1));

        var texts = fields[0].Value.Texts?.ToArray();
        Assert.That(texts, Is.Not.Null);
        Assert.That(texts, Has.Length.EqualTo(1));
        Assert.That(texts.Single(), Does.StartWith("This is the file content."));
    }

    private class MyPdfFileValueHandler : IFileValueHandler
    {
        public bool CanHandle(string extension)
            => extension.InvariantEquals(".pdf");

        public Task<string> GetFileContentsAsync(Stream stream, CancellationToken cancellationToken)
            => Task.FromResult("This is the file content.");
    }
}
