using System;

namespace Umbraco.Core.CodeAnnotations
{
    /// <summary>
    /// Assembly level attribute used to suppress <see cref="UmbracoVolatileAttribute"/> errors to warnings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class UmbracoSuppressVolatileAttribute : Attribute
    {

    }
}
