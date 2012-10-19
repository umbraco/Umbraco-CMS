using System;

namespace Umbraco.Core.Persistence.DatabaseAnnotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DatabaseTypeAttribute : Attribute
    {
        public DatabaseTypeAttribute(SpecialDbTypes databaseType)
        {
            DatabaseType = databaseType;
        }

        public SpecialDbTypes DatabaseType { get; set; }
        public int Length { get; set; }
    }
}