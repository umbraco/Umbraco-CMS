---
name: delete-dont-duplicate-when-one-caller-stays-sync
description: When retiring a sync IContentService member whose only remaining caller is itself sync and out of scope for this increment, don't keep the sync member alive alongside a new async sibling — delete it and bridge that one caller with .GetAwaiter().GetResult() instead, so there's only ever one implementation of the check.
metadata:
  type: feedback
  originSessionId: acc3673d-daf5-4c6f-9c2e-4a53931f01bd
  modified: 2026-08-21T14:05:31.796Z
---

Planning the `IsPathPublished` retirement, I found it had a real internal production caller
(`StrategyCanPublish`, deep inside the still-fully-sync `Publish`/`SaveAndPublish` orchestration engine —
itself explicitly out of scope for this increment). My instinct was to treat this as a blocker on full
retirement: leave the sync `IsPathPublished` in place (so `StrategyCanPublish` keeps compiling/working
unchanged) and just add a new `IsPathPublishedAsync` sibling alongside it for external callers — the
"can't delete it, something still needs it" reasoning.

The user corrected this: "We should delete the IsPathPublished sync member, and simply fake it on the
element service until we get there, use GetAwaiter/GetResult if necessary." i.e. a real internal sync
caller doesn't justify keeping a second, parallel implementation of the same logic (NPoco-backed sync,
EF-Core-backed async) alive — it means that ONE caller needs to bridge to the single (now-async)
implementation via `.GetAwaiter().GetResult()`, at that specific call site, same as any other sync
context that needs to call into async code in this codebase.

**Why:** Two implementations of "is this path published" (one hitting NPoco, one hitting EF Core) is a
correctness and maintenance liability going forward — they could drift, and every future bugfix has to be
applied twice. A sync caller that can't be made async yet is a normal, already-common situation in this
codebase (see the many other `.GetAwaiter().GetResult()` bridges already in `ContentTelemetryProvider`,
`ContentEventsTests.cs`'s `Children()` helper, etc.) — it's not a reason to duplicate logic, just a reason
to bridge at that one boundary.

**Also clarified via `AskUserQuestion`**: "fake it on the element service" did NOT mean adding a new stub
implementation to `ElementService`/`PublishableContentServiceBase<TContent>` (a separate, older base class
still fully sync, used only by Element, with its own independent copy of the same method). It meant: leave
that completely separate class alone — its own sync copy stays exactly as-is, untouched, as the "fake"/
placeholder until Element itself gets migrated to the async base in a future increment. Don't hoist the
member onto a shared interface just because two classes happen to have similarly-named methods, if doing
so would force an unrelated, not-yet-migrated class to grow a stub it doesn't otherwise need.

**How to apply:** When a sync member slated for retirement has a real internal caller that must itself
stay sync (because it's part of larger orchestration logic that's out of scope this increment), default to
deleting the sync member and bridging that specific call site with `.GetAwaiter().GetResult()` — do not
keep the old sync implementation alive as a parallel path just to avoid touching that one caller. Only
keep a sync member alive if deleting it would require migrating something genuinely out of scope (e.g. the
orchestration method itself), which bridging at the call site specifically avoids needing to do.
