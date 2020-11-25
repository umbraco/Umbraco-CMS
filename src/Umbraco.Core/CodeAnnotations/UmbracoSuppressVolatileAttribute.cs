using System;

namespace Umbraco.Core.CodeAnnotations
{
    /// This attribute is used by the Umbraco.Code package to control the behaviour of the Volatile Analyzer.
    /// If a user wishes to use a resource marked with the UmbracoVolatileAttribute for testing purposes
    /// they can mark their assembly with this attribute, and the analyzer will diagnose a warning instead of an error,
    /// allowing their build to succeed. We don't need to use this in the Umbraco projects because the analyzer has an
    /// allow list specifically for the Umbraco CMS projects.

    /// <summary>
    /// Assembly level attribute used to suppress <see cref="UmbracoVolatileAttribute"/> errors to warnings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class UmbracoSuppressVolatileAttribute : Attribute
    {

    }
}
