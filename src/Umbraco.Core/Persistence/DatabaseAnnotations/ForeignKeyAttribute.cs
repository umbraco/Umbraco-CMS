using System;

namespace Umbraco.Core.Persistence.DatabaseAnnotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKeyAttribute : ReferencesAttribute
    {
        public ForeignKeyAttribute(Type type) : base(type)
        {
        }

        public string OnDelete { get; set; }
        public string OnUpdate { get; set; }

        public string Name { get; set; }//Used to override Default naming: FK_thisTableName_refTableName
    }
}