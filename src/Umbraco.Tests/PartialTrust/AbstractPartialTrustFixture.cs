using System;
using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.PartialTrust
{
	[TestFixture]
	public abstract class AbstractPartialTrustFixture<T> : IPartialTrustFixture
		where T : class, IPartialTrustFixture, new()
	{
		private AppDomain _partialTrustAppDomain;
		private PartialTrustHelper<T>.PartialTrustMethodRunner<T> _methodRunner;

		/// <summary>
		/// Run once by NUnit for this fixture. This will setup a new partial-trust <see cref="AppDomain"/>
		/// for the duration of this test fixture, and a <see cref="PartialTrustHelper{T}"/> which will also
		/// survive for the life of the test fixture.
		/// </summary>
		[TestFixtureSetUp]
		protected virtual void FixtureSetup()
		{
			_partialTrustAppDomain = PartialTrustHelper<T>.CreatePartialTrustDomain();
			_methodRunner = PartialTrustHelper<T>.GenerateMarshaller(_partialTrustAppDomain);
		}

		/// <summary>
		/// Causes the partial-trust <see cref="AppDomain"/> to be unloaded.
		/// </summary>
		[TestFixtureTearDown]
		protected void FixtureTearDown()
		{
			AppDomain.Unload(_partialTrustAppDomain);
		}

		/// <summary>
		/// This is run before each test by the NUnit runner. It will first cause <see cref="TestSetup"/> to be
		/// run on the derived test class in the partial-trust domain. It will then run the current test method
		/// (obtained from NUnit's TestContext) in the same partial-trust domain. In order to prevent the same test
		/// then being run a second time in our original full-trust <see cref="AppDomain"/>, it then throws
		/// an NUnit <see cref="SuccessException"/> which counts as a Pass and prevents continued execution of the test.
		/// </summary>
		/// <remarks>
		/// If the test failed in partial trust, it will be marked as failed as normal - including any errors specific
		/// to running it in a partial trust environment.
		/// </remarks>
		[SetUp]
		protected void PartialTrustSetup()
		{
			if (ShouldTestOnlyInFullTrust())
			{
				// Do not run this test in partial trust
				TestSetup();
				return;
			}

			// First, run the TestSetup method in the partial trust domain
			_methodRunner.Run(Umbraco.Core.ExpressionHelper.GetMethodInfo<AbstractPartialTrustFixture<T>>(x => x.TestSetup()));

			// Now, run the actual test in the same appdomain
			PartialTrustHelper<T>.CheckRunNUnitTestInPartialTrust(_partialTrustAppDomain, _methodRunner);

			// If we get here, we've had no exceptions, so throw an NUnit SuccessException which will avoid rerunning the test
			// in the existing full-trust appdomain and cause the test to pass
			throw new SuccessException("Automatically avoided running test in full trust as it passed in partial trust");
		}

		private static bool ShouldTestOnlyInFullTrust()
		{
			var currentTest = PartialTrustHelper<T>.FindCurrentTestMethodInfo();

			// If we can't find a compatible test, it might be because we cannot run it remotely in the
			// AppDomain e.g. if it's a parameterised test, in which case fail with a note
			if (currentTest == null) Assert.Fail("Cannot find current test; is test parameterised?");

			return currentTest.GetCustomAttributes(typeof(TestOnlyInFullTrustAttribute), false).Any();
		}

		/// <summary>
		/// Run once before each test in derived test fixtures.
		/// </summary>
		public abstract void TestSetup();

		/// <summary>
		/// This is run once after each test by the NUnit runner. It will cause <see cref="TestTearDown"/> to be 
		/// run on the derived test class in the partial-trust domain.
		/// </summary>
		[TearDown]
		public void PartialTrustTearDown()
		{
			if (ShouldTestOnlyInFullTrust())
			{
				// Do not run this test in partial trust
				TestTearDown();
				return;
			}

			// Run the TestTearDown method in the partial trust domain
			_methodRunner.Run(Umbraco.Core.ExpressionHelper.GetMethodInfo<AbstractPartialTrustFixture<T>>(x => x.TestTearDown()));
		}

		/// <summary>
		/// Run once after each test in derived test fixtures.
		/// </summary>
		public abstract void TestTearDown();
	}
}