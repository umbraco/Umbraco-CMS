---
name: general-add-clipboard-support
description: Add clipboard copy and/or paste support to a property editor — copy its value to the clipboard, paste a clipboard entry into it, and interoperate with other property editors through a shared entry value type. Use when the user asks to make a property editor copyable or pasteable, to support copy/paste between two property editors, to add a clipboard tab to a picker, or to add a per-item copy action to an input. Covers the property context, copy/paste value translators, property actions, entry value types, and the input-to-property-editor event channel.
allowed-tools: Read, Write, Edit, Grep, Glob, Bash
---

# Add Clipboard Support To A Property Editor

## Foundational documentation

Read before proceeding:

- **[Clipboard](../../../docs/clipboard.md)** — the model, translator keying, where the work belongs, rules
- **[Manifests & Aliases](../../../docs/manifests.md)** — manifest shape and alias conventions

## What you need from the user

1. **Property editor** — the UI alias and where it lives (e.g. `Umb.PropertyEditorUi.MyPicker`, `src/packages/.../property-editors/my-picker/`)
2. **Direction** — copy, paste, or both
3. **Interop** — should it exchange values with another property editor? If so, which, and is there already a shared entry value type for the format?
4. **Affordance** — where does the user trigger it?
   - property actions only (copy/paste the whole value) — the cheapest, start here
   - a clipboard tab in a picker modal (paste one entry, merged into the value)
   - a per-item copy action on the input
5. **Value shape** — determine it from the code, don't ask:

| Shape | Paste means | Steps that apply |
|---|---|---|
| **Collection** — a list of items, each identifiable within the value | merge, deduplicated | all steps |
| **Single value** — one scalar or object | replace | 1–4 only; a per-item affordance makes no sense |

Do not guess 2–4. The answers change which of the steps below apply.

**Stop if the value is edited through a live surface** — an editor instance that holds the current state, so that a paste means "insert at a position" rather than "merge into a value". Steps 5–7 do not apply: writing a value back replaces the document and discards the selection and undo history, and nothing in the codebase solves the placement yet. Report that and agree an approach before writing code.

---

## Step 1 — Establish the entry value type

An entry value type is a **format**, not an editor. Reuse before inventing.

```bash
grep -rn "CLIPBOARD_ENTRY_VALUE_TYPE" src/packages --include=constants.ts
```

If a suitable one exists, use it and skip to step 2. Only invent one when no existing format fits.

**Creating one** — in the package that owns the *format*, not the property editor:

```typescript
// {package}/clipboard/constants.ts
export const UMB_{FORMAT}_CLIPBOARD_ENTRY_VALUE_TYPE = '{format}';

// {package}/clipboard/types.ts
export type Umb{Format}ClipboardEntryValueModel = Array<{ unique: string }>; // shape it as the format requires
```

Export both from the package's `constants.ts` and `types.ts` barrels.

Rules:
- Name it after **what the data is**, never after the property editor that produces it. A second editor will emit the same format, and a name that points at the first one will read as a mistake there.
- Keep it minimal and serialisable — it is stored in local storage and read by editors that know nothing about yours.
- If you have a rich format, define a plain one too and emit both on copy (step 2). Without it, only editors that understand the rich format can paste.

## Step 2 — Copy translator (skip if paste-only)

```
{property-editor}/clipboard/{format}/copy/
├── manifest.ts
└── {editor}-to-{format}-copy-translator.ts
```

```typescript
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardCopyPropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';

export class Umb{Editor}To{Format}ClipboardCopyPropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardCopyPropertyValueTranslator<{TPropertyValue}, {TEntryValue}>
{
	async translate(propertyValue: {TPropertyValue}): Promise<{TEntryValue}> {
		if (!propertyValue) {
			throw new Error('Property value is missing.');
		}

		// Map to the entry value format. Filter out anything the format cannot represent.
		return propertyValue.map((item) => ({ unique: item.unique }));
	}
}

export { Umb{Editor}To{Format}ClipboardCopyPropertyValueTranslator as api };
```

```typescript
// manifest.ts
export const manifest: UmbExtensionManifest = {
	type: 'clipboardCopyPropertyValueTranslator',
	alias: 'Umb.ClipboardCopyPropertyValueTranslator.{Editor}To{Format}',
	name: '{Editor} To {Format} Clipboard Copy Property Value Translator',
	api: () => import('./{editor}-to-{format}-copy-translator.js'),
	fromPropertyEditorUi: {PROPERTY_EDITOR_UI_ALIAS},
	toClipboardEntryValueType: UMB_{FORMAT}_CLIPBOARD_ENTRY_VALUE_TYPE,
};
```

