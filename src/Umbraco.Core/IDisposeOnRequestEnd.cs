using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Core
{
    /// <summary>
    /// Any class implementing this interface that is added to the httpcontext.items keys or values will be disposed of at the end of the request.
    /// </summary>
    public interface IDisposeOnRequestEnd : IDisposable
    {
    }
}
