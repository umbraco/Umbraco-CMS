using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration;

[TestFixture]
[UmbracoTest(Boot = true)]
public class ComponentRuntimeTests : UmbracoIntegrationTest
{
    // ensure composers are added
    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddComposers();

    /// <summary>
    ///     This will boot up umbraco with components enabled to show they initialize and shutdown
    /// </summary>
    [Test]
    public async Task Start_And_Stop_Umbraco_With_Components_Enabled()
    {
        var runtime = Services.GetRequiredService<IRuntime>();
        var runtimeState = Services.GetRequiredService<IRuntimeState>();
        var mainDom = Services.GetRequiredService<IMainDom>();
        var components = Services.GetRequiredService<ComponentCollection>();

        var myComponent = components.OfType<MyComponent>().First();

        Assert.IsTrue(mainDom.IsMainDom);
        Assert.IsNull(runtimeState.BootFailedException);
        Assert.IsTrue(myComponent.IsInit, "The component was not initialized");

        // force stop now
        await runtime.StopAsync(CancellationToken.None);
        Assert.IsTrue(myComponent.IsTerminated, "The component was not terminated");
    }

    public class MyComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder) => builder.Components().Append<MyComponent>();
    }

    public class MyComponent : IComponent
    {
        private readonly ILogger<MyComponent> _logger;

        public MyComponent(ILogger<MyComponent> logger) => _logger = logger;
        public bool IsInit { get; private set; }

        public bool IsTerminated { get; private set; }

        public void Initialize() => IsInit = true;

        public void Terminate() => IsTerminated = true;
    }
}
