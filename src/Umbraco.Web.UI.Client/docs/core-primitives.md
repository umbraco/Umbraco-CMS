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

UmbLitElement extends Lit's `LitElement` with four core capabilities:

### observe(source, callback?, controllerAlias?)

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
- If omitted and callback exists: auto-generated hash of the callback function
- If `null`: no alias (controller cannot be replaced by alias)
- If provided: explicit string/symbol for later reference

### consumeContext(alias, callback)

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

### getContext(alias, options?)

Promise-based one-time context retrieval. Use for user-triggered actions where you don't need continuous observation.

```typescript
async #handleClick() {
	const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
	notificationContext?.peek('positive', { data: { message: 'Done!' } });
}
```

### provideContext(alias, instance)

Make a context instance available to all descendant elements.

```typescript
constructor() {
	super();
	this.provideContext(UMB_MY_CONTEXT, new UmbMyContext(this));
}
```

### this.localize

Built-in localization controller. Available on every UmbLitElement.

```typescript
// In templates
html`<span>${this.localize.term('general_close')}</span>`

// In code
const label = this.localize.term('content_createBlueprintFrom');
```

---

## Observable State

Reactive state containers built on RxJS `BehaviorSubject`. All state classes emit the current value immediately on subscription, then emit on every change.

### State Types

| Class | For | Change Detection |
|-------|-----|-----------------|
| `UmbStringState` | Strings | `!==` (reference) |
| `UmbNumberState` | Numbers | `!==` (reference) |
| `UmbBooleanState` | Booleans | `!==` (reference) |
| `UmbObjectState` | Objects | JSON deep comparison + deep freeze |
| `UmbArrayState` | Arrays | JSON deep comparison + deep freeze |
| `UmbClassState` | Class instances | `.equal()` method on the class |
| `UmbDeepState` | Any (base for Object/Array) | JSON deep comparison + deep freeze |

### Basic Usage

```typescript
import { UmbStringState, UmbArrayState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

// Create state (private)
#name = new UmbStringState('');
#items = new UmbArrayState<MyItem>([], (item) => item.unique);
#data = new UmbObjectState<MyModel | undefined>(undefined);

// Expose as observable (public, read-only)
readonly name = this.#name.asObservable();
readonly items = this.#items.asObservable();
readonly data = this.#data.asObservable();

// Read current value
const currentName = this.#name.getValue();

// Update value
this.#name.setValue('new name');
```

### UmbArrayState

Requires a unique-key function for identity tracking. Provides rich array manipulation methods.

```typescript
#items = new UmbArrayState<MyItem>([], (item) => item.unique);

// Append
this.#items.appendOne(newItem);
this.#items.append([item1, item2]);

// Remove
this.#items.removeOne('unique-id');
this.#items.remove(['id1', 'id2']);

// Update a single item (partial merge)
this.#items.updateOne('unique-id', { name: 'Updated' });

// Filter
this.#items.filter((item) => item.active);

// Sort
this.#items.sortBy((a, b) => a.name.localeCompare(b.name));

// Clear
this.#items.clear();

// Check existence
this.#items.getHasOne('unique-id'); // boolean
```

### UmbObjectState

```typescript
#data = new UmbObjectState<MyModel>({ name: '', count: 0 });

// Partial update (shallow merge)
this.#data.update({ name: 'Updated' }); // count stays 0
```

### Derived Observables (asObservablePart)

Create an observable that only emits when a specific derived value changes. Uses memoization to avoid unnecessary updates.

```typescript
// Only emits when the name changes, not when other properties change
readonly name = this.#data.asObservablePart((data) => data?.name);

// Count of items
readonly itemCount = this.#items.asObservablePart((items) => items.length);

// First item
readonly firstItem = this.#items.asObservablePart((items) => items[0]);
```

### Deep Freeze

`UmbDeepState`, `UmbObjectState`, and `UmbArrayState` deep-freeze their data. Attempting to mutate frozen data throws an error in development. This enforces immutable update patterns:

```typescript
// WRONG — throws TypeError in development
const items = this.#items.getValue();
items.push(newItem); // Frozen!

// CORRECT — create new array
this.#items.appendOne(newItem);
// or
this.#items.setValue([...this.#items.getValue(), newItem]);
```

### Mute/Unmute

`UmbDeepState` (and its subclasses) support muting to batch multiple updates without emitting intermediate values:

```typescript
this.#data.mute();
this.#data.update({ name: 'a' });
this.#data.update({ count: 1 });
this.#data.unmute(); // Single emission with both changes
```

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
