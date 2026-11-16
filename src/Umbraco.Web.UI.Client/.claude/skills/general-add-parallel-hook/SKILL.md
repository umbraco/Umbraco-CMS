---
name: general-add-parallel-hook
description: Add or use a parallel hook (UmbParallelHookController) so other code can run work at a defined point in a context or class's lifecycle. Use when the user says "add a hook", "let other code run something during load/init/save", or when an extension point is needed that is not a manifest. Be very sceptical before adding one — every hook and its meta type becomes public API.
allowed-tools: Read, Write, Edit, Grep, Glob
---

# Add or Use a Parallel Hook

`UmbParallelHookController` (`@umbraco-cms/backoffice/hook-api`) lets a class expose a point in its lifecycle where other code can add methods. All methods run at the same time, and `execute()` resolves when they have all finished.

Existing example: `initHook` and `incomingDataHook` on `UmbEntityDetailWorkspaceContextBase`.

## Be very sensible about adding hooks

A hook is **public API the moment it is exported**, and it is hard to take back. Adding one is a design decision, not a convenience.

What becomes a contract when you add a hook:

- **The property itself** (`readonly myHook`) on the class or interface.
- **The meta type**, every field of it. Fields can be added later, never removed or changed.
- **When it fires.** Consumers rely on the timing (before data is processed, before the first render, etc.).
- **What waits for it.** If `execute()` is awaited on a critical path (loading, saving), a slow or failing method blocks that path for everyone.
- **Failure behaviour.** If any method throws, `execute()` throws the first failure once all have finished. Whoever calls `execute()` decides what that means for the user.

### Before adding a hook, rule these out first

1. **Can it be an extension?** A manifest, condition, kind, or an existing extension type may already cover it. Prefer those, they have a deprecation story and are discoverable.
2. **Can the consumer observe state instead?** If the need is "react when X changes", an observable already on the context is better than a hook.
3. **Can the owner just do the work?** A hook is for work the owner cannot know about. If it is known at design time, call it directly.
4. **Is there a real second consumer?** A hook with a single caller is usually just a function call in disguise. Do not add hooks for hypothetical use cases.

If you cannot name a concrete need that the alternatives do not cover, do not add the hook. When in doubt, **ask the user before adding**.

## Parallel vs. sequential

- **Parallel**: methods are independent, and only "all done" matters. Nothing is passed between them and nothing is returned. This is the safe default.
- **Sequential**: methods transform a value and pass it on. See the `general-add-sequential-hook` skill, and its warnings, before choosing it.

If the methods do not need each other's results, always choose parallel.

## What you need from the user

1. **The lifecycle point**: exactly when does the hook fire, and what is waiting on it?
2. **The concrete consumer**: who adds methods, and what do they do?
3. **The alternatives considered**: why not an extension, an observable, or a direct call?

## Step 1: Define the meta type

Keep it as small as possible. Meta is the only information handed to consumers, and every field is permanent.

```typescript
export interface UmbMyThingLoadHookMeta {
	entityType: string;
	unique: string | undefined;
}
```

- Put it in the owner's `types.ts` and export it as a type.
- Pass identifiers, not live objects or internal state. Do not expose anything that would stop you refactoring internals later.
- Extend via interface inheritance if a more specific owner needs more (see `UmbContentDetailIncomingDataHookMeta extends UmbEntityDetailIncomingDataHookMeta`).
- Do not use the meta to pass values back. A parallel hook has no return value. If you need that, you need a different design, not a sequential hook by reflex.

## Step 2: Add the hook to the owner

```typescript
import { UmbParallelHookController } from '@umbraco-cms/backoffice/hook-api';

/**
 * Awaited while ... Describe when it fires and what waits for it.
 */
public readonly loadHook = new UmbParallelHookController<UmbMyThingLoadHookMeta>(this);
```

- JSDoc the **timing and what is awaited**, in the vocabulary of the owner, not of the first consumer.
- `readonly`, and constructed with `this` as the controller host.
- Destroy it in the owner's `destroy()` before `super.destroy()`.

```typescript
override destroy() {
	this.loadHook.destroy();
	super.destroy();
}
```

## Step 3: Execute it

```typescript
await this.loadHook.execute({ entityType: this.getEntityType(), unique });
```

- Decide deliberately whether to `await` it, or run it alongside other work with `Promise.all`, as the entity detail workspace does with the data request.
- Decide what a failure means. Let it propagate, or catch and report, but do not leave it accidental.
- Methods added while executing start right away and are awaited by that execution, so do not assume a fixed set.

## Step 4: Expose it on the interface (if needed)

If other packages reach the hook through an interface (like `UmbContentWorkspaceContext`), add it there as `readonly`. Adding a member to a public interface is a breaking change for external implementers; follow `docs/deprecation.md` and the `general-deprecate-api` skill. Check whether the interface is implemented outside this repo before touching it.

## Using a hook (adding a method)

```typescript
if ('loadHook' in api && api.loadHook instanceof UmbParallelHookController) {
	api.loadHook.add(async (meta) => {
		// independent work
	});
}
```

- The `instanceof` check (see `workspace.element.ts`) keeps this safe against contexts that do not have the hook.
- Keep methods **fast and bounded**. They run on a path the user is waiting for. If a method can hang, add a timeout, as `workspace.element.ts` does.
- A method must not depend on another method having run, as order is not guaranteed.
- Keep a reference to the method if it needs removing. `remove()` does not affect executions that are already running.

## Testing

Follow `docs/testing.md`. Test the rule, not the scenario: all methods run, `execute()` waits for all of them, methods added mid-execution are awaited, and a failure is thrown after the others finish. See `parallel-hook.controller.test.ts` for the shape.

## Checklist

- [ ] Alternatives ruled out (extension, observable, direct call), and a concrete second consumer exists
- [ ] User has confirmed adding a new public hook
- [ ] Meta type is minimal, identifier-only, exported as a type
- [ ] Hook is `readonly`, constructed with `this`, and destroyed in `destroy()`
- [ ] JSDoc states when it fires and what waits for it
- [ ] `execute()` call has a deliberate await and failure strategy
- [ ] Interface change (if any) handled as a potentially breaking change
- [ ] Tests cover the contract
- [ ] Compiles: `npm run compile`
