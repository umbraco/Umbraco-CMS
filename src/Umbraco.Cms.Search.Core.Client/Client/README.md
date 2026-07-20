# @umbraco-cms/search

TypeScript type definitions and runtime constants for extending the [Umbraco Search](https://docs.umbraco.com/umbraco-search/) backoffice.

This package ships **types only** — no runtime code is bundled. Runtime symbols (contexts, constants) are resolved in the Umbraco backoffice via the import map provided by the Umbraco Search Core Client. Install this package as a `devDependency` to get type safety and IntelliSense when authoring search extensions and providers.

## Installation

```bash
npm install --save-dev @umbraco-cms/search
```

> **Prereleases** are published under the `next` dist-tag:
>
> ```bash
> npm install --save-dev @umbraco-cms/search@next
> ```

**Peer dependency:** `@umbraco-cms/backoffice >= 17.0.0`

## Entry points

The package exposes two subpath entry points:

| Import path                    | Contents                                                                                 |
| ------------------------------ | ---------------------------------------------------------------------------------------- |
| `@umbraco-cms/search/global`   | Entity-type constants, workspace aliases, and shared manifest types.                     |
| `@umbraco-cms/search/settings` | Workspace context tokens (`UMB_SEARCH_WORKSPACE_CONTEXT`), repositories, and view types. |

## Usage

### Entity actions for search documents

```typescript
import { UMB_SEARCH_DOCUMENT_ENTITY_TYPE } from '@umbraco-cms/search/global';

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: 'entityAction',
    kind: 'default',
    alias: 'My.EntityAction.SearchDocument',
    name: 'My Search Document Action',
    api: () => import('./my-action.js'),
    forEntityTypes: [UMB_SEARCH_DOCUMENT_ENTITY_TYPE],
    meta: { icon: 'icon-search', label: 'My Action' },
  },
];
```

### Consuming the workspace context

```typescript
import { UMB_SEARCH_WORKSPACE_CONTEXT } from '@umbraco-cms/search/settings';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

export class MyBoxElement extends UmbLitElement {
  constructor() {
    super();
    this.consumeContext(UMB_SEARCH_WORKSPACE_CONTEXT, (context) => {
      const indexAlias = context.getUnique();
      this.observe(context.documentCount, (count) => {
        // ...
      });
    });
  }
}
```

### Detail boxes, workspace views, routable modals

The Search backoffice exposes additional extension points (`searchIndexDetailBox`, workspace views targeting `UMB_SEARCH_WORKSPACE_ALIAS`, and routable-modal patterns for deep-linkable document detail pages). See the full guide below.

## Documentation

The complete guide for extending the Search backoffice — including detail boxes, workspace views, routable modals, and cross-package type augmentation — lives in the Umbraco docs:

**[Extending the Search Backoffice →](https://docs.umbraco.com/umbraco-search/extending/backoffice-extensions)**

## Source

This package is generated from the Core Client workspace in [umbraco/Umbraco.Cms.Search](https://github.com/umbraco/Umbraco.Cms.Search). Issues and contributions welcome.

## License

MIT
