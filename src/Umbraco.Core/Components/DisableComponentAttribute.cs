using System;

namespace Umbraco.Core.Components
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class DisableComponentAttribute : Attribute
    {
        public DisableComponentAttribute()
        { }

        public DisableComponentAttribute(Type disabledType)
        {
            DisabledType = disabledType;
        }

        public Type DisabledType { get; }
    }
}
