using System;

namespace Umbraco.Web.WebApi
{
    // TODO: This has been moved to netcore so can be removed when ready

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class IsBackOfficeAttribute : Attribute
    {
    }
}