Register one copy translator **per format** you want to emit. All of them run on a copy, producing one entry with several values.

## Step 3 — Paste translator (skip if copy-only)

```
{property-editor}/clipboard/{format}/paste/
├── manifest.ts
└── {format}-to-{editor}-paste-translator.ts
```

```typescript
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardPastePropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';

export class Umb{Format}To{Editor}ClipboardPastePropertyValueTranslator
	extends UmbControllerBase
	implements
		UmbClipboardPastePropertyValueTranslator<{TEntryValue}, {TPropertyValue}, UmbPropertyEditorConfig | undefined>
{
	async translate(
		value: {TEntryValue},
		config: UmbPropertyEditorConfig | undefined,
	): Promise<{TPropertyValue}> {
		if (!value) {
			throw new Error('Value is missing.');
		}

		// Produce a value this property editor supports *under this configuration*.
		// Drop options the config does not declare rather than importing them blindly.
		return value.map((item) => ({ unique: item.unique }));
	}

	async isCompatibleValue(
		_propertyValue: {TPropertyValue},
		config: UmbPropertyEditorConfig | undefined,
	): Promise<boolean> {
		// Gate, not transform. Entries that fail render disabled in the picker.
		return this.#configValue(config, '{someAlias}') !== undefined;
	}

	#configValue<ValueType>(config: UmbPropertyEditorConfig | undefined, alias: string): ValueType | undefined {
		return config?.find((property) => property.alias === alias)?.value as ValueType | undefined;
	}
}

export { Umb{Format}To{Editor}ClipboardPastePropertyValueTranslator as api };
```

```typescript
// manifest.ts
export const manifest: UmbExtensionManifest = {
	type: 'clipboardPastePropertyValueTranslator',
	alias: 'Umb.ClipboardPastePropertyValueTranslator.{Format}To{Editor}',
	name: '{Format} To {Editor} Clipboard Paste Property Value Translator',
	api: () => import('./{format}-to-{editor}-paste-translator.js'),
	fromClipboardEntryValueType: UMB_{FORMAT}_CLIPBOARD_ENTRY_VALUE_TYPE,
	toPropertyEditorUi: {PROPERTY_EDITOR_UI_ALIAS},
	// weight: 100,  // higher wins when several translators match one entry — set it when you register two
};
```

**Config is the raw array**, not `UmbPropertyEditorConfigCollection`. `config.getValueByAlias(...)` will throw at runtime; a test that passes a collection will pass against code that can never work in the app. Always look up by alias.

**Put configuration-dependent filtering here, not in the property editor element.** The paste property action calls the translator and writes straight to the property context, never touching the element — so logic in the element covers only the picker route.

## Step 4 — Register the property context and actions

```typescript
// {property-editor}/clipboard/manifests.ts
import { manifest as copyTranslator } from './{format}/copy/manifest.js';
import { manifest as pasteTranslator } from './{format}/paste/manifest.js';
import {
	UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS,
	UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
} from '@umbraco-cms/backoffice/property';

const forPropertyEditorUis = [{PROPERTY_EDITOR_UI_ALIAS}];

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyContext',
		kind: 'clipboard',
		alias: 'Umb.PropertyContext.{Editor}.Clipboard',
		name: '{Editor} Clipboard Property Context',
		forPropertyEditorUis,
	},
	{
		type: 'propertyAction',
		kind: 'copyToClipboard',
		alias: 'Umb.PropertyAction.{Editor}.Clipboard.Copy',
		name: '{Editor} Copy To Clipboard Property Action',
		forPropertyEditorUis,
		conditions: [{ alias: UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS }],
	},
	{
		type: 'propertyAction',
		kind: 'pasteFromClipboard',
		alias: 'Umb.PropertyAction.{Editor}.Clipboard.Paste',
		name: '{Editor} Paste From Clipboard Property Action',
		forPropertyEditorUis,
		conditions: [{ alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS }],
	},
	copyTranslator,
	pasteTranslator,
];
```

Then spread `...clipboardManifests` into the property editor's own `manifests.ts`.

**Register only the actions you have translators for.** A paste action without a paste translator, or a copy action for a configuration that has nothing to copy, shows a broken affordance. Where availability depends on *configuration* rather than alias, the manifest cannot express it — omit the actions and gate in the element instead (step 5).

Property actions replace the whole value, so a paste action only makes sense where replacing is a reasonable thing for the user to ask for. Where it is not — a value the user composes rather than picks — register the context and the translators but omit the paste action.

**Stop here if property actions are the only affordance.** Go to step 8.

## Step 5 — Property editor element: derive the configuration to hand down

