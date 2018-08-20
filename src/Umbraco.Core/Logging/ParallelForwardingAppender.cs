using log4net.Core;
using log4net.Util;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// An asynchronous appender based on <see cref="BlockingCollection{T}"/>
    /// </summary>
    /// <remarks>
    /// Borrowed from https://github.com/cjbhaines/Log4Net.Async - will reference Nuget packages directly in v8
    /// </remarks>
    [Obsolete("Use the Log4Net.Async.ParallelForwardingAppender instead this will be removed in future versions")]
    public class ParallelForwardingAppender : Log4Net.Async.ParallelForwardingAppender
    {
    }
}