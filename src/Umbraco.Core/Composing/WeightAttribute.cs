using System;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Specifies the weight of pretty much anything.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class WeightAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WeightAttribute"/> class with a weight.
        /// </summary>
        /// <param name="weight"></param>
        public WeightAttribute(int weight)
        {
            Weight = weight;
        }

        /// <summary>
        /// Gets the weight value.
        /// </summary>
        public int Weight { get; }
    }
}
