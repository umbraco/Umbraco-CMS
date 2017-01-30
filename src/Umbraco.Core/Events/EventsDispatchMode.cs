namespace Umbraco.Core.Events
{
    public enum EventsDispatchMode
    {
        Unspecified = 0,
        PassThrough, // both Doing and Done trigger immediately
        Scope, // Doing triggers immediately, Done queued and triggered when & if the scope completes
        Passive // Doing never triggers, Done queued and needs to be handled by custom code
    }
}