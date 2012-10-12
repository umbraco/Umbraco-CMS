using System;

namespace Umbraco.Core.Persistence.DatabaseAnnotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class ReferencesAttribute : Attribute
    {
        public ReferencesAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; set; }
    }
}