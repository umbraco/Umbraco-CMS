---
name: feedback-async-named-classes-stay-pure
description: "A class or interface named \"Async*\" (e.g. AsyncPublishableContentServiceBase) must contain zero sync surface, even a one-line delegating bridge, even when a legacy/shared interface requires a sync member to exist somewhere"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: c61c9c25-746f-4318-b3b3-9e5cf5da0d87
  modified: 2026-09-14T08:25:19.416Z
---

When a sync interface member must still be implemented somewhere because it's shared with an
unmigrated sibling (e.g. `IContentServiceBase.CheckDataIntegrity`, required by `IMediaService`/
`IMemberService`, which have no async repository), do **not** place even a trivial one-line
`.GetAwaiter().GetResult()` bridge on the migrated entity's own class (`ContentService`) or on any
class/interface with "Async" in its name (`AsyncPublishableContentServiceBase`,
`IAsyncContentServiceBase`, etc.). Push the bridge to the interface layer instead, via an **explicit
interface reabstraction** declared directly on the entity-specific top interface (`IContentService`,
not "Async"-named): `ReturnType IAncestorInterface.Member(...) => AsyncMember(...).GetAwaiter().GetResult();`.

**Why**: user rejected two successive placements before landing here — first the bridge on
`ContentService.cs` itself ("not too fond of ContentService still implementing
CheckDataIntegrity... the rest is spillover from to-do repositories like media"), then the bridge
moved to the shared `AsyncPublishableContentServiceBase` class ("It's still in the async base class,
we should remove it entirely from the async* chain"). The goal: a class/interface named "Async*"
should be legible at a glance as 100% async — no sync surface at all, not even a delegating shim,
regardless of why the shim exists.

**How to apply**: any future retirement-campaign increment where the sync member being retired is
declared on a WIDE shared interface (e.g. `IContentServiceBase`, not the narrower
`IPublishableContentService<TContent>`) — meaning it can't just be deleted because Media/Member still
need it — reach for this pattern immediately rather than trying the "bridge on the base class"
approach first. See [[project_document_repository_retirement_plan]]'s "Session of 2026-09-14:
CheckDataIntegrity retired" section for the full worked example, including a C# gotcha: a plain
redeclaration-with-default-body on the derived interface does NOT satisfy the ancestor's requirement
(verified with a minimal repro) — only the explicit `ReturnType IAncestor.Member(...)` syntax inside
the derived interface actually works.

**One consequence to flag when applying this elsewhere**: the resulting member is only reachable
through the interface type, not through a concrete-class-typed reference (explicit interface
implementations are never part of a class's own public surface). Check for any caller holding a
concrete-class-typed reference before assuming this pattern is a drop-in.
