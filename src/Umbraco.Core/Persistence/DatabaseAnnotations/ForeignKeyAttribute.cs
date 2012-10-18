using System;

namespace Umbraco.Core.Persistence.DatabaseAnnotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ForeignKeyAttribute : ReferencesAttribute
    {
        public ForeignKeyAttribute(Type type) : base(type)
        {
        }

        public string OnDelete { get; set; }
        public string OnUpdate { get; set; }

        public string Name { get; set; }//Used to override Default naming: FK_thisTableName_refTableName
        public string Column { get; set; }//Used to point foreign key to a specific Column. PrimaryKey column is used by default
    }
}