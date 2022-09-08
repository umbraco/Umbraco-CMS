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

    public ThreadAbortExceptionEnricher(
        IOptionsMonitor<CoreDebugSettings> coreDebugSettings,
        IHostingEnvironment hostingEnvironment, IMarchal marchal)
    {
        _coreDebugSettings = coreDebugSettings.CurrentValue;
        _hostingEnvironment = hostingEnvironment;
        _marchal = marchal;
        coreDebugSettings.OnChange(x => _coreDebugSettings = x);
    }

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

    private static bool IsTimeoutThreadAbortException(Exception exception)
    {
        if (!(exception is ThreadAbortException abort))
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
        if (!IsTimeoutThreadAbortException(logEvent.Exception))
        {
            return;
        }

        var message = "The thread has been aborted, because the request has timed out.";

        // dump if configured, or if stacktrace contains Monitor.ReliableEnter
        var dump = _coreDebugSettings.DumpOnTimeoutThreadAbort ||
                   IsMonitorEnterThreadAbortException(logEvent.Exception);

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