Only for a picker clipboard tab or a per-item copy action.

```typescript
import {
	UMB_CLIPBOARD_PROPERTY_CONTEXT,
	type UmbClipboardCopyRequestEvent,
	type UmbClipboardPasteRequestEvent,
} from '@umbraco-cms/backoffice/clipboard';

@state()
private _clipboardConfig?: Umb{Feature}ClipboardConfig;

#clipboardContext?: typeof UMB_CLIPBOARD_PROPERTY_CONTEXT.TYPE;
#clipboardCopyAvailable = false;
#clipboardPasteTypes?: Array<string>;

constructor() {
	super();

	this.consumeContext(UMB_CLIPBOARD_PROPERTY_CONTEXT, (context) => {
		this.#clipboardContext = context;

		this.observe(
			context?.copyAvailable,
			(available) => {
				this.#clipboardCopyAvailable = available ?? false;
				this.#updateClipboardConfig();
			},
			'observeClipboardCopyAvailable',
		);

		this.observe(
			context?.pasteAvailable,
			async (available) => {
				this.#clipboardPasteTypes = available ? await context!.getSupportedPasteEntryValueTypes() : undefined;
				this.#updateClipboardConfig();
			},
			'observeClipboardPasteAvailable',
		);
	});
}

#updateClipboardConfig() {
	const clipboardContext = this.#clipboardContext;
	const types = this.#clipboardPasteTypes;

	this._clipboardConfig = clipboardContext
		? {
				copy: { enabled: this.#clipboardCopyAvailable },
				paste: {
					enabled: !!types?.length,
					types: types ?? [],
					pickableFilter: (entry) => clipboardContext.isEntryPastable(entry),
				},
			}
		: undefined;
}
```

Notes:
- The context is **absent** for editors that don't register it, so keep every read optional. `undefined` config means no clipboard at all.
- Call `#updateClipboardConfig()` from the `config` setter too if availability depends on configuration (e.g. only offer the clipboard when the editor is configured for a compatible source type).
- The config type belongs in the package that owns the **picker modal contract**, since its paste half is what the modal takes. Shape it as `{ copy: { enabled }, paste: { enabled, types, pickableFilter? } }` — both halves objects so they can grow, `enabled` the only switch, and `types` required and `[]` when empty.

## Step 6 — Input: report, never resolve

The input takes the config as one optional property and hands back identities.

```typescript
/**
 * The clipboard affordances to offer. Supplied by the hosting property editor, because it is the one that owns
 * the value on both sides — see the `clipboard-copy-request` and `clipboard-paste-request` events.
 */
@property({ type: Object, attribute: false })
clipboardConfig?: Umb{Feature}ClipboardConfig;
```

**Per-item copy:**

