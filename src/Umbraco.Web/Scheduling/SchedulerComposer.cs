using System;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Scheduling
{
    /// <summary>
    /// Used to do the scheduling for tasks, publishing, etc...
    /// </summary>
    /// <remarks>
    /// All tasks are run in a background task runner which is web aware and will wind down
    /// the task correctly instead of killing it completely when the app domain shuts down.
    /// </remarks>
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public sealed class SchedulerComposer : ComponentComposer<SchedulerComponent>, ICoreComposer
    { }
}
