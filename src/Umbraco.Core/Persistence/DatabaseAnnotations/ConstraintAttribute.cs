using System;

namespace Umbraco.Core.Persistence.DatabaseAnnotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ConstraintAttribute : Attribute
    {
        /// <summary>
        /// Overrides the default naming of a property constraint:
        /// DF_tableName_propertyName
        /// </summary>
        public string Name { get; set; }

        public string Default { get; set; }
    }
}