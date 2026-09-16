using NUnit.Framework;
using Umbraco.Cms.Search.Core.Models.Searching;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests;

// tests related to searching with an empty (rather than null) culture, as happens for the
// invariant Media section list view in the backoffice
public class EmptyCultureTests : SearcherTestBase
{
    [Test]
    public async Task CanQueryInvariantDocumentsWithEmptyCulture()
    {
        SearchResult result = await SearchAsync(query: "single12", culture: string.Empty);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(1));
                Assert.That(result.Documents.First().Id, Is.EqualTo(DocumentIds[12]));
            });
    }
}
