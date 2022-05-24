using NUnit.Framework;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
public class LegacyScopeProviderTests : UmbracoIntegrationTest
{
    [Test]
    public void CreateScope_Always_ReturnsLegacyIScope()
    {
        var scopeProvider = GetRequiredService<global::Umbraco.Cms.Core.Scoping.IScopeProvider>();

        using (var scope = scopeProvider.CreateScope())
        {
            Assert.IsInstanceOf<global::Umbraco.Cms.Core.Scoping.IScope>(scope);
        }
    }
}
