using System;
using System.Reflection;
using NUnit.Framework;

namespace Umbraco.Tests.PartialTrust
{
	[TestFixture]
	public class PartialTrustHelperFixture
	{
		[TestFixtureSetUp]
		public void SetupFixture()
		{
			//TestHelper.SetupLog4NetForTests();
		}

		public class FakeFixture
		{
			public void PartialTrustShouldSucceed()
			{
				//LogHelper.TraceIfEnabled<FakeFixture>("In PartialTrustShouldSucceed, doing nothing much");
			}

			public void PartialTrustShouldFail()
			{
				//LogHelper.TraceIfEnabled<FakeFixture>("In PartialTrustShouldFail, about to try to access a private field");
				//using (DisposableTimer.TraceDuration<FakeFixture>("PartialTrustShouldFail", "PartialTrustShouldFail"))
				//{
				var fieldInfo = typeof(Int32).GetField("m_value", BindingFlags.Instance | BindingFlags.NonPublic);
				var value = fieldInfo.GetValue(1);
				//LogHelper.TraceIfEnabled<FakeFixture>("value: {0}", () => value);
				//}
			}
		}

		[Test]
		public void InPartialTrust_WhenMethodShouldSucceed_PartialTrustHelper_ReportsSuccess()
		{
			//LogHelper.TraceIfEnabled<PartialTrustHelperFixture>("In WhenTestShouldSucceed_InPartialTrust_PartialTrustHelper_ReportsSuccess");
			PartialTrustHelper<FakeFixture>.RunInPartial(Umbraco.Core.ExpressionHelper.GetMethodInfo<FakeFixture>(x => x.PartialTrustShouldSucceed()));
			Assert.Pass();
		}

		[Test]
		public void InPartialTrust_WhenMethodShouldNotSucceed_PartialTrustHelper_ReportsFailure()
		{
			//LogHelper.TraceIfEnabled<PartialTrustHelperFixture>("In WhenTestShouldNotSucceed_InPartialTrust_PartialTrustHelper_ReportsFailure");
			try
			{
				PartialTrustHelper<FakeFixture>.RunInPartial(Umbraco.Core.ExpressionHelper.GetMethodInfo<FakeFixture>(x => x.PartialTrustShouldFail()));
			}
			catch (PartialTrustTestException ex)
			{
				Assert.Pass("PartialTrustTestException raised, message: " + ex.Message);
				return;
			}
			catch (Exception ex)
			{
				Assert.Fail("Exception was raised but it was not PartialTrustTestException, was: " + ex.Message);
			}

			Assert.Fail("No exception was raised");
		}

		// To have all tests in a fixture automatically checked under the medium trust settings on your machine (web_mediumtrust.config)
		// add this to your test fixture (APN)
		//[SetUp]
		//public void PreTest()
		//{
		//    PartialTrustHelper<PartialTrustHelperFixture>.CheckRunNUnitTestInPartialTrust();
		//}
	}
}