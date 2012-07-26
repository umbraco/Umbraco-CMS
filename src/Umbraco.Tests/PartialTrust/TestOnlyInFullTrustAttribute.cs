using System;

namespace Umbraco.Tests.PartialTrust
{
	/// <summary>
	/// Specifies that the <see cref="AbstractPartialTrustFixture{T}"/> will not run the decorated test in a partial trust AppDomain
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class TestOnlyInFullTrustAttribute : Attribute
	{
		/// <summary>
		/// Specifies that the <see cref="AbstractPartialTrustFixture{T}"/> will not run the decorated test in a partial trust AppDomain
		/// </summary>
		public TestOnlyInFullTrustAttribute()
		{
		}
	}
}