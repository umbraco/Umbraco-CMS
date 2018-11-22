using System;
using Umbraco.Tests.CodeFirst.Definitions;

namespace Umbraco.Tests.CodeFirst.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class PropertyTypeConventionAttribute : Attribute
    {
        public abstract PropertyDefinition GetPropertyConvention();
    }
}