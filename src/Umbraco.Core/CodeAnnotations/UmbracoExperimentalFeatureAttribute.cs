using System;

namespace Umbraco.Core.CodeAnnotations
{
	/// <summary>
	/// An attribute used to decorate classes or methods that have been marked to become public but require more testing and review 
	/// before this is possible.
	/// </summary>
	internal class UmbracoExperimentalFeatureAttribute : Attribute
	{
		/// <summary>
		/// constructor requires a tracker url and a description
		/// </summary>
		/// <param name="trackerUrl"></param>
		/// <param name="description"></param>
		public UmbracoExperimentalFeatureAttribute(string trackerUrl, string description)
		{
			
		}
	}
}