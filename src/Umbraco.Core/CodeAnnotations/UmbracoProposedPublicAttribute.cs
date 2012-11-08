using System;

namespace Umbraco.Core.CodeAnnotations
{
	/// <summary>
	/// An attribute used to decorate classes or methods that have potential to become part of the public API
	/// </summary>
	/// <remarks>
	/// Classes, properties or methods marked with this attribute will have a tracker URL assigned where discussions may take place
	/// regarding whether or not this should become part of the public APIs and if/when. Please note that any objects marked with this
	/// attribute may not ever become public, this attribute exists to tag code that has potential to become public pending reviews.
	/// </remarks>
	internal class UmbracoProposedPublicAttribute : Attribute
	{
		/// <summary>
		/// constructor requires a tracker url and a description
		/// </summary>
		/// <param name="trackerUrl"></param>
		/// <param name="description"></param>
		public UmbracoProposedPublicAttribute(string trackerUrl, string description)
		{
			
		}
	}
}