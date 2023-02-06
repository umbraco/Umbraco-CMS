using NUnit.Framework;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
public class EfCoreScopeTest : UmbracoIntegrationTest
{
    private IEfCoreScopeProvider EfCoreScopeProvider =>
        GetRequiredService<IEfCoreScopeProvider>();

    [Test]
    public void CanCreateScope() =>
        Assert.DoesNotThrow(() =>
        {
            using var scope = EfCoreScopeProvider.CreateScope();
            scope.Complete();
        });

    [Test]
    public void CanCreateScopeTwice() =>
        Assert.DoesNotThrow(() =>
        {
            using (var scope = EfCoreScopeProvider.CreateScope())
            {
                scope.Complete();
            }

            using (var scopeTwo = EfCoreScopeProvider.CreateScope())
            {
                scopeTwo.Complete();
            }
        });
}
