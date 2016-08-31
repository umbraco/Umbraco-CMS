using System;

namespace Umbraco.Core.Components
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    public class RequireComponentAttribute : Attribute
    {
        public RequireComponentAttribute(Type requiredType)
        {
            RequiredType = requiredType;
        }

        public Type RequiredType { get; }
    }
}
