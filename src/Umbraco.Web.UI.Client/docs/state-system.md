# State System

Reactive state containers built on RxJS `BehaviorSubject`. The canonical way to hold and expose mutable data in contexts, controllers, and elements.

For the broader observable lifecycle (`this.observe(...)`, controller cleanup, `observeMultiple`), see [core-primitives.md](./core-primitives.md).

---


## State Types

| Class | Holds | Comparison |
|-------|-------|-----------|
| `UmbStringState` / `UmbNumberState` / `UmbBooleanState` | Primitives | `===` |
| `UmbObjectState` / `UmbArrayState` / `UmbDeepState` | Plain JSON data, deep-frozen | `JSON.stringify` deep compare |
| `UmbClassState` | Class instance with `.equal()` | `===` first, falling back to `oldValue.equal(newValue)` |
| `UmbBasicState` | Anything | `===` |

`UmbObjectState`/`UmbArrayState`/`UmbDeepState` deep-freeze their value — mutate by replacing, never in place. Use `UmbClassState` for class instances; `JSON.stringify` strips prototypes.

Comparison determines whether a new value differs from the current one — subscribers are only notified when it does, so observer callbacks don't need their own re-emit guards.

---

## Standard Pattern

Private state, public observable, immutable updates:

```typescript
#name = new UmbStringState<string>('');
#items = new UmbArrayState<MyItem>([], (item) => item.unique);
#data = new UmbObjectState<MyModel | undefined>(undefined);

readonly name = this.#name.asObservable();
readonly items = this.#items.asObservable();
readonly data = this.#data.asObservable();

this.#name.setValue('new name');
this.#data.update({ count: 1 }); // shallow merge
this.#items.appendOne(newItem);
```

---

## `asObservablePart`

Available on every state class. Project to a sub-value with its own deduplication layer:

```typescript
readonly name = this.#data.asObservablePart((data) => data?.name);
readonly itemCount = this.#items.asObservablePart((items) => items.length);
```

Default memoization handles both objects (deep-equal via JSON) and primitives (`===`). Pass a custom memoization function as the second arg when the default is too coarse or too fine.

---

## Mute / Unmute (UmbDeepState only)

Batch multiple writes into a single emission:

```typescript
this.#data.mute();
this.#data.update({ name: 'a' });
this.#data.update({ count: 1 });
this.#data.unmute(); // single emission
```

Available on `UmbDeepState`, `UmbObjectState`, `UmbArrayState`. `getMutePromise()` resolves on the next `unmute`.

---

All state classes are imported from `@umbraco-cms/backoffice/observable-api`.
