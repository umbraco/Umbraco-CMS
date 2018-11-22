using System;

namespace Umbraco.Core.CodeAnnotations
{
	/// <summary>
	/// Marks the program elements that Umbraco is experimenting with and could become public.
	/// </summary>
	/// <remarks>
	/// <para>Indicates that Umbraco  is experimenting with code that potentially could become
	/// public, but we're not sure, and the code is not stable and can be refactored at any time.</para>
	/// <para>The issue tracker should contain more details, discussion, and planning.</para>
	/// </remarks>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
	internal sealed class UmbracoExperimentalFeatureAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="UmbracoExperimentalFeatureAttribute"/> class with a description.
		/// </summary>
		/// <param name="description">The text string that describes what is intended.</param>
		public UmbracoExperimentalFeatureAttribute(string description)
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UmbracoExperimentalFeatureAttribute"/> class with a tracker url and a description.
		/// </summary>
		/// <param name="trackerUrl">The url of a tracker issue containing more details, discussion, and planning.</param>
		/// <param name="description">The text string that describes what is intended.</param>
		public UmbracoExperimentalFeatureAttribute(string trackerUrl, string description)
		{
			
		}
	}
}