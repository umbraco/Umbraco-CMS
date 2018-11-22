using System;

namespace Umbraco.Core.CodeAnnotations
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    internal class UmbracoUdiTypeAttribute : Attribute
    {
        public string UdiType { get; private set; }

        public UmbracoUdiTypeAttribute(string udiType)
        {
            UdiType = udiType;
        }
    }
}
