using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Core.CodeAnnotations
{
	/// <summary>
	/// Marks the program elements that Umbraco will obsolete.
	/// </summary>
	/// <remarks>
	/// Indicates that Umbraco will obsolete the program element at some point in the future, but we do not want to
	/// explicitely mark it [Obsolete] yet to avoid warning messages showing when developers compile Umbraco.
	/// </remarks>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
	internal sealed class UmbracoWillObsoleteAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="UmbracoWillObsoleteAttribute"/> class with a description.
		/// </summary>
		/// <param name="description">The text string that describes what is intended.</param>
		public UmbracoWillObsoleteAttribute(string description)
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UmbracoWillObsoleteAttribute"/> class with a tracker url and a description.
		/// </summary>
		/// <param name="trackerUrl">The url of a tracker issue containing more details, discussion, and planning.</param>
		/// <param name="description">The text string that describes what is intended.</param>
		public UmbracoWillObsoleteAttribute(string trackerUrl, string description)
		{
			
		}
	}
}
