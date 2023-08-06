using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Search.Diagnostics;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.baseTests;

public class SearchProviderDiagnosticTests : UmbracoIntegrationTest
{
    //Test to check if the search provider deliver correct Diagnostics
    [Test]
    public void SearchProvider_Diagnostics_Works()
    {
        IIndexDiagnostics diagnostics = IndexDiagnosticsFactory.Create(Constants.UmbracoIndexes.InternalIndexName);
        Assert.AreEqual("LIFTI", diagnostics.SearchEngine.Name);
        Assert.AreEqual( "The LIFTI Query Syntax", diagnostics.SearchEngine.NativeSyntaxName);
        Assert.AreEqual("5.0.0", diagnostics.SearchEngine.Version);
    }
}
