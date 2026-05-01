# Core Primitives

The foundational building blocks that all backoffice code is built on. These live in `src/libs/` and are framework-agnostic (no CMS domain knowledge).

---

## UmbLitElement

**The base class for all web components in the backoffice.** Never extend `LitElement` directly.

```typescript
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-my-element')
export class UmbMyElement extends UmbLitElement {
	// All primitives below are available on `this`
}
```

UmbLitElement extends Lit's `LitElement` with core capabilities:

### Observe a State

Lifecycle-managed observable subscription. Automatically unsubscribes when the element disconnects from the DOM — no manual cleanup needed.

```typescript
// Basic usage — subscribe to an observable, update @state
this.observe(context.items, (items) => {
	this._items = items;
});

// With explicit alias — allows replacement or removal by name
this.observe(context.items, (items) => {
	this._items = items;
}, '_observeItems');

// Later, stop observing by alias:
this.removeUmbControllerByAlias('_observeItems');

// Combining multiple observables (emits when ANY source changes)
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';

this.observe(
	observeMultiple([context.items, context.filter]),
	([items, filter]) => {
		this._items = items;
		this._filter = filter;
	},
	'_observeItemsAndFilter',
);
```

**Alias behavior:**

- If omitted and callback exists: auto-generated hash of the callback function — guards against accidental duplicate subscriptions when the same `observe(...)` line runs more than once
- If `null`: no alias — use this when the call site is one-shot (e.g., set up once in a constructor and never invoked again). Intentional; not a missing-alias smell.
- If provided: explicit string/symbol — re-running the same `observe(...)` line replaces the existing controller with the same alias, and you can remove it actively via `removeUmbControllerByAlias('_observeItems')`

### Retrieve a Context

The choice between consumeContext and getContext depends on how the code uses the context.
If you only need a context during user interactions or events, use getContext.
Use consumeContext when the context is a primary dependency that must stay up to date.

#### consumeContext(alias, callback)

Subscribe to a context provided by an ancestor element. The callback fires when the context becomes available (and re-fires if the context changes).

```typescript
#myContext?: typeof UMB_MY_CONTEXT.TYPE;

constructor() {
	super();
	this.consumeContext(UMB_MY_CONTEXT, (context) => {
		this.#myContext = context;
		// Set up observations on the context here
		this.observe(context.items, (items) => {
			this._items = items;
		});
	});
}
```

#### getContext(alias, options?)

Promise-based one-time context retrieval. Use for user-triggered actions where you don't need continuous observation.

```typescript
async #handleClick() {
	const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
	notificationContext?.peek('positive', { data: { message: 'Done!' } });
}
```

### Provide a Context

Make a context instance available to all descendant elements by extending UmbContextBase.

```typescript
class UmbMyContext extends UmbContextBase {
	constructor(host: UmbControllerHost) {
		super(host, UMB_MY_CONTEXT);
	}
}
```

### Localization

Built-in localization controller. Available on every UmbLitElement.

```typescript
// In templates
html`<span>${this.localize.term('general_close')}</span>`

// In code
const label = this.localize.term('content_createBlueprintFrom');
```

---

## Observable State

Reactive state containers built on RxJS `BehaviorSubject`. **All `Umb*State` classes only emit when the new value differs from the current value** — observers are not invoked on `setValue(x)` when the state already holds `x`. This means you do not need to write "is this a redundant re-emit?" guards inside observer callbacks.

| Class | Holds | Comparison |
|-------|-------|-----------|
| `UmbStringState` / `UmbNumberState` / `UmbBooleanState` | Primitives | `===` |
| `UmbObjectState` / `UmbArrayState` / `UmbDeepState` | Plain JSON data, deep-frozen | `JSON.stringify` deep compare |
| `UmbClassState` | Class instance with `.equal()` | `oldValue.equal(newValue)` (when both non-null) |
| `UmbBasicState` | Anything | `===` (reference) |

```typescript
import { UmbStringState, UmbArrayState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

// Private state, public read-only observable
#name = new UmbStringState<string>('');
readonly name = this.#name.asObservable();
this.#name.setValue('new name');
```

For per-type comparison details, change-detection caveats (`UmbBasicState` holding objects, `UmbClassState` with `undefined`), `asObservablePart` memoization, mute/unmute, and the rationale for not writing emission guards in observers, see **[State System](./state-system.md)**.

