---
name: general-add-localization
description: Add localization keys and use them in elements or controllers. Use when adding user-facing text that should be translatable — labels, descriptions, error messages, button text, status text, or any string shown in the backoffice UI.
allowed-tools: Read, Write, Edit, Grep, Glob
---

# Add Localization

Add translatable text to the Umbraco backoffice.

## What you need from the user

1. **The text to localize** — What the user sees (e.g., "Create item", "Status")
2. **Which group it belongs to** — Feature area (e.g., `actions`, `general`, `content`, `user`)
3. **Where it's used** — Element template, controller logic, or standalone component

## Step 1: Add the key to the English dictionary

**File:** `src/assets/lang/en.ts`

This file exports a `UmbLocalizationDictionary` — a nested object where top-level keys are groups and nested keys are the terms.

```typescript
// src/assets/lang/en.ts
export default {
	// ... existing groups
	myFeature: {
		myLabel: 'My Label',
		myDescription: 'A description of the feature',
		createFor: (name: string) => (name ? `Create item for ${name}` : 'Create'),
	},
} satisfies UmbLocalizationDictionary;
```

### Key naming rules

- **Group**: camelCase feature name (e.g., `actions`, `general`, `content`, `media`, `user`)
- **Term**: camelCase descriptive name (e.g., `assignDomain`, `auditTrail`, `browse`)
- **Full key** used in code: `group_termName` (underscore separator)

#### Choosing a group

A group covers a **shared UX area**, not just the one component you're editing. Don't pick a group unilaterally — confirm it with the user:

1. **If you already have a good idea of the scope** (from the surrounding code, the feature being worked on, etc.), propose it: "This looks like it belongs to the `{scope}` scope — should that be the localization group?"
2. **If you don't**, ask directly: "What is the common group name for this localization?"
3. **Either way, check for an existing match first.** Search the groups already in `src/assets/lang/en.ts` for one that already covers this UX area, and if you find one, ask whether it should be reused instead of creating a new group: "`{existingGroup}` already covers this — should I use that instead?"

Examples already in this codebase:

- `blockEditor` — Block List, Block Grid, and Block RTE share the same block-configuration UX
- `contentTypeEditor` — Document Type, Media Type, and Member Type share the same editing UX
- `codeEditor` — anything embedding the code editor

Only create a new group once the user confirms no existing one fits.

#### Choosing a term

A term names the **situation**, not the wording — two situations with identical English text today should still get separate terms, since the copy can diverge later.

Build it from two parts:

1. **Presentation role**: `Title`, `Description`, `Action`, `Label`, `Notice`, `Message`, `ValidationMessage`, `Headline`, etc.
2. **Subject**: `CreateBlock`, `ConfirmDelete`, `AddGroup`.

Combined: `createAction`, `confirmDeleteTitle`, `addGroupDescription`.

Example — three terms for one dialog in the `blockEditor` group, same subject (`confirmDeleteBlockGroup`) with different roles:

```typescript
confirmDeleteBlockGroupTitle: 'Delete group?',
confirmDeleteBlockGroupMessage: 'Are you sure you want to delete group <strong>%0%</strong>?',
confirmDeleteBlockGroupNotice: 'The content of these Blocks will still be present, editing of this content will no longer be available and will be shown as unsupported content.',
```

Grouping by subject keeps every piece of the dialog together, even though the wording shares nothing.

### Value types

| Type               | Use when                 | Example                                           |
| ------------------ | ------------------------ | ------------------------------------------------- |
| `string`           | Static text              | `myLabel: 'My Label'`                             |
| `(args) => string` | Text with dynamic values | `createFor: (name: string) => \`Create ${name}\`` |

## Step 2: Use the localized text

### In element templates — `this.localize.term()`

Available on any class extending `UmbLitElement`. The `localize` property is provided automatically.

```typescript
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-my-element')
export class UmbMyElement extends UmbLitElement {
	override render() {
		return html`<uui-button label=${this.localize.term('myFeature_myLabel')}></uui-button>`;
	}
}
```

### In templates — `<umb-localize>` element

For inline localized text in HTML templates:

```html
<umb-localize key="myFeature_myLabel"></umb-localize>

<!-- With fallback text (shown if key is missing) -->
<umb-localize key="myFeature_myLabel">Fallback text</umb-localize>
```

### In controllers or non-element classes — `UmbLocalizationController`

```typescript
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export class UmbMyManager extends UmbControllerBase {
	readonly #localization = new UmbLocalizationController(this);

	someMethod() {
		const label = this.#localization.term('myFeature_myLabel');
	}
}
```

## Checklist

- [ ] Key added to `src/assets/lang/en.ts` in the correct group
- [ ] Key follows `group_termName` convention (camelCase, underscore separator)
- [ ] Used `this.localize.term()` in elements or `UmbLocalizationController` in non-elements
- [ ] No hardcoded user-facing strings remain
- [ ] Compiles: `npm run compile`
