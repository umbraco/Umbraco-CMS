# Visual Editor Tidy-Up Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Harden, simplify, and document the visual editor branch (`feature/visual-editor`) per the approved spec at `docs/plans/2026-06-11-visual-editor-tidy-up-design.md` — no new feature phases.

**Architecture:** Frontend work happens in the embedded document-workspace view (`src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/`) and the guest script (`src/Umbraco.Web.UI.Client/src/apps/visual-editor/injected.ts`). Backend work touches `Umbraco.Web.Common` (block list template extensions, guest script tag helper) and the `Umbraco.Web.UI` block list partial. The 1,210-line workspace view element is reduced by extracting a SignalR controller, a property-structure resolver, and a postMessage router into sibling files.

**Tech Stack:** TypeScript + Lit (backoffice client), C# / ASP.NET Core Razor (server). Working directory for ALL tasks: `D:/CMS/Umbraco-CMS/.worktrees/feature-visual-editor`.

**Testing note:** The approved spec scopes automated tests OUT of this round (no test harness exists for the visual editor surface; E2E coverage is recorded as future work). Verification per task = compile (`npm run build` for TS, `dotnet build` for C#), plus a manual smoke pass in the final task. This is a deliberate, spec-approved deviation from TDD.

**Verified facts this plan relies on** (checked against source, not the audit summaries):
- The hybrid opt-in/opt-out filter exists ONLY in `#resolveBlockPropertyStructures` (element lines 565–579). `#fetchPropertyStructures` (document properties) has no filter and must stay unfiltered — `#propertyStructures` doubles as general metadata for block-config lookup.
- `UmbracoViewPage` exposes `protected IUmbracoContext? UmbracoContext` (line 84) — usable from the Razor partial.
- `GetBlockListHtmlAsync(model)` returns `HtmlString.Empty` when `model?.Count == 0`, and `blocklist/default.cshtml` early-returns when empty — empty lists currently render nothing.
- The guest script's `PROP_SELECTOR` is `[data-umb-property]` — the list container must use a different attribute (`data-umb-block-property`) to avoid becoming a clickable property region.
- `#suppressRefresh` (element line 60) is declared and read but never written — dead state, removed in Task 4.
- `VisualEditorPropertyTracker` already has class-level XML docs — Task 8 covers only `VisualEditorGuestScript.GetScriptTag()`.

---

### Task 1: Guest script origin hardening

**Files:**
- Modify: `src/Umbraco.Web.UI.Client/src/apps/visual-editor/injected.ts`

- [ ] **Step 1: Add PARENT_ORIGIN derivation**

In `injected.ts`, directly after the `'use strict';` line (line 30), insert:

```typescript
	// =====================================================================
	// Parent origin — all postMessage traffic is pinned to the embedding
	// backoffice origin. Derived from document.referrer because in dev mode
	// the backoffice (Vite) runs on a different origin than this page.
	// =====================================================================

	const PARENT_ORIGIN = (() => {
		try {
			return document.referrer ? new URL(document.referrer).origin : window.location.origin;
		} catch {
			return window.location.origin;
		}
	})();
```

- [ ] **Step 2: Pin outgoing messages**

Replace the body of the `send()` function (line 143–145):

```typescript
	function send(message: Record<string, unknown>) {
		window.parent.postMessage({ ...message, source: 'umb-visual-editor-guest' }, '*');
	}
```

with:

```typescript
	function send(message: Record<string, unknown>) {
		window.parent.postMessage({ ...message, source: 'umb-visual-editor-guest' }, PARENT_ORIGIN);
	}
```

- [ ] **Step 3: Validate incoming message origin**

In the `window.addEventListener('message', ...)` handler (line 540–541), replace:

```typescript
	window.addEventListener('message', (evt: MessageEvent) => {
		if (!evt.data) return;
```

with:

```typescript
	window.addEventListener('message', (evt: MessageEvent) => {
		if (evt.origin !== PARENT_ORIGIN) return;
		if (!evt.data) return;
```

Known limitation (acceptable, dev-only): if the user clicks a link *inside* the preview iframe, the next page's `document.referrer` is the previous iframe page (server origin), so in cross-origin dev mode messages are dropped until refresh. In production the server and backoffice share an origin, so this has no effect. Parent→iframe postMessage already doesn't work cross-origin in dev (documented in `visual-page-builder.md` §7.8).

- [ ] **Step 4: Build**

Run: `cd src/Umbraco.Web.UI.Client && npm run build`
Expected: exits 0 (tsc clean).

- [ ] **Step 5: Commit**

```bash
git add src/Umbraco.Web.UI.Client/src/apps/visual-editor/injected.ts
git commit -m "fix(visual-editor): pin guest script postMessage to parent origin"
```

---

### Task 2: Message router + parent-side origin/source validation

**Files:**
- Create: `src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/visual-editor-message-router.ts`
- Modify: `src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/document-workspace-view-visual-editor.element.ts`

- [ ] **Step 1: Create the router**

Create `visual-editor-message-router.ts` with exactly:

```typescript
/**
 * Map of guest-script message types to their handler signatures.
 * Mirrors the messages sent by `src/apps/visual-editor/injected.ts`.
 */
export type UmbVisualEditorGuestMessageHandlers = {
	'umb:ve:property-selected': (data: { propertyAlias: string }) => void;
	'umb:ve:block-selected': (data: { blockKey: string; contentTypeAlias: string }) => void;
	'umb:ve:block-add': (data: { siblingBlockKey: string; insertIndex?: number }) => void;
	'umb:ve:block-add-to-area': (data: { parentBlockKey: string; areaAlias: string; insertIndex?: number }) => void;
	'umb:ve:block-move': (data: {
		blockKey: string;
		targetIndex?: number;
		targetParentBlockKey?: string;
		targetAreaAlias?: string;
	}) => void;
	'umb:ve:block-delete': (data: { blockKey: string }) => void;
	'umb:ve:block-reorder': (data: { blockKey: string; toIndex?: number }) => void;
	'umb:ve:region-map': (data: { regions?: Array<unknown> }) => void;
};

/**
 * Routes postMessage events from the visual editor guest script to typed handlers.
 * Rejects messages that do not originate from the preview iframe (origin + source checked).
 */
export class UmbVisualEditorMessageRouter {
	#handlers: Partial<UmbVisualEditorGuestMessageHandlers>;
	#getExpectedOrigin: () => string | undefined;
	#getExpectedSource: () => Window | null | undefined;

	constructor(args: {
		handlers: Partial<UmbVisualEditorGuestMessageHandlers>;
		getExpectedOrigin: () => string | undefined;
		getExpectedSource: () => Window | null | undefined;
	}) {
		this.#handlers = args.handlers;
		this.#getExpectedOrigin = args.getExpectedOrigin;
		this.#getExpectedSource = args.getExpectedSource;
	}

	readonly onMessage = (event: MessageEvent) => {
		const data = event.data;
		if (!data || data.source !== 'umb-visual-editor-guest') return;

		const expectedOrigin = this.#getExpectedOrigin();
		if (!expectedOrigin || event.origin !== expectedOrigin) return;

		const expectedSource = this.#getExpectedSource();
		if (!expectedSource || event.source !== expectedSource) return;

		const handler = this.#handlers[data.type as keyof UmbVisualEditorGuestMessageHandlers];
		handler?.(data);
	};
}
```

- [ ] **Step 2: Add iframe/origin helpers to the element**

In `document-workspace-view-visual-editor.element.ts`, replace `#postToIframe` (lines 687–691):

```typescript
	#postToIframe(message: Record<string, unknown>) {
		const iframe = this.shadowRoot?.querySelector('iframe') as HTMLIFrameElement | null;
		if (!iframe?.contentWindow) return;
		iframe.contentWindow.postMessage(message, '*');
	}
```

with:

```typescript
	#getIframe(): HTMLIFrameElement | null {
		return this.shadowRoot?.querySelector('iframe') ?? null;
	}

	#getServerOrigin(): string | undefined {
		if (!this.#serverUrl) return undefined;
		try {
			return new URL(this.#serverUrl).origin;
		} catch {
			return undefined;
		}
	}

	#postToIframe(message: Record<string, unknown>) {
		const iframe = this.#getIframe();
		const origin = this.#getServerOrigin();
		if (!iframe?.contentWindow || !origin) return;
		iframe.contentWindow.postMessage(message, origin);
	}
```

And in `#injectGuestScript` (line 622) replace:

```typescript
		const iframe = this.shadowRoot?.querySelector('iframe') as HTMLIFrameElement | null;
```

with:

```typescript
		const iframe = this.#getIframe();
```

- [ ] **Step 3: Replace `#onMessage` with the router**

Add to the element's imports (after the `UmbVisualEditorPreviewContext` import on line 15):

```typescript
import { UmbVisualEditorMessageRouter } from './visual-editor-message-router.js';
```

Replace the whole `#onMessage` field (lines 707–737, the `#onMessage = (event: MessageEvent) => { ... };` block including the `switch`) with:

```typescript
	#messageRouter = new UmbVisualEditorMessageRouter({
		getExpectedOrigin: () => this.#getServerOrigin(),
		getExpectedSource: () => this.#getIframe()?.contentWindow,
		handlers: {
			'umb:ve:property-selected': (d) => this.#onPropertyClicked(d.propertyAlias),
			'umb:ve:block-selected': (d) => this.#onBlockClicked(d.blockKey, d.contentTypeAlias),
			'umb:ve:block-add': (d) => this.#onBlockAdd(d.siblingBlockKey, d.insertIndex ?? 0),
			'umb:ve:block-add-to-area': (d) => this.#onBlockAddToArea(d.parentBlockKey, d.areaAlias, d.insertIndex ?? 0),
			'umb:ve:block-move': (d) =>
				this.#onBlockMove(d.blockKey, d.targetIndex ?? 0, d.targetParentBlockKey, d.targetAreaAlias),
			'umb:ve:block-delete': (d) => this.#onBlockDelete(d.blockKey),
			'umb:ve:block-reorder': (d) => this.#onBlockReorder(d.blockKey, d.toIndex ?? 0),
			'umb:ve:region-map': (d) => {
				this._hasRegions = (d.regions?.length ?? 0) > 0;
			},
		},
	});
```

In `connectedCallback` (line 697) replace `window.addEventListener('message', this.#onMessage);` with:

```typescript
		window.addEventListener('message', this.#messageRouter.onMessage);
```

In `disconnectedCallback` (line 702) replace `window.removeEventListener('message', this.#onMessage);` with:

```typescript
		window.removeEventListener('message', this.#messageRouter.onMessage);
```

- [ ] **Step 4: Build**

Run: `cd src/Umbraco.Web.UI.Client && npm run build`
Expected: exits 0.

- [ ] **Step 5: Commit**

```bash
git add src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/
git commit -m "refactor(visual-editor): extract typed message router with origin and source validation"
```

---

### Task 3: Strict opt-in semantics + minor cleanup

**Files:**
- Modify: `src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/document-workspace-view-visual-editor.element.ts`

- [ ] **Step 1: Remove the hybrid block-property filter**

In `#resolveBlockPropertyStructures`, delete lines 565–579 entirely:

```typescript
		// Check if any property has editableInVisualEditor explicitly enabled.
		// If none do, include all properties (opt-out model — works out of the box).
		const anyExplicitlyEnabled = data.properties.some((p) => {
			const a = p.appearance as { editableInVisualEditor?: boolean } | undefined;
			return a?.editableInVisualEditor === true;
		});

		const result: UmbVisualEditorPropertyInfo[] = [];
		for (const prop of data.properties) {
			// If any property is explicitly opted in, filter to only those.
			// Otherwise include all properties (opt-out fallback).
			if (anyExplicitlyEnabled) {
				const appearance = prop.appearance as { editableInVisualEditor?: boolean } | undefined;
				if (!appearance?.editableInVisualEditor) continue;
			}
```

and replace with:

```typescript
		const result: UmbVisualEditorPropertyInfo[] = [];
		for (const prop of data.properties) {
```

Result: the block editing modal shows ALL of the element type's properties. Document property editability remains gated server-side — `PublishedContentExtensions.TrackVisualEditorAccess` only annotates properties whose property type has `EditableInVisualEditor == true`, so unflagged document properties never become click targets. This is the approved semantic: strict opt-in for document properties, no filter for block properties.

- [ ] **Step 2: Key-order cleanup in clipboard paste**

In `#handleClipboardPaste` (line 971–972) replace:

```typescript
				const pastedLayoutCount =
					pastedBlocks.layout[Object.keys(pastedBlocks.layout)[0] ?? '']?.length ?? 0;
```

with:

```typescript
				const pastedLayoutCount = Object.values(pastedBlocks.layout)[0]?.length ?? 0;
```

- [ ] **Step 3: Build**

Run: `cd src/Umbraco.Web.UI.Client && npm run build`
Expected: exits 0.

- [ ] **Step 4: Commit**

```bash
git add src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/document-workspace-view-visual-editor.element.ts
git commit -m "fix(visual-editor): strict opt-in for document properties, unfiltered block modal fields"
```

---

### Task 4: Extract SignalR controller

**Files:**
- Create: `src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/visual-editor-signalr.controller.ts`
- Modify: `src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/document-workspace-view-visual-editor.element.ts`

- [ ] **Step 1: Create the controller**

Create `visual-editor-signalr.controller.ts` with exactly:

```typescript
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { HubConnectionBuilder } from '@umbraco-cms/backoffice/external/signalr';
import type { HubConnection } from '@umbraco-cms/backoffice/external/signalr';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Manages the SignalR PreviewHub connection for the visual editor.
 * Invokes the supplied callback with the refreshed document key whenever the
 * server signals that preview content has changed.
 */
export class UmbVisualEditorSignalRController extends UmbControllerBase {
	#connection?: HubConnection;
	#onRefreshed: (documentKey: string) => void;

	constructor(host: UmbControllerHost, onRefreshed: (documentKey: string) => void) {
		super(host);
		this.#onRefreshed = onRefreshed;
	}

	async connect(serverUrl: string) {
		if (!serverUrl) return;
		await this.disconnect();

		const hubUrl = `${serverUrl}/umbraco/PreviewHub`;
		this.#connection = new HubConnectionBuilder().withUrl(hubUrl).build();
		this.#connection.on('refreshed', this.#onRefreshed);

		try {
			await this.#connection.start();
		} catch (e) {
			console.error('[VisualEditor] SignalR connection failed', e);
		}
	}

	async disconnect() {
		if (this.#connection) {
			await this.#connection.stop();
			this.#connection = undefined;
		}
	}

	override destroy() {
		this.disconnect();
		super.destroy();
	}
}
```

- [ ] **Step 2: Use it in the element**

In `document-workspace-view-visual-editor.element.ts`:

(a) Add to imports (after the message-router import added in Task 2):

```typescript
import { UmbVisualEditorSignalRController } from './visual-editor-signalr.controller.js';
```

(b) Remove these imports, now unused by the element:

```typescript
import { HubConnectionBuilder } from '@umbraco-cms/backoffice/external/signalr';
import type { HubConnection } from '@umbraco-cms/backoffice/external/signalr';
```

(c) Replace the fields (lines 58–60):

```typescript
	#connection?: HubConnection;
	#serverUrl = '';
	#suppressRefresh = false;
```

with (`#suppressRefresh` is declared but never assigned anywhere — dead state):

```typescript
	#serverUrl = '';
	#signalR = new UmbVisualEditorSignalRController(this, (documentKey) => {
		if (documentKey === this.#workspaceContext?.getUnique()) {
			this.#refreshIframe();
		}
	});
```

(d) Delete the whole `#initSignalR` method (lines 1057–1080, from `async #initSignalR() {` to its closing `}` — including the `// --- SignalR ---` comment header).

(e) In `#initialize`, replace:

```typescript
		// Connect SignalR for live refresh
		this.#initSignalR();
```

with:

```typescript
		// Connect SignalR for live refresh
		this.#signalR.connect(this.#serverUrl);
```

(f) In `disconnectedCallback`, replace:

```typescript
		this.#connection?.stop();
		this.#connection = undefined;
```

with:

```typescript
		this.#signalR.disconnect();
```

- [ ] **Step 3: Build**

Run: `cd src/Umbraco.Web.UI.Client && npm run build`
Expected: exits 0.

- [ ] **Step 4: Commit**

```bash
git add src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/
git commit -m "refactor(visual-editor): extract SignalR controller, drop dead suppressRefresh state"
```

---

### Task 5: Extract property-structure resolver (Map-indexed)

**Files:**
- Create: `src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/visual-editor-property-structure.resolver.ts`
- Modify: `src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/document-workspace-view-visual-editor.element.ts`

- [ ] **Step 1: Create the resolver**

Create `visual-editor-property-structure.resolver.ts` with exactly:

```typescript
import type {
	UmbVisualEditorPropertyGroup,
	UmbVisualEditorPropertyInfo,
} from './visual-editor-property-modal.token.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { DataTypeService, DocumentTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';

export interface UmbVisualEditorBlockStructure {
	name: string;
	properties: UmbVisualEditorPropertyInfo[];
	groups: UmbVisualEditorPropertyGroup[];
}

/**
 * Resolves and caches property structures for the visual editor:
 * - Document properties across the full composition chain, indexed by alias.
 * - Block content/settings element type structures, cached by content type key.
 */
export class UmbVisualEditorPropertyStructureResolver extends UmbControllerBase {
	#documentProperties = new Map<string, UmbVisualEditorPropertyInfo>();
	#blockStructureCache = new Map<string, UmbVisualEditorBlockStructure>();

	get documentProperties(): Array<UmbVisualEditorPropertyInfo> {
		return [...this.#documentProperties.values()];
	}

	getDocumentProperty(alias: string): UmbVisualEditorPropertyInfo | undefined {
		return this.#documentProperties.get(alias);
	}

	/** Fetch the document type (and its composition chain) and index its properties by alias. */
	async loadDocumentStructure(contentTypeUnique: string): Promise<void> {
		const fetchedIds = new Set<string>();
		const toFetch = [contentTypeUnique];
		const allProperties: Array<{
			alias: string;
			name: string;
			description?: string | null;
			dataType: { id: string };
			validation: any;
		}> = [];

		while (toFetch.length > 0) {
			const batch = [...toFetch];
			toFetch.length = 0;

			for (const id of batch) {
				if (fetchedIds.has(id)) continue;
				fetchedIds.add(id);

				const { data } = await tryExecute(this, DocumentTypeService.getDocumentTypeById({ path: { id } }));
				if (!data) continue;

				allProperties.push(...(data.properties ?? []));

				for (const comp of data.compositions ?? []) {
					if (!fetchedIds.has(comp.documentType.id)) {
						toFetch.push(comp.documentType.id);
					}
				}
			}
		}

		this.#documentProperties.clear();
		for (const prop of allProperties) {
			const { editorUiAlias, config } = await this.#resolveDataType(prop.dataType.id);

			this.#documentProperties.set(prop.alias, {
				alias: prop.alias,
				name: prop.name ?? prop.alias,
				description: prop.description ?? undefined,
				editorUiAlias,
				config,
				validation: prop.validation,
			});
		}
	}

	/** Resolve a block element type's properties and groups, cached by content type key. */
	async resolveBlockPropertyStructures(contentTypeKey: string): Promise<UmbVisualEditorBlockStructure> {
		const cached = this.#blockStructureCache.get(contentTypeKey);
		if (cached) return cached;

		const { data } = await tryExecute(this, DocumentTypeService.getDocumentTypeById({ path: { id: contentTypeKey } }));
		if (!data?.properties) return { name: 'Block', properties: [], groups: [] };

		const groups: UmbVisualEditorPropertyGroup[] = (data.containers ?? [])
			.filter((c) => c.type === 'Group')
			.map((c) => ({ id: c.id, name: c.name ?? '', sortOrder: c.sortOrder }))
			.sort((a, b) => a.sortOrder - b.sortOrder);

		const result: UmbVisualEditorPropertyInfo[] = [];
		for (const prop of data.properties) {
			const { editorUiAlias, config } = await this.#resolveDataType(prop.dataType.id);

			result.push({
				alias: prop.alias,
				name: prop.name ?? prop.alias,
				description: prop.description ?? undefined,
				editorUiAlias: editorUiAlias || 'Umb.PropertyEditorUi.TextBox',
				config,
				validation: prop.validation,
				containerId: prop.container?.id ?? null,
			});
		}

		const resolved = { name: data.name ?? 'Block', properties: result, groups };
		this.#blockStructureCache.set(contentTypeKey, resolved);
		return resolved;
	}

	async #resolveDataType(
		dataTypeId: string | undefined,
	): Promise<{ editorUiAlias: string; config?: UmbPropertyEditorConfig }> {
		if (!dataTypeId) return { editorUiAlias: '' };

		const { data } = await tryExecute(this, DataTypeService.getDataTypeById({ path: { id: dataTypeId } }));
		if (!data) return { editorUiAlias: '' };

		return { editorUiAlias: data.editorUiAlias ?? '', config: data.values as UmbPropertyEditorConfig };
	}
}
```

- [ ] **Step 2: Replace element state and methods**

In `document-workspace-view-visual-editor.element.ts`:

(a) Add to imports:

```typescript
import { UmbVisualEditorPropertyStructureResolver } from './visual-editor-property-structure.resolver.js';
```

(b) Remove these now-unused imports from the element (`tryExecute`, `DataTypeService`/`DocumentTypeService` — but FIRST check with grep that no other element code uses them; `DocumentTypeService` is still used by `#sendNonEditableTypes` and must stay):

Run: `grep -n "tryExecute\|DataTypeService\|DocumentTypeService" src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/document-workspace-view-visual-editor.element.ts`

Expected after this task's edits: `DocumentTypeService` + `tryExecute` remain (used at the `#sendNonEditableTypes` alias lookup, line ~662); `DataTypeService` has no remaining uses → remove `DataTypeService` from the import:

```typescript
import { DocumentTypeService } from '@umbraco-cms/backoffice/external/backend-api';
```

(c) Delete the `#blockStructureCache` field (lines 70–74) and the `#propertyStructures` field (line 394), and add in their place (near the other private fields at the top):

```typescript
	#structures = new UmbVisualEditorPropertyStructureResolver(this);
```

(d) Delete the whole `#fetchPropertyStructures` method (lines 483–546) and the whole `#resolveBlockPropertyStructures` method (lines 548–609, as amended by Task 3), including the `// --- Property structures ---` comment header.

(e) In `#initialize`, replace:

```typescript
		// Resolve property structures from content type
		await this.#fetchPropertyStructures();
```

with:

```typescript
		// Resolve property structures from content type
		const contentTypeUnique = this.#workspaceContext.getContentTypeUnique();
		if (contentTypeUnique) {
			await this.#structures.loadDocumentStructure(contentTypeUnique);
		}
```

(f) Replace every remaining call site (verify with `grep -n "#propertyStructures\|#resolveBlockPropertyStructures" ...element.ts` — must return zero hits when done):

| Location | Old | New |
|---|---|---|
| `#ensureBlockManager` (line 87) | `this.#propertyStructures.find((p) => p.alias === propertyAlias)` | `this.#structures.getDocumentProperty(propertyAlias)` |
| property modal `onSetup` (line 144) | `this.#propertyStructures.find((p) => p.alias === alias)` | `this.#structures.getDocumentProperty(alias)` |
| block modal `onSetup` (line 192) | `await this.#resolveBlockPropertyStructures(found.block.contentTypeKey)` | `await this.#structures.resolveBlockPropertyStructures(found.block.contentTypeKey)` |
| block modal `onSetup` settings (line 207) | `await this.#resolveBlockPropertyStructures(blockTypeConfig.settingsElementTypeKey)` | `await this.#structures.resolveBlockPropertyStructures(blockTypeConfig.settingsElementTypeKey)` |
| catalogue `onSetup` (line 280) | `this.#propertyStructures.find((p) => p.alias === this.#pendingAdd!.propertyAlias)` | `this.#structures.getDocumentProperty(this.#pendingAdd!.propertyAlias)` |
| `#onPropertyClicked` (line 745) | `this.#propertyStructures.find((p) => p.alias === alias)` | `this.#structures.getDocumentProperty(alias)` |
| `#getBlocksConfig` (line 758) | `this.#propertyStructures.find((p) => p.alias === propertyAlias)` | `this.#structures.getDocumentProperty(propertyAlias)` |
| `#blockHasEditableFields` (line 788) | `await this.#resolveBlockPropertyStructures(contentTypeKey)` | `await this.#structures.resolveBlockPropertyStructures(contentTypeKey)` |
| `#blockHasEditableFields` loop (line 797) | `for (const prop of this.#propertyStructures) {` | `for (const prop of this.#structures.documentProperties) {` |
| `#blockHasEditableFields` settings (line 807) | `await this.#resolveBlockPropertyStructures(settingsElementTypeKey)` | `await this.#structures.resolveBlockPropertyStructures(settingsElementTypeKey)` |
| `#onBlockDelete` (line 986) | `await this.#resolveBlockPropertyStructures(found.block.contentTypeKey)` | `await this.#structures.resolveBlockPropertyStructures(found.block.contentTypeKey)` |
| `#handleClipboardPaste` (line 928) | `this.#propertyStructures.find((p) => p.alias === propertyAlias)` | `this.#structures.getDocumentProperty(propertyAlias)` |

(g) The element may no longer need the `UmbVisualEditorPropertyGroup` type import if it was only used by the deleted methods — check with grep; it is still used in the `#blockStructureCache` type which is deleted, but ALSO referenced in the block modal `onSetup` destructuring types. Keep imports that still have uses; remove ones tsc flags as unused.

- [ ] **Step 3: Build**

Run: `cd src/Umbraco.Web.UI.Client && npm run build`
Expected: exits 0. If tsc reports unused imports, remove exactly those.

- [ ] **Step 4: Commit**

```bash
git add src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/
git commit -m "refactor(visual-editor): extract Map-indexed property structure resolver"
```

---

### Task 6: Empty root-level block lists — server side

**Files:**
- Modify: `src/Umbraco.Web.Common/Extensions/BlockListTemplateExtensions.cs`
- Modify: `src/Umbraco.Web.UI/Views/Partials/blocklist/default.cshtml`

**Why:** An empty block list currently renders nothing (`GetBlockListHtmlAsync` returns empty string when `Count == 0`; the partial early-returns when empty), so the guest script has no container to attach an "Add content" button to and no way to learn the property alias. Fix: flow the property alias into the partial via ViewData on the alias-aware overloads, and render an annotated empty container in preview mode. The attribute is `data-umb-block-property` (NOT `data-umb-property` — that is the guest script's property-region selector and would turn the whole list into a clickable property region).

- [ ] **Step 1: Flow the property alias through the template extensions**

Replace the full contents of `src/Umbraco.Web.Common/Extensions/BlockListTemplateExtensions.cs` with:

```csharp
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions;

public static class BlockListTemplateExtensions
{
    public const string DefaultFolder = "blocklist/";
    public const string DefaultTemplate = "default";

    /// <summary>
    /// ViewData key under which the block list property alias is passed to the partial view,
    /// enabling the visual editor to annotate (and offer block creation on) empty block lists.
    /// </summary>
    public const string PropertyAliasViewDataKey = "umbBlockListPropertyAlias";

    #region Async

    public static async Task<IHtmlContent> GetBlockListHtmlAsync(this IHtmlHelper html, BlockListModel? model, string template = DefaultTemplate)
    {
        if (model?.Count == 0)
        {
            return new HtmlString(string.Empty);
        }

        return await html.PartialAsync(DefaultFolderTemplate(template), model);
    }

    public static async Task<IHtmlContent> GetBlockListHtmlAsync(this IHtmlHelper html, IPublishedProperty property, string template = DefaultTemplate)
        => await GetBlockListHtmlAsync(html, property.GetValue() as BlockListModel, template, property.Alias);

    public static async Task<IHtmlContent> GetBlockListHtmlAsync(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias)
        => await GetBlockListHtmlAsync(html, contentItem, propertyAlias, DefaultTemplate);

    public static async Task<IHtmlContent> GetBlockListHtmlAsync(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias, string template)
    {
        IPublishedProperty property = GetRequiredProperty(contentItem, propertyAlias);
        return await GetBlockListHtmlAsync(html, property.GetValue() as BlockListModel, template, propertyAlias);
    }

    private static async Task<IHtmlContent> GetBlockListHtmlAsync(IHtmlHelper html, BlockListModel? model, string template, string propertyAlias)
        => await html.PartialAsync(DefaultFolderTemplate(template), model, WithPropertyAlias(html, propertyAlias));

    #endregion

    #region Sync

    public static IHtmlContent GetBlockListHtml(this IHtmlHelper html, BlockListModel? model, string template = DefaultTemplate)
    {
        if (model?.Count == 0)
        {
            return new HtmlString(string.Empty);
        }

        return html.Partial(DefaultFolderTemplate(template), model);
    }

    public static IHtmlContent GetBlockListHtml(this IHtmlHelper html, IPublishedProperty property, string template = DefaultTemplate)
        => GetBlockListHtml(html, property.GetValue() as BlockListModel, template, property.Alias);

    public static IHtmlContent GetBlockListHtml(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias)
        => GetBlockListHtml(html, contentItem, propertyAlias, DefaultTemplate);

    public static IHtmlContent GetBlockListHtml(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias, string template)
    {
        IPublishedProperty property = GetRequiredProperty(contentItem, propertyAlias);
        return GetBlockListHtml(html, property.GetValue() as BlockListModel, template, propertyAlias);
    }

    private static IHtmlContent GetBlockListHtml(IHtmlHelper html, BlockListModel? model, string template, string propertyAlias)
        => html.Partial(DefaultFolderTemplate(template), model, WithPropertyAlias(html, propertyAlias));

    #endregion

    private static ViewDataDictionary WithPropertyAlias(IHtmlHelper html, string propertyAlias)
        => new(html.ViewData) { [PropertyAliasViewDataKey] = propertyAlias };

    private static string DefaultFolderTemplate(string template) => $"{DefaultFolder}{template}";

    private static IPublishedProperty GetRequiredProperty(IPublishedContent contentItem, string propertyAlias)
    {
        ArgumentNullException.ThrowIfNull(propertyAlias);

        if (string.IsNullOrWhiteSpace(propertyAlias))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(propertyAlias));
        }

        IPublishedProperty? property = contentItem.GetProperty(propertyAlias);
        if (property == null)
        {
            throw new InvalidOperationException("No property type found with alias " + propertyAlias);
        }

        return property;
    }
}
```

Notes:
- Public surface: only an ADDED const (`PropertyAliasViewDataKey`); no signature changes, no binary break. The new 4-arg core methods are `private`.
- Behavior change (intended): the alias-aware overloads no longer short-circuit empty models — the partial decides, so it can render an annotated empty container in preview mode. Outside preview the partial still renders nothing for empty models.

- [ ] **Step 2: Annotate the container and render the empty state in preview**

Replace the full contents of `src/Umbraco.Web.UI/Views/Partials/blocklist/default.cshtml` with:

```razor
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockListModel>
@{
    var propertyAlias = ViewData[Umbraco.Extensions.BlockListTemplateExtensions.PropertyAliasViewDataKey] as string;
    var annotate = (UmbracoContext?.InPreviewMode ?? false) && !string.IsNullOrEmpty(propertyAlias);
    if (Model?.Any() != true && !annotate) { return; }
}
<div class="umb-block-list" data-umb-block-property="@(annotate ? propertyAlias : null)">
    @if (Model is not null)
    {
        foreach (var block in Model)
        {
            if (block?.ContentKey == null) { continue; }
            var data = block.Content;

            <div data-umb-block-key="@block.ContentKey" data-umb-content-type="@data.ContentType.Alias">
                @await Html.PartialAsync("blocklist/Components/" + data.ContentType.Alias, block)
            </div>
        }
    }
</div>
```

(Razor omits the `data-umb-block-property` attribute entirely when the value expression is `null`, and HTML-encodes the alias when present.)

- [ ] **Step 3: Build the backend**

Run: `dotnet build src/Umbraco.Web.UI/Umbraco.Web.UI.csproj`
Expected: Build succeeded, 0 errors. (Building Web.UI transitively compiles Web.Common; the cshtml is validated at runtime since RazorCompileOnBuild=false, so review it carefully.)

- [ ] **Step 4: Commit**

```bash
git add src/Umbraco.Web.Common/Extensions/BlockListTemplateExtensions.cs src/Umbraco.Web.UI/Views/Partials/blocklist/default.cshtml
git commit -m "feat(visual-editor): annotate block list containers with property alias in preview"
```

---

### Task 7: Empty root-level block lists — client side

**Files:**
- Modify: `src/Umbraco.Web.UI.Client/src/apps/visual-editor/injected.ts`
- Modify: `src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/visual-editor-message-router.ts`
- Modify: `src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/document-workspace-view-visual-editor.element.ts`

- [ ] **Step 1: Guest script — resolve the alias and send the new message**

In `injected.ts`, replace the root-level empty list branch at the end of `insertAddButtons()` (lines 1033–1041):

```typescript
		// Empty block lists at root level (not inside a grid area)
		document.querySelectorAll<HTMLElement>('.umb-block-list').forEach((list) => {
			if (list.querySelector(BLOCK_SELECTOR)) return; // Has blocks
			if (list.querySelector(`[${ADD_BTN_ATTR}]`)) return; // Already handled

			// Find a sibling block key — for root-level empty lists there's none,
			// so we can't determine the property alias. This is a limitation.
			// TODO: annotate block list containers with property alias for empty state support.
		});
```

with:

```typescript
		// Empty block lists at root level (not inside a grid area).
		// The container carries data-umb-block-property (emitted by blocklist/default.cshtml
		// in preview mode) so the property alias is known even with no blocks present.
		document.querySelectorAll<HTMLElement>('.umb-block-list').forEach((list) => {
			if (list.querySelector(BLOCK_SELECTOR)) return; // Has blocks
			if (list.querySelector(`[${ADD_BTN_ATTR}]`)) return; // Already handled

			const propertyAlias = list.dataset.umbBlockProperty || '';
			if (!propertyAlias) return;

			list.appendChild(
				createEmptyPlaceholder(() => {
					send({ type: 'umb:ve:block-add-to-property', propertyAlias, insertIndex: 0 });
				}),
			);
		});
```

Also update the file-header doc comment: under "## Data attributes consumed" add the line:

```
 * - `data-umb-block-property` — Property alias on a block list container (empty-state block creation)
```

and under "## PostMessage protocol (sent to parent)" add:

```
 * - `umb:ve:block-add-to-property` — User clicked "Add content" on an empty block list
```

- [ ] **Step 2: Router — add the message type**

In `visual-editor-message-router.ts`, add to `UmbVisualEditorGuestMessageHandlers` after the `'umb:ve:block-add'` entry:

```typescript
	'umb:ve:block-add-to-property': (data: { propertyAlias: string; insertIndex?: number }) => void;
```

- [ ] **Step 3: Element — handle the message, DRY with the existing add path**

In `document-workspace-view-visual-editor.element.ts`, replace the whole `#onBlockAdd` method (the `// --- Block add ---` section) with:

```typescript
	// --- Block add ---

	async #onBlockAdd(siblingBlockKey: string, insertIndex: number) {
		const found = this.#findBlock(siblingBlockKey);
		if (!found) return;
		if (!found.blockValue?.contentData) return;

		await this.#addBlockToProperty(found.propertyAlias, found.blockValue, insertIndex);
	}

	async #onBlockAddToProperty(propertyAlias: string, insertIndex: number) {
		if (!this.#structures.getDocumentProperty(propertyAlias)) return;

		const raw = this.#getPropertyValue(propertyAlias) as BlockValue | undefined;
		const propertyValue: BlockValue =
			raw && Array.isArray(raw.contentData)
				? raw
				: { layout: {}, contentData: [], settingsData: [], expose: [] };

		await this.#addBlockToProperty(propertyAlias, propertyValue, insertIndex);
	}

	async #addBlockToProperty(propertyAlias: string, propertyValue: BlockValue, insertIndex: number) {
		const blocksConfig = this.#getBlocksConfig(propertyAlias);
		if (!blocksConfig || blocksConfig.length === 0) return;

		if (blocksConfig.length === 1) {
			// Single block type — skip catalogue, create directly
			const contentTypeKey = blocksConfig[0].contentElementTypeKey;
			const selectedBlockConfig = this.#findBlockTypeConfig(propertyAlias, contentTypeKey);

			const { updatedValue, contentKey } = addBlockToValue(
				propertyValue,
				contentTypeKey,
				insertIndex,
				undefined,
				selectedBlockConfig?.areas,
				selectedBlockConfig?.settingsElementTypeKey,
			);
			await this.#setPropertyValue(propertyAlias, updatedValue);

			const hasProperties = await this.#blockHasEditableFields(contentTypeKey, propertyAlias);
			if (hasProperties) {
				this.#onBlockClicked(contentKey, contentTypeKey);
			}
		} else {
			// Multiple block types — open routed catalogue modal
			this.#pendingAdd = { propertyAlias, propertyValue, insertIndex };
			this.#catalogueModalRegistration.open({ insertIndex });
		}
	}
```

(`addBlockToValue` defaults its `editorAlias` parameter to `UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS` when the layout is empty, which is correct here — only the block LIST partial emits the annotation.)

Then add to the router handlers map in the element (after `'umb:ve:block-add'`):

```typescript
			'umb:ve:block-add-to-property': (d) => this.#onBlockAddToProperty(d.propertyAlias, d.insertIndex ?? 0),
```

- [ ] **Step 4: Build**

Run: `cd src/Umbraco.Web.UI.Client && npm run build`
Expected: exits 0.

- [ ] **Step 5: Commit**

```bash
git add src/Umbraco.Web.UI.Client/src/apps/visual-editor/injected.ts src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/
git commit -m "feat(visual-editor): add-content placeholder for empty root-level block lists"
```

---

### Task 8: XML doc polish

**Files:**
- Modify: `src/Umbraco.Web.Common/Views/VisualEditorGuestScript.cs`

(`VisualEditorPropertyTracker` already has complete class-level XML docs — verified; no change needed there.)

- [ ] **Step 1: Fix the stale path comment and add param docs**

Replace the full contents of `VisualEditorGuestScript.cs` with:

```csharp
namespace Umbraco.Cms.Web.Common.Views;

/// <summary>
/// Generates the script tag for the visual editor guest script.
/// The script is built from <c>src/apps/visual-editor/injected.ts</c> in the backoffice client
/// and served from the backoffice static assets under <c>apps/visual-editor/injected.js</c>.
/// </summary>
internal static class VisualEditorGuestScript
{
    /// <summary>
    /// Builds the script tag referencing the visual editor guest script asset.
    /// </summary>
    /// <param name="nonce">The CSP nonce for the current request, or <c>null</c>/empty when CSP is not in use.</param>
    /// <param name="backOfficePath">The (cache-busted) backoffice assets path the script is served under.</param>
    /// <returns>The <c>&lt;script&gt;</c> tag markup.</returns>
    public static string GetScriptTag(string? nonce, string backOfficePath = "/umbraco/backoffice")
    {
        var nonceAttr = string.IsNullOrEmpty(nonce) ? string.Empty : $" nonce=\"{nonce}\"";
        return $"<script data-umb-visual-editor src=\"{backOfficePath}/apps/visual-editor/injected.js\"{nonceAttr}></script>";
    }
}
```

- [ ] **Step 2: Build + commit**

Run: `dotnet build src/Umbraco.Web.Common/Umbraco.Web.Common.csproj`
Expected: Build succeeded, 0 errors.

```bash
git add src/Umbraco.Web.Common/Views/VisualEditorGuestScript.cs
git commit -m "docs(visual-editor): correct guest script asset path and document GetScriptTag parameters"
```

---

### Task 9: Update the feature plan doc

**Files:**
- Modify: `docs/plans/visual-page-builder.md`

All edits below are exact-string replacements against the current file.

- [ ] **Step 1: Refresh the status header**

Replace:

```markdown
**Status**: PoC Complete — click-to-edit properties with live preview updates working end-to-end
**Date**: 2026-03-16
```

with:

```markdown
**Status**: Phases 1–2 complete; block manipulation (add/remove/reorder/cross-area move, clipboard paste, settings editing) implemented. Tidy-up round 2026-06-11 — see `2026-06-11-visual-editor-tidy-up-design.md`.
**Date**: 2026-03-16 (last updated 2026-06-11)
```

- [ ] **Step 2: Close Open Question 5**

In section 9, move Q5 from "Still Open" to "Answered by PoC" by replacing:

```markdown
5. **Property type setting vs editor-alias convention for controlling annotation?**
   - Current: hard-coded list of editor aliases (TextBox, TextArea, RichText, MarkdownEditor)
   - Alternative: property type appearance setting "Editable in Visual Editor" — more flexible, allows per-property control
   - Requires: backend model change, migration, API DTO, frontend toggle in property type settings
   - Recommendation: Start with convention, add the setting in Phase 2 if needed
```

with:

```markdown
5. **~~Property type setting vs editor-alias convention for controlling annotation?~~**
   - **Answer**: Property type setting. `EditableInVisualEditor` is implemented end-to-end (DB column + migration, `IPropertyType`, Management API `appearance.editableInVisualEditor`, settings toggle). Semantics are **strict opt-in** for document properties: only flagged properties are annotated/editable. Block content/settings properties are NOT filtered — the block editing modal shows all of the element type's fields.
```

- [ ] **Step 3: Revise Q11 (standalone window) and mark §10 deferred**

Replace:

```markdown
11. **Standalone window vs embedded workspace view?**
    - **Answer**: Standalone window. The visual editor will run as a separate app entry point (like `umb-preview`) in its own browser tab/window. Editing modals appear directly over the preview — no tab-switching. See [Architecture Evolution: Standalone Window](#10-architecture-evolution-standalone-window) for full design.
```

with:

```markdown
11. **Standalone window vs embedded workspace view?**
    - **Answer (revised 2026-06-11)**: Embedded workspace view is the current architecture. The standalone window evolution (§10) is **deferred** — it remains a possible future direction but is not the next step.
```

And replace the §10 heading:

```markdown
## 10. Architecture Evolution: Standalone Window
```

with:

```markdown
## 10. Architecture Evolution: Standalone Window (Deferred)

> **Deferred 2026-06-11** — the embedded workspace view is the current architecture; this section is kept as a possible future direction.
```

- [ ] **Step 4: Update Phase 2 "What's remaining" and Phase 3 status**

Replace:

```markdown
**What's remaining:**
- Block editing should ideally open the existing `UMB_BLOCK_WORKSPACE_MODAL` (full workspace with tabs, settings, validation) rather than our custom property list modal
- Block catalogue modal (`UMB_BLOCK_CATALOGUE_MODAL`) should be used for the "add block" type picker instead of auto-selecting the first type
- Block value write-back via block manager context (currently uses raw JSON manipulation)
```

with:

```markdown
**What's remaining:**
- Block editing should ideally open the existing `UMB_BLOCK_WORKSPACE_MODAL` (full workspace with tabs, settings, validation) rather than our custom property list modal
- ~~Block catalogue modal (`UMB_BLOCK_CATALOGUE_MODAL`) should be used for the "add block" type picker~~ — DONE: the routed catalogue modal is used (with clipboard paste support); single-block-type properties skip it deliberately
- Block value write-back via block manager context (currently uses raw JSON manipulation via `visual-editor-block-helper.ts`)
```

And replace:

```markdown
### Phase 3: Block Manipulation + Partial Re-render

**Goal**: Blocks can be added, removed, and reordered visually. Changed regions re-render without full page reload.

**What to build:**
- Block add/remove/reorder via visual UI (overlay "+" buttons, drag handles)
- Partial re-render API endpoint (extends BlockPreview pattern)
- `PropertyOverridePublishedContent` decorator for rendering with unsaved values
- Guest script DOM patching for partial updates
```

with:

```markdown
### Phase 3: Block Manipulation + Partial Re-render (Block Manipulation COMPLETE)

**Goal**: Blocks can be added, removed, and reordered visually. Changed regions re-render without full page reload.

**Done:**
- Block add (inline "+" buttons + catalogue modal, including empty-list and empty-area placeholders), remove (confirm dialog), reorder, and cross-area move via drag-and-drop
- Clipboard paste with translator compatibility checks and expose entries

**Still to build:**
- Partial re-render API endpoint (extends BlockPreview pattern)
- `PropertyOverridePublishedContent` decorator for rendering with unsaved values
- Guest script DOM patching for partial updates
```

- [ ] **Step 5: Update Appendix B**

In Appendix B, replace:

```markdown
**Planned for future phases:**

| Attribute | Purpose |
|-----------|---------|
| `data-umb-block-area` | Block grid area identifier (for drop zones) |
| `data-umb-inline-edit` | Enables inline contenteditable editing |
```

with:

```markdown
| `data-umb-block-property` | `blocklist/default.cshtml` (preview mode) | Property alias | Block list container annotation enabling empty-state block creation |

**Planned for future phases:**

| Attribute | Purpose |
|-----------|---------|
| `data-umb-block-area` | Block grid area identifier (for drop zones) |
| `data-umb-inline-edit` | Enables inline contenteditable editing |
```

(The new row extends the "Implemented in PoC" table that directly precedes the "Planned for future phases" block.)

- [ ] **Step 6: Commit**

```bash
git add docs/plans/visual-page-builder.md
git commit -m "docs(visual-editor): refresh plan statuses, close opt-in question, defer standalone window"
```

---

### Task 10: Final verification

- [ ] **Step 1: Full client build + lint**

```bash
cd src/Umbraco.Web.UI.Client
npm run build
npm run lint
```

Expected: build exits 0; lint reports no NEW errors in the visual-editor files (pre-existing warnings elsewhere are out of scope).

- [ ] **Step 2: Full solution build**

```bash
dotnet build umbraco.sln
```

Expected: 0 errors (66k pre-existing StyleCop warnings are known and out of scope).

- [ ] **Step 3: Manual smoke pass** (run the site: `dotnet run --project src/Umbraco.Web.UI`, backoffice at https://localhost:44339/umbraco)

Checklist:
1. Open a document with a template in the visual editor tab — iframe loads, regions outlined.
2. A document text property flagged "Editable in visual editor" is clickable and edits via the sidebar modal; an UNflagged text property is not annotated/clickable.
3. Click a block — modal shows ALL its content/settings fields (no filtering).
4. Add a block via "+" between blocks; delete a block; drag a block to reorder; move a block into a grid area.
5. Empty a block list (delete all blocks), save — the "Add content" placeholder appears; clicking it opens the catalogue (or directly creates when one block type) and the new block renders.
6. Save → SignalR refresh → previous selection re-highlighted.
7. PostMessage still functions (clicks reach the backoffice) — verify in BOTH built mode (same-origin) and `npm run dev` Vite mode (cross-origin).

- [ ] **Step 4: Update the design spec status line**

In `docs/plans/2026-06-11-visual-editor-tidy-up-design.md` replace:

```markdown
**Status**: Approved design, pending implementation
```

with:

```markdown
**Status**: Implemented
```

```bash
git add docs/plans/2026-06-11-visual-editor-tidy-up-design.md
git commit -m "docs(visual-editor): mark tidy-up design as implemented"
```
