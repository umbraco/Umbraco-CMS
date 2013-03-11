using System;

namespace Umbraco.Tests.CodeFirst.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SortOrderAttribute : Attribute
    {
        public SortOrderAttribute(int order)
        {
            Order = order;
        }

        /// <summary>
        /// Gets or sets the sort order of the Property
        /// </summary>
        public int Order { get; private set; }
    }
}