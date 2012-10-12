using System;

namespace Umbraco.Core.Persistence.DatabaseAnnotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DatabaseTypeAttribute : Attribute
    {
        public DatabaseTypeAttribute(DatabaseTypes databaseType)
        {
            DatabaseType = databaseType;
        }

        public DatabaseTypes DatabaseType { get; private set; }
        public int Length { get; set; }
    }
}