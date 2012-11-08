using System;
using System.Security;
using NUnit.Framework;

namespace Umbraco.Tests.PartialTrust
{
	[TestFixture]
	public class PartialTrustFixtureFixture : AbstractPartialTrustFixture<PartialTrustFixtureFixture>
	{
		public override void TestSetup()
		{
			//LogHelper.TraceIfEnabled<PartialTrustFixtureFixture>("Being setup");
		}

		public override void TestTearDown()
		{
			//LogHelper.TraceIfEnabled<PartialTrustFixtureFixture>("Being torn down");
		}

		[Test]
		public void InPartialTrust_WhenMethodShouldSucceed_PartialTrustHelper_ReportsSuccess()
		{
			//LogHelper.TraceIfEnabled<PartialTrustFixtureFixture>("In WhenTestShouldSucceed_InPartialTrust_PartialTrustHelper_ReportsSuccess");
			var fakeFixture = new PartialTrustHelperFixture.FakeFixture();
			fakeFixture.PartialTrustShouldSucceed();
		}

		[Test]
		public void InPartialTrust_WhenMethodShouldNotSucceed_PartialTrustHelper_ReportsFailure()
		{
			//LogHelper.TraceIfEnabled<PartialTrustFixtureFixture>("In WhenTestShouldNotSucceed_InPartialTrust_PartialTrustHelper_ReportsFailure");
			try
			{
				var fakeFixture = new PartialTrustHelperFixture.FakeFixture();
				fakeFixture.PartialTrustShouldFail();
			}
			catch (FieldAccessException ex)
			{
				Assert.Pass("FieldAccessException raised, message: " + ex.Message);
				return;
			}
			catch (SecurityException ex)
			{
				Assert.Pass("SecurityException raised, message: " + ex.Message);
				return;
			}
			catch (Exception ex)
			{
				Assert.Fail("Exception was raised but it was not SecurityException, was: " + ex.ToString());
			}

			Assert.Fail("No exception was raised");
		}
	}
}