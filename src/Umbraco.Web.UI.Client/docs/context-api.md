# Context API

DOM-event-based dependency injection. A provider exposes a context to its descendants; a consumer requests it with a typed `UmbContextToken`. For the element methods (`consumeContext`, `getContext`, `provideContext`), see [Core Primitives](./core-primitives.md#retrieve-a-context).

---

## Token Anatomy

Source: `src/libs/context-api/token/context-token.ts`

```typescript
new UmbContextToken<BaseType, ResultType>(contextAlias, apiAlias?, discriminator?);
```

| Part | Role |
| ---- | ---- |
| `contextAlias` | The slot a provider answers for. |
| `apiAlias` | Must equal the provider's (default `'default'`). Not a way to narrow a token; leave it `undefined`. |
| `discriminator` | Type guard on the found instance. Narrows the type and rejects instances that don't fit. |

`typeof UMB_MY_CONTEXT.TYPE` gives the resolved type.

---

## One Instance, Many Tokens

Tokens sharing a `contextAlias` resolve to the **same instance**, differing only in how specific a view they take. About 50 tokens share `'UmbWorkspaceContext'`:

```typescript
UMB_WORKSPACE_CONTEXT                // any workspace
UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT  // entity detail workspaces
UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT  // the document type workspace
```

`UmbDocumentTypeWorkspaceContext` extends `UmbContentTypeWorkspaceContextBase`, which extends `UmbEntityDetailWorkspaceContextBase`, so the most specific token gives you everything the less specific ones do.

**Consume once, with the most specific token you need.** Use a less specific token only when the code must work across many workspaces.

```typescript
// ❌ Two consumers for one instance
this.consumeContext(UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT, (ctx) => { this.#ctx = ctx; });
this.consumeContext(UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT, (ctx) => { /* use ctx */ });

// ✅ One consumer
this.consumeContext(UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT, (ctx) => {
	this.#ctx = ctx;
	/* use ctx */
});
```

---

## Resolution

Source: `context-consumer.ts`, `context-provider.ts`

- The request stops at the **first provider with a matching `contextAlias`** (`passContextAliasMatches()` turns this off).
- If the discriminator rejects that instance, the consumer gets nothing. It does not keep searching ancestors.
- The callback re-fires when the context is replaced, and receives `undefined` when it goes away.
- `skipHost()` starts the search at the host's parent.

---

## Looking Up a Token

Read the source before consuming; don't guess from the name.

```bash
grep -rn "export const UMB_X_CONTEXT\b" src -A8          # alias, discriminator, result type
grep -rln "'UmbWorkspaceContext'" src --include='*.context-token.ts'   # tokens on the same alias
grep -rn "class UmbX\b" src -A3                           # base classes of the provider
```

Check that the member you need exists on the token's result type, and that you aren't already consuming the same instance through another token.

---

## Creating a Context

```typescript
export const UMB_MY_CONTEXT = new UmbContextToken<UmbMyContext>('UmbMyContext');

export class UmbMyContext extends UmbContextBase<UmbMyContext> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_MY_CONTEXT);
	}
}
```

`UmbContextBase` provides the context on construction.

- A more specific form of an existing slot (e.g. a workspace for one entity type): reuse the parent's `contextAlias` with a discriminator. See [Workspaces](./workspaces.md#context-token-pattern).
- A separate context: new `contextAlias`.
