using System;

namespace Umbraco.Cms.Core.CodeAnnotations
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class UmbracoUdiTypeAttribute : Attribute
    {
        public string UdiType { get; private set; }

        public UmbracoUdiTypeAttribute(string udiType)
        {
            UdiType = udiType;
        }
    }
}
