using System;

namespace Umbraco.Cms.Core
{
    /// <summary>
    /// Any class implementing this interface that is added to the httpcontext.items keys or values will be disposed of at the end of the request.
    /// </summary>
    // TODO: Once UmbracoContext no longer needs this (see TODO in UmbracoContext), this should be killed
    public interface IDisposeOnRequestEnd : IDisposable
    {
    }
}
