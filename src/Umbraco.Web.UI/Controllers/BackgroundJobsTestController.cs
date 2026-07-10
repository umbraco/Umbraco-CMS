//using Microsoft.AspNetCore.Mvc;
//using Umbraco.Cms.Infrastructure.BackgroundJobs;
//using Umbraco.Cms.Infrastructure.HostedServices;
//using Umbraco.Cms.Web.UI.Custom.BackgroundJobs;

//namespace Umbraco.Cms.Web.UI.Controllers;

///// <summary>
///// Endpoints to manually trigger the temporary background jobs registered by
///// <c>BackgroundJobsTestComposer</c>. Pair with a tail of the logs to see the effects of each strategy.
///// </summary>
//public class BackgroundJobsTestController : Controller
//{
//    private readonly IRecurringBackgroundJobTrigger<TriggerableHeartbeatJob> _heartbeatTrigger;
//    private readonly IRecurringBackgroundJobTrigger<ManualOnlyJob> _manualOnlyTrigger;

//    public BackgroundJobsTestController(
//        IRecurringBackgroundJobTrigger<TriggerableHeartbeatJob> heartbeatTrigger,
//        IRecurringBackgroundJobTrigger<ManualOnlyJob> manualOnlyTrigger)
//    {
//        _heartbeatTrigger = heartbeatTrigger;
//        _manualOnlyTrigger = manualOnlyTrigger;
//    }

//    /// <summary>
//    /// Triggers <see cref="TriggerableHeartbeatJob" /> with <see cref="NextExecutionStrategy.None" /> —
//    /// the original schedule is kept; the original next tick may be skipped if the triggered run overshoots it.
//    /// </summary>
//    [HttpGet("/test/background-jobs/heartbeat/none")]
//    public IActionResult HeartbeatNone()
//        => Ok(new { triggered = _heartbeatTrigger.TriggerExecution() });

//    /// <summary>
//    /// Triggers <see cref="TriggerableHeartbeatJob" /> with <see cref="NextExecutionStrategy.Reset" /> —
//    /// waits a full Period after the triggered execution completes.
//    /// </summary>
//    [HttpGet("/test/background-jobs/heartbeat/reset")]
//    public IActionResult HeartbeatReset()
//        => Ok(new { triggered = _heartbeatTrigger.TriggerExecution(NextExecutionStrategy.Reset) });

//    /// <summary>
//    /// Triggers <see cref="TriggerableHeartbeatJob" /> with <see cref="NextExecutionStrategy.Replace" /> —
//    /// the triggered execution replaces the next scheduled tick; the following tick is one Period after the
//    /// originally-scheduled time.
//    /// </summary>
//    [HttpGet("/test/background-jobs/heartbeat/replace")]
//    public IActionResult HeartbeatReplace()
//        => Ok(new { triggered = _heartbeatTrigger.TriggerExecution(NextExecutionStrategy.Replace) });

//    /// <summary>
//    /// Triggers <see cref="TriggerableHeartbeatJob" /> with a custom delay (in seconds) before the next execution.
//    /// </summary>
//    [HttpGet("/test/background-jobs/heartbeat/delay/{seconds:int}")]
//    public IActionResult HeartbeatDelay(int seconds)
//        => Ok(new { triggered = _heartbeatTrigger.TriggerExecution(TimeSpan.FromSeconds(seconds)) });

//    /// <summary>
//    /// Triggers <see cref="ManualOnlyJob" /> — the only way this job runs, since Period and Delay are both
//    /// <see cref="Timeout.InfiniteTimeSpan" />.
//    /// </summary>
//    [HttpGet("/test/background-jobs/manual")]
//    public IActionResult Manual()
//        => Ok(new { triggered = _manualOnlyTrigger.TriggerExecution() });
//}
