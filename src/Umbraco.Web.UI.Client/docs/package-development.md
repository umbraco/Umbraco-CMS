# Package Development
[← Umbraco Backoffice](../CLAUDE.md) | [← Monorepo Root](../../CLAUDE.md)

---

This guide covers how to structure, organize, and build a new package (or extend an existing one) within the Umbraco Backoffice frontend. For the underlying architecture concepts (extension registry, context API, repository pattern, etc.), see [Architecture](./architecture.md).

---

## Folder Structure Conventions

The most important organizational rule: **sub-folders are organized by sub-feature, not by technical role.** A sub-feature folder contains *everything* related to that feature — its elements, repository, store, modals, actions, constants, types, and manifests. You do not create top-level `repositories/` or `modals/` folders.

- **No top-level `repositories/`, `modals/`, or `actions/` folders.** These live inside the sub-feature that owns them.
- **Every sub-feature is self-contained.** It owns its full vertical slice — elements, repository, store, data sources, actions, modals, types, constants, and manifests. If you deleted the `analytics-dashboard/` folder, nothing outside it would break (apart from the missing manifest registration).
- **Every folder that contains registerable items has its own `manifests.ts`.** Child manifests bubble up to the package root.
- **Every sub-feature has `index.ts`, `manifests.ts`, and `constants.ts` at minimum.** Add `types.ts` when the sub-feature defines shared types.

---

## Key Organizational Rules

### Sub-Feature Self-Containment

Each sub-feature folder (e.g., `analytics-dashboard/`) owns its full vertical slice.

### `local-components/`

When a sub-feature's main element needs to split into smaller elements that are *only used within that sub-feature*, place them in a `local-components/` folder inside the sub-feature. These are never exported and are imported directly from the component using them. They are private implementation details.

### `global-components/`

When a package needs to expose reusable elements that *other packages* can use in their elements, place them in `global-components/`. These components **must be registered as custom elements in the browser at startup** (so they are available in any element) and their **classes must be exported** from the package's `index.ts` so other packages can reference their types.

```typescript
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-feature')
export class UmbFeatureElement extends UmbLitElement {
  render() {
    return html`Feature Element`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    ['umb-feature']: UmbFeatureElement;
  }
}
```

### Manifest Bubbling

Each sub-feature exports its own `manifests` array. The package-level `manifests.ts` aggregates them all:

```typescript
import { manifests as sectionManifests } from './section/manifests.js';
import { manifests as dashboardManifests } from './dashboard/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

export const manifests = [
  ...sectionManifests,
  ...dashboardManifests,
  ...workspaceManifests,
];
```

The same pattern applies to constants and types — each sub-feature exports its own, and the package-level files re-export what should be part of the public API.

---

## The Package's Public API (index.ts)

The `index.ts` barrel export controls what other packages can import. Only export what constitutes the public contract — context tokens, types, constants, and global component classes:

**Manifests are NOT exported from `index.ts`** — they are registered internally by the package's startup mechanism, not consumed as imports by other packages.

---

## Building Elements with Local Components

```typescript
import { customElement, html, css, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

// Import local components — private to this sub-feature
import './local-components/feature-a.element.js';

@customElement('umb-feature')
export class UmbFeatureElement extends UmbLitElement {
  render() {
    return html`<ubm-feature-a></umb-feature-a>`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    ['umb-feature']: UmbFeatureElement;
  }
}
```

---

## Registering Package Module Export

To make a new package module importable by other packages:

1. Add the subpath to `package.json` exports:

```json
{
  "exports": {
    "./module": "./dist-cms/packages/package/module/index.js"
  }
}
```

2. Run the generators:

```bash
npm run i
npm run generate:tsconfig
```

This single edit ensures TypeScript, the browser import map, and test configs all know about your new package. See [Architecture — Import Map & Code Generation Pipeline](./architecture.md#import-map--code-generation-pipeline) for details.

---

## Conventions & Rules

1. **Never import from `dist-cms/` directly.** Always use the `@umbraco-cms/backoffice/<subpath>` form.
2. **Every new module export must be added to `package.json` exports** and then regenerated.
3. **Use `UmbLitElement` or `UmbElementMixin`** as your base class — never raw `LitElement` — to get Context API, localization, and lifecycle support.
4. **Export `api`** from any file that provides a class to be instantiated by the extension registry (e.g., workspace contexts, actions). The extension system expects a named `api` export.
5. **Use element lazy loading** in manifests via `element: () => import('./path.js')` for code splitting.
6. **Use the Umbraco UI Library (`@umbraco-ui/uui`)** components for UI consistency: `<uui-box>`, `<uui-button>`, `<uui-input>`, `<uui-table>`, etc.
7. **Declare custom Extension Manifest Types** on the global `UmbExtensionManifestMap` interface so other packages can extend yours.
8. **Localization keys** should be registered via `localization` extension manifests, not hardcoded strings.
9. **Naming convention**: Element tag names use `umb-` prefix for core (e.g., `umb-dashboard`). Package developers should use their own prefix.
10. **Extension aliases** follow a dot-separated namespace: `Umb.Section.Content`, `My.Dashboard.Analytics`.
