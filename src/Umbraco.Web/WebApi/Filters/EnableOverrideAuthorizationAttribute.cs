using System;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// This allows for Action based auth attributes to override Class based auth attributes if they are specified
    /// </summary>
    /// <remarks>
    /// This attribute can be applied at the class level and will indicate to any class level auth attribute that inherits from OverridableAuthorizationAttribute
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class EnableOverrideAuthorizationAttribute : Attribute
    {
        
    }
}