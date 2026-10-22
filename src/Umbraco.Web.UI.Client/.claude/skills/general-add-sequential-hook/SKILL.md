---
name: general-add-sequential-hook
description: Add or use a sequential hook (UmbSequentialHookController) where methods transform a value one after the other. Use only when the user explicitly needs other code to modify a value in a pipeline. The value type becomes public and nearly impossible to change later, so be extremely sceptical and confirm with the user before adding one.
allowed-tools: Read, Write, Edit, Grep, Glob
---

# Add or Use a Sequential Hook

`UmbSequentialHookController` (`@umbraco-cms/backoffice/hook-api`) passes a value through a list of methods. Each method receives the value returned by the previous one, ordered by weight (highest first), and `execute()` returns the final value.

## Read this first: a sequential hook is the most expensive kind of hook

Everything in the `general-add-parallel-hook` skill applies, **plus this**:

> **The value type is handed to third-party code that modifies and returns it.** That type, its shape, its invariants, and what consumers are allowed to do with it become public API. Changing it later is a breaking change for every package with a method on the hook, and it is almost never possible to do without breaking someone.

Why it is worse than a parallel hook:

- **The value type is a two-way contract.** You are not only promising what you send, you are promising to accept whatever comes back. Any consumer may return a changed, replaced, or even partly invalid value, and every later method and the owner must cope.
- **You cannot add required fields or narrow the type** without breaking consumers that build or return the value.
- **Order matters, so the order is public.** Weights and default ordering become something packages depend on, and your own later methods can change the result for them.
- **One bad method affects every other.** A method that throws, returns the wrong shape, or forgets to return the value breaks the whole chain. There is no isolation.
- **Consumers become coupled to each other** through the value, in ways you cannot see.
- **It is hard to reason about and test.** The result depends on which packages are installed.

### Rule of thumb

If you are not **certain** the value type can stay as it is for years, do not add a sequential hook. Treat adding one as an API design review, and **always ask the user to confirm** before writing it.

### Prefer, in this order

1. **No hook.** Does the owner really need to let others change the value, or does it only need to tell them about it?
2. **A parallel hook** with meta that identifies the thing, where the work is side effects. Not a transformation of the value.
3. **An extension type** with a defined, narrow API (a manifest and an API class) so the contract is explicit, versionable, and can be deprecated.
4. **A narrow value.** If a sequential hook is unavoidable, pass the smallest value that does the job, not a whole model.

## What you need from the user

Do not start without all of these:

1. **The concrete consumer** and exactly what it needs to change in the value.
2. **Why not parallel, an extension, or a direct call.**
3. **Confirmation that the value type is stable enough** to be public for the foreseeable future.
4. **The failure semantics** you want when a method throws.

## Step 1: Choose the value and meta types

```typescript
export interface UmbMyThingOutgoingHookMeta {
	entityType: string;
	unique: string;
}
```

- **Value type**: as small and as stable as possible. Prefer a dedicated narrow interface over an existing internal model. Do not reuse a generated API model or a model that is likely to evolve, as it freezes that model.
- **Meta type**: read-only information the methods may need. Identifiers only, minimal, and extended by inheritance when needed.
- Export both from the owner's `types.ts`, as types. Document them as public contract.
- Decide whether consumers should mutate the value or return a new one, and write it in the JSDoc. Mutating shared references is the usual source of surprises.

## Step 2: Add the hook to the owner

```typescript
import { UmbSequentialHookController } from '@umbraco-cms/backoffice/hook-api';

/**
 * Passes the value through all methods before ... Describe when it runs, what each method receives
 * and what it must return.
 */
public readonly outgoingHook = new UmbSequentialHookController<UmbMyThingModel, UmbMyThingOutgoingHookMeta>(this);
```

- `readonly`, constructed with `this`, and destroyed in the owner's `destroy()` before `super.destroy()`.
- JSDoc the timing, the expected return, and what happens on failure.

## Step 3: Execute it

```typescript
data = await this.outgoingHook.execute(data, { entityType, unique });
```

- Always use the **returned** value. Ignoring it makes the hook silently do nothing.
- `execute()` accepts a value or a promise of one.
- Decide what a thrown error means. A sequential hook stops at the first failure and the error propagates, so the owner must decide whether that fails the whole operation.
- Do not validate or trust the result blindly if later code depends on its shape. Consumers can return anything that satisfies the type, and the type may be wider than you meant.

## Step 4: Weights

- Without a weight, a method is added **after** all existing ones.
- A higher weight runs first. Equal weights run in the order added.
- Do not invent weight conventions in the owner. If ordering between packages matters, that is a sign the design is too coupled; document it as part of the contract if it really must exist.
- Methods added while executing run if they come after the current method, and are skipped by that execution if they come before.

## Using a hook (adding a method)

```typescript
if ('outgoingHook' in api && api.outgoingHook instanceof UmbSequentialHookController) {
	api.outgoingHook.add(async (data, meta) => {
		// change data
		return data; // always return the value
	}, weight);
}
```

- **Always return the value.** Forgetting to is the most common mistake.
- Do only what is necessary, and keep the method fast. It is on the path of everything that comes after it.
- Do not rely on other methods having run, unless the owner documents it.
- Keep a reference to the method if it needs removing with `remove()`.

## Interface changes

Exposing the hook on a public interface adds a member that external implementers may not have. Treat it as breaking, use `docs/deprecation.md` and the `general-deprecate-api` skill, and check external implementers first.

## Testing

Follow `docs/testing.md`. Test the rule: methods run in weight order, each receives the previous return value, methods added mid-execution follow the ordering rule, and a failure stops the chain. See `sequential-hook.controller.test.ts`. Add a test in the owner that a method can change the value, and that the owner copes with it.

## Checklist

- [ ] Parallel hook, extension, and no-hook options ruled out, with a concrete consumer
- [ ] **User has explicitly confirmed** the value and meta types as public, long-lived API
- [ ] Value type is narrow, not an internal or generated model
- [ ] Hook is `readonly`, constructed with `this`, and destroyed in `destroy()`
- [ ] JSDoc states timing, expected return value, and failure behaviour
- [ ] The result of `execute()` is used, and its shape is not trusted blindly
- [ ] Interface change (if any) handled as a potentially breaking change
- [ ] Tests cover order, value passing, and failure
- [ ] Compiles: `npm run compile`
