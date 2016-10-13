using NUnit.Framework;
using Umbraco.Core.DI;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Provides the top-level base class for all Umbraco tests.
    /// </summary>
    /// <remarks>
    /// <para>Ensures that Current is properly resetted before and after each test executes.</para>
    /// <para>Defines the SetUp and TearDown methods with proper attributes (not needed on overrides).</para>
    /// </remarks>
    public abstract class BaseTestBase
    {
        [SetUp]
        public virtual void SetUp()
        {
            // should not need this if all other tests were clean
            // but hey, never know, better avoid garbage-in
            Current.Reset();
        }

        [TearDown]
        public virtual void TearDown()
        {
            Current.Reset();
        }
    }
}