---

## Context API

Framework-agnostic dependency injection via DOM events. Contexts flow **down** the DOM tree from provider to consumer.

### UmbContextToken

Type-safe identifier for a context. The generic parameters flow through to `consumeContext()` and `getContext()` return types.

```typescript
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

// Simple token
export const UMB_MY_CONTEXT = new UmbContextToken<UmbMyContext>('UmbMyContext');

// Token with discriminator (for type narrowing)
export const UMB_MY_SPECIFIC_CONTEXT = new UmbContextToken<UmbMyBaseContext, UmbMySpecificContext>(
	'UmbMyContext',          // contextAlias — matches the provider alias
	'UmbMySpecificContext',  // apiAlias — distinguishes variant
	(context): context is UmbMySpecificContext => context instanceof UmbMySpecificContext,
);
```

**The `.TYPE` property** gives you the resolved type for variable declarations:

```typescript
#myContext?: typeof UMB_MY_CONTEXT.TYPE;
```

### How It Works

1. **Provider** registers a context instance on a host element
2. **Consumer** dispatches a DOM event that bubbles up the tree
3. When the event reaches a provider with a matching alias, the provider calls back with the instance
4. The consumer's callback fires with the typed context

### Creating a Context

```typescript
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MY_CONTEXT = new UmbContextToken<UmbMyContext>('UmbMyContext');

export class UmbMyContext extends UmbContextBase<UmbMyContext> {
	// State, observables, methods...

	constructor(host: UmbControllerHost) {
		super(host, UMB_MY_CONTEXT);
	}
}
```

`UmbContextBase` automatically calls `provideContext()` in its constructor, making the context available to descendants.

### Common Built-in Contexts

```typescript
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
```

---

## Controller Pattern

Lifecycle-aware logic units that attach to a host (element or other controller). Controllers implement `UmbController` and are managed by the host's controller system.

### UmbControllerBase

Base class for non-element controllers. Has the same `observe()`, `consumeContext()`, `provideContext()`, and `getContext()` methods as `UmbLitElement`.

```typescript
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbMyController extends UmbControllerBase {
	constructor(host: UmbControllerHost) {
		super(host, 'my-controller-alias'); // alias is optional
	}

	// Has full access to observe(), consumeContext(), etc.
}
```

### Controller Lifecycle

```
host.addUmbController(ctrl)  →  ctrl.hostConnected()
                                       ↓
                              (subscriptions active, DOM events listening)
                                       ↓
host disconnects from DOM     →  ctrl.hostDisconnected()
                                       ↓
                              (subscriptions paused)
                                       ↓
host reconnects to DOM        →  ctrl.hostConnected()
                                       ↓
host.removeUmbController(ctrl) → ctrl.destroy()
```

### Controller Aliases

When a controller is added with an alias that already exists on the host, the **old controller is destroyed and replaced**. This is how `observe()` avoids duplicate subscriptions when called multiple times:

```typescript
// First call: creates observer with alias '_observeItems'
this.observe(context.items, (items) => { ... }, '_observeItems');

// Second call: destroys previous '_observeItems' observer, creates new one
this.observe(context.items, (items) => { ... }, '_observeItems');
```



---

## Quick Reference

| Primitive | Import Path | Purpose |
|-----------|-------------|---------|
| `UmbLitElement` | `@umbraco-cms/backoffice/lit-element` | Base class for all elements |
| `UmbControllerBase` | `@umbraco-cms/backoffice/class-api` | Base class for non-element controllers |
| `UmbContextBase` | `@umbraco-cms/backoffice/class-api` | Base class for contexts (auto-provides) |
| `UmbContextToken` | `@umbraco-cms/backoffice/context-api` | Type-safe context identifier |
| `UmbStringState` | `@umbraco-cms/backoffice/observable-api` | String state container |
| `UmbNumberState` | `@umbraco-cms/backoffice/observable-api` | Number state container |
| `UmbBooleanState` | `@umbraco-cms/backoffice/observable-api` | Boolean state container |
| `UmbArrayState` | `@umbraco-cms/backoffice/observable-api` | Array state container (requires unique key) |
| `UmbObjectState` | `@umbraco-cms/backoffice/observable-api` | Object state container |
| `observeMultiple` | `@umbraco-cms/backoffice/observable-api` | Combine multiple observables |
