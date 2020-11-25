using System;

namespace Umbraco.Core.CodeAnnotations
{
    /// <summary>
    /// An empty attribute which is used as a marker for the Volatile Analyzer contained in the Umbraco.Code package.
    /// </summary>
    /// <remarks>
    /// Any class or method marked with this attribute will throw a Volatile error which can be suppressed
    /// with <see cref="UmbracoSuppressVolatileAttribute"/>, this is intended for classes which are public
    /// for testing purposes, but should not be accessed otherwise.
    /// The analyzer resides in the Umbraco.Code package, so if that package isn't installed this attribute has no effect.
    /// </remarks>
    public class UmbracoVolatileAttribute : Attribute
    {

    }
}
