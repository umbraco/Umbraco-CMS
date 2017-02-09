namespace Umbraco.Core.Events
{
    public enum EventsDispatchMode
    {
        // in 7.5 we'd do:
        //
        //   using (var uow = ...)
        //   { ... }
        //   Done.RaiseEvent(...);
        //
        // and so the event would trigger only *after* the transaction has completed,
        // so actually PassThrough is more aggressive than what we had in 7.5 and should
        // not be used. now in 7.6 we do:
        //
        //   using (var uow = ...)
        //   {
        //     ...
        //     uow.Events.Dispatch(Done, ...);
        //   }
        //
        // so the event can be collected, so the default "kinda compatible" more has to be
        // the Scope mode, and Passive is for Deploy only

        Unspecified = 0,
        PassThrough, // both Doing and Done trigger immediately
        Scope, // Doing triggers immediately, Done queued and triggered when & if the scope completes
        Passive // Doing never triggers, Done queued and needs to be handled by custom code
    }
}