# Display Batch Resolver

**Module**: `@umbraco-cms/backoffice/display-batch-resolver`
**Location**: `src/packages/core/display-batch-resolver/`

---

Shared infrastructure for resolving the values that elements display. Elements in the DOM each register the value they render (for example one per table cell) with a coordinator context above them. The coordinator turns every registration within the same tick into one call per resolver, and each element observes its own resolved value.

Used by: [Value Summary](./value-summary.md).

## Resolver contract

```typescript
interface UmbDisplayBatchResolver<TValue, TResolved> extends UmbApi {
	resolveValues(values: ReadonlyArray<TValue>): Promise<UmbDisplayBatchResolveResult<TResolved>>;
}

interface UmbDisplayBatchResolveResult<TResolved> {
	data: ReadonlyArray<TResolved>; // positional — same order and length as input
	asObservable?: () => Observable<ReadonlyArray<TResolved>>;
}
```

- `data` must be the **same length** as `values` and **positionally aligned**.
- Provide `asObservable` only when resolved data can change after the initial load (for example, linked entities that can be renamed).
- **Return models, not display strings**, for anything that can vary by culture. The element rendering the value picks the name, for example through an item data resolver bound to its own host, so it follows the variant context it sits in.

A resolver is referenced through `UmbDisplayBatchResolverLoaderProperty`: a class, or a dynamic import of a module that exports the class as `valueResolver` (preferred) or `default`.

## Coordinator

`UmbDisplayBatchResolverCoordinatorContext` is the shared class. It is constructed with a host, a context token and a lookup:

```typescript
new UmbDisplayBatchResolverCoordinatorContext(host, MY_COORDINATOR_CONTEXT, (key) => findResolverFor(key));
```

- `register(key, value)` queues a value. Values registered within the same tick are deduplicated and resolved in one call per key.
- `observeResolvedValue(key, value)` emits the resolved value, or `undefined` until it has been resolved.
- The lookup returns the `UmbDisplayBatchResolverLoaderProperty` for a key. When it returns `undefined`, values for that key are **passed through unchanged**.
- One resolver instance is created per key and hosted by the coordinator. Subclasses can override `_createResolver(key, ResolverConstructor)` to give the resolver what it needs to know about its key. For example, the filter coordinator sets the filter's `manifest` on its resolver.
- Resolved values are kept for the **lifetime of the coordinator**. Create it on a context whose lifetime fits, such as a collection context.

## One instance per owner

Each owner runs its **own** coordinator instance, with its own context token and its own lookup. Keys only need to be unique within that owner, and owners don't share a cache. Fetches still meet in the repository's item store.

Each owner also names its own resolver interface, extending `UmbDisplayBatchResolver`, so implementations use the owner's vocabulary (for example `UmbValueSummaryResolver`). Implementations implement the owner's interface, not `UmbDisplayBatchResolver` directly.

Subclass the coordinator when the owner has a fixed token and lookup. `UmbValueSummaryCoordinatorContext` is the reference: the keys are value types, and the lookup reads `valueResolver` from the `valueSummary` manifest registered for that value type.

## Rules

1. Resolver `data` must be positionally aligned with `values`.
2. Return models, not culture-dependent display strings.
3. Use `asObservable` only when resolved data can change.
4. Export the resolver class as `valueResolver` (named export).
5. Don't share a coordinator instance between owners. Create a new instance with its own token and lookup.
6. Resolver implementations implement their owner's interface (for example `UmbValueSummaryResolver`), not `UmbDisplayBatchResolver` directly.
