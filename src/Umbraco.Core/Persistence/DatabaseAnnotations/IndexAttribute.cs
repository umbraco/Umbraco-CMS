using System;

namespace Umbraco.Core.Persistence.DatabaseAnnotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IndexAttribute : Attribute
    {
        public IndexAttribute(IndexTypes indexType)
        {
            IndexType = indexType;
        }
        
        //public Type Type { get; set; }
        public string Name { get; set; }//Overrides default naming of indexes: IX_tableName
        public IndexTypes IndexType { get; private set; }
    }
}