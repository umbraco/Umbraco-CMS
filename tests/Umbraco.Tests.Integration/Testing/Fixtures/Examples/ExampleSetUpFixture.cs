using NUnit.Framework;
using Umbraco.Cms.Tests.Common.Testing;

namespace Umbraco.Cms.Tests.Integration.Testing.Fixtures.Examples;

/// <summary>
///     Example [SetUpFixture] demonstrating how to boot Umbraco once for all test fixtures
///     in this namespace. All tests in <c>Umbraco.Cms.Tests.Integration.Testing.Fixtures.Examples</c>
///     share this single Umbraco instance, dramatically reducing test execution time.
/// </summary>
/// <remarks>
///     <para>
///     This boots one host for all test fixtures in this namespace. Individual tests still get
///     their own database (per the <see cref="UmbracoTestAttribute"/> configuration on their fixture),
///     but the expensive host build/start happens only once.
///     </para>
///     <para>
///     To use this pattern in your own tests:
///     1. Create a [SetUpFixture] like this one in your test namespace
///     2. Have it inherit from <see cref="UmbracoIntegrationFixture"/> (or a variant)
///     3. Call BuildAndStartHost() in [OneTimeSetUp] and StopAndDisposeHost() in [OneTimeTearDown]
///     4. Expose the fixture instance statically so test fixtures can access Services
///     5. In your [TestFixture] classes, access services via the static fixture instance
///     </para>
/// </remarks>
[SetUpFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console, Boot = true)]
public class ExampleSetUpFixture : UmbracoIntegrationFixture
{
    /// <summary>
    ///     The shared fixture instance, accessible by all test fixtures in this namespace.
    /// </summary>
    public static ExampleSetUpFixture Instance { get; private set; }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Instance = this;
        OnSetUpLogging();
        BuildAndStartHost();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        StopAndDisposeHost();
        OnFixtureTearDown();
        OnTearDownLogging();
        Instance = null;
    }
}
