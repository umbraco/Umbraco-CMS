# Package Development

---

How to structure, organize, and build packages within the Umbraco Backoffice frontend. For architecture concepts (extension registry, context API, repository pattern), see [Architecture](./architecture.md). For import boundary rules, see [Developer Roles](./architecture.md#developer-roles--import-boundaries).

---

## Package & Module Structure

Each top-level folder under `src/packages/` is a **package root**. A package contains one or more **modules**, each in its own subfolder.

```
src/packages/media/          <- package root
├── media/                   <- module (media management)
│   ├── index.ts
│   ├── manifests.ts
│   └── ...
├── imaging/                 <- module (image processing)
│   ├── index.ts
│   ├── manifests.ts
│   └── ...
├── manifests.ts             <- aggregates all module manifests
└── umbraco-package.ts       <- bundle entry point
```

### Public vs. Private Modules

A module becomes public by adding an import map entry in the root `package.json` exports field (see [Registering Package Module Export](#registering-package-module-export)). Only export modules with a clear external use case.

- The public API (`index.ts` + `package.json` export) is a contract with consumers.
- Keep code private until needed — non-exported code can be freely refactored.
- Once exported, changes require the [deprecation process](./deprecation.md).

---

## Folder Structure Conventions

**Sub-folders are organized by sub-feature, not by technical role.** Each sub-feature folder contains everything related to that feature — elements, repository, store, modals, actions, constants, types, and manifests. No top-level `repositories/`, `modals/`, or `actions/` folders.

- Every sub-feature is self-contained (full vertical slice). Deleting the folder should break nothing outside it (apart from the missing manifest registration).
- Every folder with registerable items has its own `manifests.ts`. Child manifests bubble up to the package root.
- Every sub-feature has `index.ts`, `manifests.ts`, and `constants.ts` at minimum. Add `types.ts` for shared types.

### `local-components/`

Elements only used within a sub-feature go in `local-components/`. Never exported — private implementation details.

### `global-components/`

Reusable elements for other packages. Must be registered as custom elements at startup and exported from the package's `index.ts`.

```typescript
@customElement('umb-feature')
export class UmbFeatureElement extends UmbLitElement {
  render() { return html`Feature Element`; }
}

declare global {
  interface HTMLElementTagNameMap {
    ['umb-feature']: UmbFeatureElement;
  }
}
```

### Manifest Bubbling

Each sub-feature exports its own `manifests` array. The package-level `manifests.ts` aggregates them:

```typescript
import { manifests as sectionManifests } from './section/manifests.js';
import { manifests as dashboardManifests } from './dashboard/manifests.js';
export const manifests = [...sectionManifests, ...dashboardManifests];
```

---

## The Package's Public API (index.ts)

Only export what constitutes the public contract — context tokens, types, constants, and global component classes. **Manifests are NOT exported from `index.ts`** — they are registered internally via the bundle mechanism.

---

## Registering Package Module Export

To make a module importable by other packages:

1. Add the subpath to `package.json` exports:
```json
{ "exports": { "./module": "./dist-cms/packages/package/module/index.js" } }
```

2. Run the generators:
```bash
npm i && npm run generate:tsconfig
```

This ensures TypeScript, the browser import map, and test configs all resolve the new package.

---

## Localization

No hardcoded UI-facing strings. All user-visible text must go through the localization system. Keys live in `src/assets/lang/en.ts`, grouped by feature area, referenced as `group_keyName` (e.g., `this.localize.term('actions_create')`).

For step-by-step instructions on adding localization keys and using them in elements or controllers, use the `general-add-localization` skill.

---

## Conventions & Rules

1. **Never import from `dist-cms/` directly.** Always use `@umbraco-cms/backoffice/<subpath>`.
2. **Every new module export must be added to `package.json` exports** and regenerated.
3. **Use `UmbLitElement` or `UmbElementMixin`** as base class — never raw `LitElement`.
4. **Export `api`** from files providing a class for the extension registry to instantiate (workspace contexts, actions).
5. **Use element lazy loading** in manifests: `element: () => import('./path.js')`.
6. **Use `@umbraco-ui/uui` components** for UI consistency (`<uui-box>`, `<uui-button>`, `<uui-input>`, etc.).
7. **Declare custom manifest types** on `UmbExtensionManifestMap` so other packages can extend yours.
8. **No hardcoded UI strings** — use the localization system. See [Localization](#localization) above and the `general-add-localization` skill.
9. **Element naming**: `umb-` prefix for core. Package developers use their own prefix.
10. **Extension aliases**: Dot-separated namespace (`Umb.Section.Content`, `My.Dashboard.Analytics`).
