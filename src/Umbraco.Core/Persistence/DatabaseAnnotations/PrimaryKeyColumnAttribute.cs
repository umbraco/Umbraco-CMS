using System;

namespace Umbraco.Core.Persistence.DatabaseAnnotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyColumnAttribute : Attribute
    {
        public PrimaryKeyColumnAttribute()
        {
            Clustered = true;
            AutoIncrement = true;
        }

        public bool Clustered { get; set; }//Defaults to true
        public bool AutoIncrement { get; set; }//Default to true
        public string Name { get; set; }//Overrides the default naming of a PrimaryKey constraint: PK_tableName
        public string OnColumns { get; set; }
    }
}