using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Core.CodeAnnotations
{

	/// <summary>
	/// An attribute used to decorate classes or methods that have been marked to be obsoleted
	/// </summary>
	/// <remarks>
	/// This attribute exists because sometimes we may not want to explicitly [Obsolete] a class or method because we 
	/// know it is still in use and we don't want warning messages showing up in developers output windows when they compile, however
	/// we will be able to produce reports declaring which classes/methods/etc... have been marked for being obsoleted in the near future
	/// </remarks>
	internal class UmbracoDeprecateAttribute : Attribute
	{
		/// <summary>
		/// Constructor requries a description
		/// </summary>
		/// <param name="description"></param>
		public UmbracoDeprecateAttribute(string description)
		{
			
		}
	}
}
