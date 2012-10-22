using System;

namespace Umbraco.Core.Persistence.DatabaseAnnotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SpecialDbTypeAttribute : Attribute
    {
        public SpecialDbTypeAttribute(SpecialDbTypes databaseType)
        {
            DatabaseType = databaseType;
        }

        public SpecialDbTypes DatabaseType { get; private set; }
    }
}