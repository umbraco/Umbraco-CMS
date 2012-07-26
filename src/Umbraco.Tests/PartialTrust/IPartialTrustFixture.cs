using System.Web.Mvc;

namespace Umbraco.Tests.PartialTrust
{
	public interface IPartialTrustFixture
	{
		/// <summary>
		/// Run once before each test in derived test fixtures.
		/// </summary>
		void TestSetup();

		/// <summary>
		/// Run once after each test in derived test fixtures.
		/// </summary>
		void TestTearDown();
	}
}