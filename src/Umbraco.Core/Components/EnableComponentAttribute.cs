using System;

namespace Umbraco.Core.Components
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class EnableComponentAttribute : Attribute
    {
        public EnableComponentAttribute()
        { }

        public EnableComponentAttribute(Type enabledType)
        {
            EnabledType = enabledType;
        }

        public Type EnabledType { get; }
    }
}
