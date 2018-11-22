using System;

namespace Umbraco.Core.CodeAnnotations
{
	/// <summary>
	/// Marks the program elements that Umbraco is considering making public.
	/// </summary>
	/// <remarks>
	/// <para>Indicates that Umbraco considers making the (currently internal) program element public
	/// at some point in the future, but the decision is not fully made yet and is still pending reviews.</para>
	/// <para>Note that it is not a commitment to make the program element public. It may not ever become public.</para>
	/// <para>The issue tracker should contain more details, discussion, and planning.</para>
	/// </remarks>
	[AttributeUsage(AttributeTargets.All, AllowMultiple=false, Inherited=false)]
	internal sealed class UmbracoProposedPublicAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="UmbracoProposedPublicAttribute"/> class with a description.
		/// </summary>
		/// <param name="description">The text string that describes what is intended.</param>
		public UmbracoProposedPublicAttribute(string description)
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UmbracoProposedPublicAttribute"/> class with a tracker url and a description.
		/// </summary>
		/// <param name="trackerUrl">The url of a tracker issue containing more details, discussion, and planning.</param>
		/// <param name="description">The text string that describes what is intended.</param>
		public UmbracoProposedPublicAttribute(string trackerUrl, string description)
		{
			
		}
	}
}