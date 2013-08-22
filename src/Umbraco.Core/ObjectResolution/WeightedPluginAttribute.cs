using System;

namespace Umbraco.Core.ObjectResolution
{
	/// <summary>
	/// Indicates the relative weight of a resolved object type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	internal class WeightedPluginAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WeightedPluginAttribute"/> class with a weight.
		/// </summary>
		/// <param name="weight">The object type weight.</param>
		public WeightedPluginAttribute(int weight)
		{
			Weight = weight;
		}

		/// <summary>
		/// Gets or sets the weight of the object type.
		/// </summary>
		public int Weight { get; private set; }
	}
}