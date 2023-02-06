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

    private IEFCoreScopeAccessor EfCoreScopeAccessor => GetRequiredService<IEFCoreScopeAccessor>();

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

    [Test]
    public void GivenUncompletedScopeOnChildThread_WhenTheParentCompletes_TheTransactionIsRolledBack()
    {
        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        IEfCoreScope mainScope = EfCoreScopeProvider.CreateScope();

        var t = Task.Run(() =>
        {
            IEfCoreScope nested = EfCoreScopeProvider.CreateScope();
            Thread.Sleep(2000);
            nested.Dispose();
        });

        Thread.Sleep(1000); // mimic some long running operation that is shorter than the other thread
        mainScope.Complete();
        Assert.Throws<InvalidOperationException>(() => mainScope.Dispose());

        Task.WaitAll(t);
    }


}
