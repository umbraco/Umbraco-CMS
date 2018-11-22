using System;

namespace Umbraco.Tests.CodeFirst.Attributes
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class MixinAttribute : Attribute
    {
        public MixinAttribute(Type type)
        {
            Type = type;
        }

        /// <summary>
        /// Gets or sets the Type of the implementing class
        /// </summary>
        public Type Type { get; private set; }
    }
}