using System;

namespace Umbraco.Tests.CodeFirst.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DescriptionAttribute : Attribute
    {
        public DescriptionAttribute(string description)
        {
            Description = description;
        }

        /// <summary>
        /// Gets or sets the Description of the Property
        /// </summary>
        public string Description { get; private set; }
    }
}