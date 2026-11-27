using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Search.Querying;
using Umbraco.Cms.Search.Provider.Examine.Services;
using Umbraco.Cms.Tests.Common.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests;

#pragma warning disable CS0618 // Type or member is obsolete
[LongRunning]
public class StopWordDetectorTests : SearcherTestBase
{
    protected override void ConfigureServices(IServiceCollection services)
        => services.AddUnique<IStopWordDetector, NoStopWordDetector>();

    [Test]
    public async Task QueryTermsNotDetectedAsStopWordsAreRequired()
    {
        SearchResult result = await SearchAsync(query: StopWordsPhrase);

        Assert.That(result.Total, Is.EqualTo(0));
    }

    private class NoStopWordDetector : IStopWordDetector
    {
        public bool IsStopWord(string term) => false;
    }
}
#pragma warning restore CS0618 // Type or member is obsolete
