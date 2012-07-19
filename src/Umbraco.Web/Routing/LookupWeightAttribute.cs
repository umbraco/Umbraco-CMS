using System;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Specifies the relative weight of an ILookup.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class LookupWeightAttribute : Attribute
    {

        /// <summary>
        /// Gets the default part weight.
        /// </summary>
        public const int DefaultWeight = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="LookupWeightAttribute"/> class with the weight.
        /// </summary>
        /// <param name="weight">The weight of the part.</param>
        public LookupWeightAttribute(int weight)
            : base()
        {
            this.Weight = weight;
        }

        /// <summary>
        /// Gets the weight of the part.
        /// </summary>
        public int Weight { get; private set; }
    }
}