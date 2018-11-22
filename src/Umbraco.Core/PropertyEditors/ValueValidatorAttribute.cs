using System;

namespace Umbraco.Core.PropertyEditors
{
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class ValueValidatorAttribute : Attribute
    {
        public ValueValidatorAttribute(string typeName)
        {
            TypeName = typeName;
        }
        public string TypeName { get; private set; }
    }
}