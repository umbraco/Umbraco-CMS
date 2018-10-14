using System;

namespace Umbraco.Web.Models.Trees
{
    /// <summary>
    /// Identifies an application tree
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ApplicationAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationAttribute"/> class.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="name">The name.</param>
        /// <param name="sortOrder">The sort order.</param>
        public ApplicationAttribute(string alias,
            string name,
            int sortOrder = 0)
        {
            Alias = alias;
            Name = name;
            SortOrder = sortOrder;
        }

        public string Alias { get; private set; }
        public string Name { get; private set; }
        public int SortOrder { get; private set; }
    }
}
