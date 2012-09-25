using System;

namespace Umbraco.Core.ObjectResolution
{
	/// <summary>
	/// Some many object resolvers require that the objects that they resolve have weights applied to them so that
	/// the objects are returned in a sorted order, this attribute is used in these scenarios.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	internal class WeightedPluginAttribute : Attribute
	{
		public WeightedPluginAttribute(int weight)
		{
			Weight = weight;
		}

		public int Weight { get; private set; }
	}
}