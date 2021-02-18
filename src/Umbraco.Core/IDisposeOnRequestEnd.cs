using System;

namespace Umbraco.Cms.Core
{
    /// <summary>
    /// Any class implementing this interface that is added to the httpcontext.items keys or values will be disposed of at the end of the request.
    /// </summary>
    public interface IDisposeOnRequestEnd : IDisposable
    {
    }
}
