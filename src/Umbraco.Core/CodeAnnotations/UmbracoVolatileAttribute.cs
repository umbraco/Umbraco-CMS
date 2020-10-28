using System;

namespace Umbraco.Core.CodeAnnotations
{
    /// <summary>
    /// An empty attribute which is used as a marker for Roslyn Analyzers.
    /// </summary>
    /// <remarks>
    /// Any class or method marked with this attribute will throw a Volatile error which can be suppressed
    /// with <see cref="UmbracoSuppressVolatileAttribute"/>, this is intended for classes which are public
    /// for testing purposes, but should not be accessed otherwise
    /// </remarks>
    public class UmbracoVolatileAttribute : Attribute
    {

    }
}
