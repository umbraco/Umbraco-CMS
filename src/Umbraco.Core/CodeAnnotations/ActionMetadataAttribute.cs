using System;

namespace Umbraco.Core.CodeAnnotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class ActionMetadataAttribute : Attribute
    {
        public string Category { get; private set; }
        public string Name { get; private set; }

        /// <summary>
        /// Constructor used to assign a Category, since no name is assigned it will try to be translated from the language files based on the action's alias
        /// </summary>
        /// <param name="category"></param>
        public ActionMetadataAttribute(string category)
        {
            if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Value cannot be null or whitespace.", "category");
            Category = category;
        }

        /// <summary>
        /// Constructor used to assign an explicit name and category
        /// </summary>
        /// <param name="category"></param>
        /// <param name="name"></param>
        public ActionMetadataAttribute(string category, string name)
        {
            if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Value cannot be null or whitespace.", "category");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", "name");
            Category = category;
            Name = name;
        }
    }
}