using System;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Specifies the relative weight of an <c>IRequestDocumentResolver</c> implementation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class RequestDocumentResolverWeightAttribute : Attribute
    {
        /// <summary>
        /// Gets the default weight.
        /// </summary>
        public const int DefaultWeight = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestDocumentResolverWeightAttribute"/> class with the weight.
        /// </summary>
        /// <param name="weight">The weight.</param>
        public RequestDocumentResolverWeightAttribute(int weight)
            : base()
        {
            this.Weight = weight;
        }

        /// <summary>
        /// Gets the weight.
        /// </summary>
        public int Weight { get; private set; }
    }
}