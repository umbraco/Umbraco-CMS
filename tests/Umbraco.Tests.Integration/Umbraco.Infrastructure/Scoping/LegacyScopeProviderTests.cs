using NUnit.Framework;
using Umbraco.Cms.Core.Scoping;
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
        var scopeProvider = GetRequiredService<IScopeProvider>();

        using (var scope = scopeProvider.CreateScope())
        {
            Assert.IsInstanceOf<IScope>(scope);
        }
    }

    [Test]
    public void LegacyScopeProvider_Always_IsACoreScopeProvider()
    {
        var scopeProvider = GetRequiredService<IScopeProvider>();

        Assert.IsInstanceOf<ICoreScopeProvider>(scopeProvider);
    }
}
