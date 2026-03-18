using System.Reflection;
using Microsoft.Extensions.Options;
using Serilog.Core;
using Serilog.Events;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Diagnostics;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Core.Logging.Serilog.Enrichers;

/// <summary>
///     Enriches the log if there are ThreadAbort exceptions and will automatically create a minidump if it can
/// </summary>
public class ThreadAbortExceptionEnricher : ILogEventEnricher
{
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IMarchal _marchal;
    private CoreDebugSettings _coreDebugSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThreadAbortExceptionEnricher"/> class.
    /// </summary>
    /// <param name="coreDebugSettings">The monitor for core debug settings.</param>
    /// <param name="hostingEnvironment">The current hosting environment.</param>
    /// <param name="marchal">The marshal instance used for thread operations.</param>
    public ThreadAbortExceptionEnricher(
        IOptionsMonitor<CoreDebugSettings> coreDebugSettings,
        IHostingEnvironment hostingEnvironment,
        IMarchal marchal)
    {
        _coreDebugSettings = coreDebugSettings.CurrentValue;
        _hostingEnvironment = hostingEnvironment;
        _marchal = marchal;
        coreDebugSettings.OnChange(x => _coreDebugSettings = x);
    }

    /// <summary>
    /// Enriches the log event with additional information related to thread abort exceptions.
    /// This enrichment is applied only when the log event level is Error or Fatal.
    /// </summary>
    /// <param name="logEvent">The log event to enrich. If the event represents a thread abort exception at Error or Fatal level, additional properties are added.</param>
    /// <param name="propertyFactory">The factory used to create log event properties.</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        switch (logEvent.Level)
        {
            case LogEventLevel.Error:
            case LogEventLevel.Fatal:
                DumpThreadAborts(logEvent, propertyFactory);
                break;
        }
    }

    private static bool IsTimeoutThreadAbortException(Exception? exception)
    {
        if (exception is null || !(exception is ThreadAbortException abort))
        {
            return false;
        }

        if (abort.ExceptionState == null)
        {
            return false;
        }

        Type stateType = abort.ExceptionState.GetType();
        if (stateType.FullName != "System.Web.HttpApplication+CancelModuleException")
        {
            return false;
        }

        FieldInfo? timeoutField = stateType.GetField("_timeout", BindingFlags.Instance | BindingFlags.NonPublic);
        if (timeoutField == null)
        {
            return false;
        }

        return (bool?)timeoutField.GetValue(abort.ExceptionState) ?? false;
    }

    private void DumpThreadAborts(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Exception is null || !IsTimeoutThreadAbortException(logEvent.Exception))
        {
            return;
        }

        var message = "The thread has been aborted, because the request has timed out.";

        // dump if configured, or if stacktrace contains Monitor.ReliableEnter
        var dump = _coreDebugSettings.DumpOnTimeoutThreadAbort ||
                   IsMonitorEnterThreadAbortException(logEvent.Exception!);

        // dump if it is ok to dump (might have a cap on number of dump...)
        dump &= MiniDump.OkToDump(_hostingEnvironment);

        if (!dump)
        {
            message += ". No minidump was created.";
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ThreadAbortExceptionInfo", message));
        }
        else
        {
            try
            {
                var dumped = MiniDump.Dump(_marchal, _hostingEnvironment, withException: true);
                message += dumped
                    ? ". A minidump was created in App_Data/MiniDump."
                    : ". Failed to create a minidump.";
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ThreadAbortExceptionInfo", message));
            }
            catch (Exception ex)
            {
                message = "Failed to create a minidump. " + ex;
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ThreadAbortExceptionInfo", message));
            }
        }
    }

    private static bool IsMonitorEnterThreadAbortException(Exception exception)
    {
        if (!(exception is ThreadAbortException abort))
        {
            return false;
        }

        var stacktrace = abort.StackTrace;
        return stacktrace?.Contains("System.Threading.Monitor.ReliableEnter") ?? false;
    }
}