```typescript
#renderCopyAction(item: {TItem}) {
	if (this.readonly || !this.clipboardConfig?.copy.enabled) return nothing;
	return html`
		<uui-button
			label=${this.localize.term('clipboard_labelForCopyToClipboard')}
			look="secondary"
			@click=${() => this.#requestClipboardCopy(item)}>
			<uui-icon name="icon-clipboard-copy"></uui-icon>
		</uui-button>
	`;
}

// Requested, not written: only the hosting property editor can produce the value for the item.
#requestClipboardCopy(item: {TItem}) {
	this.dispatchEvent(new UmbClipboardCopyRequestEvent({ unique: item.unique, name: item.name, icon: item.icon }));
}
```

`unique` must be whatever identifies the item **within the property value** — not necessarily the entity's id. `name` and `icon` are presentation the input has already resolved for rendering; the property value doesn't carry them.

**Paste via a picker's clipboard tab:** pass `clipboardConfig.paste` as the modal's clipboard data, then report the picked entries:

```typescript
const modalValue = await this.#pickerInputContext.openPickerForValue({
	/* ...existing picker data... */
	clipboard: this.clipboardConfig?.paste,
});

const clipboardSelection = modalValue?.clipboard?.selection;
if (clipboardSelection?.length) {
	// Requested, not resolved: only the hosting property editor can turn entries into its value.
	this.dispatchEvent(new UmbClipboardPasteRequestEvent(clipboardSelection));
}
```

The modal needs a `clipboard?: {PasteConfig}` key on its data and a `clipboard?: { selection: Array<string> }` key on its value, gated on `if (!this.data?.clipboard?.enabled) return nothing`.

**A forwarding element in between** (an input that renders another input) forwards the property down and re-dispatches both events, because they are `composed: false`:

```typescript
#onClipboardCopyRequest(event: UmbClipboardCopyRequestEvent) {
	event.stopPropagation();
	this.dispatchEvent(new UmbClipboardCopyRequestEvent({ unique: event.unique, name: event.name, icon: event.icon }));
}
```

If the forwarding element is polymorphic — it renders a different input depending on configuration — name the property for the case it serves (`{case}ClipboardConfig`) rather than plainly. Only one case supports the clipboard at first, and a plain name would have to be renamed, breaking consumers, when a second one does.

## Step 7 — Property editor element: handle the requests

```typescript
async #onClipboardCopyRequest(event: UmbClipboardCopyRequestEvent) {
	const item = this.value?.find((candidate) => candidate.{key} === event.unique);

	if (!item) {
		throw new Error(`Could not find a value for the item with unique: ${event.unique}`);
	}

	await this.#clipboardContext?.write({
		propertyValue: [structuredClone(item)],
		itemName: event.name,
		icon: event.icon,
	});
}

async #onClipboardPasteRequest(event: UmbClipboardPasteRequestEvent) {
	if (!this.#clipboardContext) return;

	const propertyValues = await this.#clipboardContext.readMultiple<{TPropertyValue}>(event.entryUniques);
	const pasted = propertyValues.flat();

	const currentValue = this.value ?? [];
	const additions = pasted.filter((addition) => !currentValue.some((item) => item.{key} === addition.{key}));

	if (!additions.length) return;

	// Not clamped to the configured limit: as when picking, an over-long selection is for validation to
	// report and the user to trim.
	this.value = [...currentValue, ...additions];
	this.dispatchEvent(new UmbChangeEvent());
}
```

Bind both in the template alongside the config:

```html
.clipboardConfig=${this._clipboardConfig}
@clipboard-copy-request=${this.#onClipboardCopyRequest}
@clipboard-paste-request=${this.#onClipboardPasteRequest}
```

`write()` needs only the value — the context derives the alias, the entry name and the icon from the property.

**Deduplicate, but do not clamp or replace.** Adding an item that's already present isn't the user's intent; exceeding a maximum is for validation to report.

## Step 8 — Tests

| What | Where | Why there |
|---|---|---|
| Translator output, config handling, `isCompatibleValue` | beside the translator | plain transform, no element or context harness needed |
| Config handed to the input, per-configuration gating | property editor element test | needs the rendered element |
| Request handling — copy produces the right value, paste merges | property editor element test | provide a stub via `UmbContextProvider(element, UMB_CLIPBOARD_PROPERTY_CONTEXT, stub)` with only the members used |
| Affordance rendering and event dispatch | input test | assert the event payload carries the identity, not a value |

Stub shape for the element tests:

```typescript
const clipboardContext = {
	getHostElement: () => element,
	copyAvailable: new UmbBooleanState(true).asObservable(),
	pasteAvailable: new UmbBooleanState(true).asObservable(),
	getSupportedPasteEntryValueTypes: async () => ['{format}'],
	isEntryPastable: async () => true,
	readMultiple: async () => pasteResult,
	write: async (args) => { written.push(args); return undefined; },
} as any;
```

Dispatch the events from the **inner input**, not the host — the property editor listens on the input:

```typescript
element.shadowRoot!.querySelector('{input-element}')!.dispatchEvent(event);
```

Per the repo's testing rule, verify each behaviour test fails before the fix: revert the production change and confirm red.

## Verification

```bash
npx tsc --noEmit -p tsconfig.json
npm test -- --files "src/packages/{package}/**/*.test.ts"
npm run check:circular
```

`npx eslint` over whole directories can crash on unrelated files (`import/order` under ESLint 10) — lint the changed files individually.

## Checklist

- [ ] Entry value type reused where one existed; any new one named for the format, in the package that owns the format
- [ ] A lowest-common-denominator value emitted alongside any rich one
- [ ] Translator manifests key the right direction (`fromPropertyEditorUi` for copy, `toPropertyEditorUi` for paste)
- [ ] `weight` set when two paste translators can match one entry
- [ ] Config read by alias off the raw array — no `getValueByAlias`
- [ ] Configuration-dependent filtering in the translator, so the property action route gets it too
- [ ] Property actions registered only where they have translators, and the paste action omitted where replacing the whole value is not a sensible request
- [ ] Copy affordances gated on `copyAvailable`, paste on `pasteAvailable`
- [ ] The input constructs no clipboard value and reads no clipboard entry — identities only
- [ ] Both events re-dispatched by any forwarding element
- [ ] Paste deduplicates but does not clamp or replace
- [ ] `clipboard_labelForCopyToClipboard` (or an existing key) used for labels — no hardcoded strings
- [ ] Tests placed per the table, each verified to fail before the fix
