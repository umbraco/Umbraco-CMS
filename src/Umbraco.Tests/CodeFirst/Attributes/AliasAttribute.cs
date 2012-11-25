using System;

namespace Umbraco.Tests.CodeFirst.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class AliasAttribute : Attribute
    {
        public AliasAttribute(string @alias)
        {
            Alias = alias;
        }

        /// <summary>
        /// Gets or Sets the Alias of the Property
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// Gets or Sets an optional name of the Property
        /// </summary>
        public string Name { get; set; }
    }
}