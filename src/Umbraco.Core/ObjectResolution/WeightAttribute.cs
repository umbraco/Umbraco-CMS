using System;

namespace Umbraco.Core.ObjectResolution
{
	/// <summary>
	/// Indicates the relative weight of a resolved object type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class WeightAttribute : Attribute
	{
        /// <summary>
		/// Initializes a new instance of the <see cref="WeightAttribute"/> class with a weight.
		/// </summary>
		/// <param name="weight">The object type weight.</param>
		/// <remarks>
		/// This internal constructor allows for internal Umbraco products to set a negative number weight
		/// </remarks>
	    internal WeightAttribute(int weight)
	    {
	        Weight = weight;
	    }

		/// <summary>
		/// Initializes a new instance of the <see cref="WeightAttribute"/> class with a weight.
		/// </summary>
		/// <param name="weight">The object type weight.</param>
		/// <remarks>
		/// The weight must be a positive number
		/// </remarks>
		public WeightAttribute(uint weight)
		{
			Weight = Convert.ToInt32(weight);
		}

		/// <summary>
		/// Gets or sets the weight of the object type.
		/// </summary>
		public int Weight { get; private set; }
	}
}