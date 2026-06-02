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
- Add to an **existing group** when the text belongs to that feature area
- Create a **new group** only for new feature areas

### Value types

| Type | Use when | Example |
|------|----------|---------|
| `string` | Static text | `myLabel: 'My Label'` |
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
		return html`
			<uui-button label=${this.localize.term('myFeature_myLabel')}></uui-button>
		`;
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
