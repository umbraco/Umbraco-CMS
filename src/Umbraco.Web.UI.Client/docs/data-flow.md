# Data Flow & API Integration

How data moves between the server and UI components. This covers the internal implementation pattern using base classes — the same pattern used across all entity types in the backoffice. For how repositories are categorized, organized, and structured across features, see [Repositories](./repositories.md).

---

## The Data Flow Chain

```
Element ──observe()──> Workspace Context ──> Repository ──> Data Source ──> Generated API Client ──> Server
   ↑                        ↑                    ↑
   └── @state updates ──────┘                    │
                                                 ↓
                                              Store (cache)
```

Each layer has a single responsibility:

| Layer | Responsibility |
|-------|---------------|
| **Element** | Renders UI, handles user input |
| **Context** | Coordinates data and lifecycle for a feature |
| **Repository** | Orchestrates data source + store |
| **Data Source** | Maps between server API and domain models |
| **Store** | Caches entities, provides observables (see note below) |
| **Generated Client** | Typed HTTP calls from OpenAPI spec |

Each layer has domain-specific base classes depending on the type of data operation (detail CRUD, item listing, collection, tree, etc.). See [Repositories](./repositories.md) for a complete guide to all repository categories and when to use each. The example below uses the **detail** variant.

### Store Direction

Stores are being phased out. Only **Detail Store** and **Item Store** remain in active use. Do not introduce new store types. New features should manage state within the context/workspace layer instead, using observable state classes directly. The existing detail and item stores will be addressed in a future version.

---

## Generated API Client

API clients are auto-generated from the backend OpenAPI specification.

### Generation

```bash
npm run generate:server-api
```

This reads `src/packages/core/openapi-ts.config.ts` and outputs typed service classes and models to `src/packages/core/backend-api/`.

### Usage

Generated services are imported from the `external/backend-api` subpath:

```typescript
import { WebhookService } from '@umbraco-cms/backoffice/external/backend-api';
import type { CreateWebhookRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
```

**Never call generated services directly from elements or contexts.** Always go through a data source and repository.

---

## Caching & Request Deduplication

Caching and request deduplication live **between the data source and the API client** — not in the repository or context. This is the layer that knows the API transport details needed to deduplicate and cache effectively.

```
Data Source ──> Request Manager ──> Generated API Client
                   ├─ Data cache: returns cached items immediately
                   ├─ Inflight cache: deduplicates concurrent requests for the same data
                   └─ Server event sync: invalidates cache when the server broadcasts changes
```

The `management-api` package provides base classes for this: `UmbManagementApiItemDataRequestManager` handles the caching/deduplication logic, and concrete implementations wire it to a specific API endpoint and cache instance. See the document, media, and member item data sources for examples.

This pattern is specific to the Management API transport. Other data sources (e.g., a custom API or offline source) would implement their own caching strategy in the same position — between data source and transport.

---

## tryExecute

The standard wrapper for all API calls. Handles errors, shows notifications, and returns a `{ data, error }` tuple — never throws.

```typescript
import { tryExecute } from '@umbraco-cms/backoffice/resources';

const { data, error } = await tryExecute(
	this.#host,
	WebhookService.getWebhookById({ path: { id: unique } }),
);

if (error || !data) {
	return { error };
}
```

**Parameters:**
- `host` — Controller host (for notification context access)
- `promise` — The API call promise
- `opts?` — Optional: `{ abortSignal?, disableNotifications? }`

**Behavior:**
- Returns `{ data }` on success
- Returns `{ error }` on failure (never throws)
- Automatically shows error notifications to the user
- Silently handles 401/403/404 (UI is expected to handle these)

---

## Detail Data Flow Example

A generic example showing the structural pattern for each layer. Replace `MyEntity` with the actual entity name. When implementing, find an existing entity in the codebase that is similar to your task and follow its patterns.

### 1. Domain Model

```typescript
export interface UmbMyEntityDetailModel {
	entityType: string;
	unique: string;
	name: string;
	// ... domain-specific properties
}
```

### 2. Data Source

Implements the data source interface for the domain. Responsible for calling the generated API client and mapping between server types and the domain model.

```typescript
import type { UmbMyEntityDetailModel } from '../../types.js';
import { UMB_MY_ENTITY_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import { MyEntityService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbMyEntityDetailServerDataSource implements UmbDetailDataSource<UmbMyEntityDetailModel> {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async createScaffold(preset: Partial<UmbMyEntityDetailModel> = {}) {
		const data: UmbMyEntityDetailModel = {
			entityType: UMB_MY_ENTITY_ENTITY_TYPE,
			unique: UmbId.new(),
			name: '',
			// ... defaults for domain-specific properties
			...preset,
		};
		return { data };
	}

	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(
			this.#host,
			MyEntityService.getMyEntityById({ path: { id: unique } }),
		);

		if (error || !data) {
			return { error };
		}

		// Map: server response → domain model
		const model: UmbMyEntityDetailModel = {
			entityType: UMB_MY_ENTITY_ENTITY_TYPE,
			unique: data.id,
			name: data.name ?? '',
			// ... map domain-specific properties
		};

		return { data: model };
	}

	async create(model: UmbMyEntityDetailModel) {
		if (!model) throw new Error('Model is missing');

		// Map: domain model → server request
		const body = {
			id: model.unique,
			name: model.name,
			// ... map domain-specific properties
		};

		const { data, error } = await tryExecute(
			this.#host,
			MyEntityService.postMyEntity({ body }),
		);

		if (data) {
			return this.read(data as never);
		}
		return { error };
	}

	async update(model: UmbMyEntityDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		const body = {
			name: model.name,
			// ... map domain-specific properties
		};

		const { error } = await tryExecute(
			this.#host,
			MyEntityService.putMyEntityById({ path: { id: model.unique }, body }),
		);

		if (!error) {
			return this.read(model.unique);
		}
		return { error };
	}

	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecute(
			this.#host,
			MyEntityService.deleteMyEntityById({ path: { id: unique } }),
		);
	}
}
```

**Key pattern:** The data source is the only layer that knows about server API types. It maps in both directions — server → domain on reads, domain → server on writes.

### 3. Store

```typescript
import type { UmbMyEntityDetailModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMyEntityDetailStore extends UmbDetailStoreBase<UmbMyEntityDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_MY_ENTITY_DETAIL_STORE_CONTEXT.toString());
	}
}

export default UmbMyEntityDetailStore;

export const UMB_MY_ENTITY_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbMyEntityDetailStore>(
	'UmbMyEntityDetailStore',
);
```

### 4. Repository

Wires data source + store together. The base class handles all CRUD orchestration.

```typescript
import type { UmbMyEntityDetailModel } from '../../types.js';
import { UmbMyEntityDetailServerDataSource } from './my-entity-detail.server.data-source.js';
import { UMB_MY_ENTITY_DETAIL_STORE_CONTEXT } from './my-entity-detail.store.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbMyEntityDetailRepository extends UmbDetailRepositoryBase<UmbMyEntityDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbMyEntityDetailServerDataSource, UMB_MY_ENTITY_DETAIL_STORE_CONTEXT);
	}
}

export default UmbMyEntityDetailRepository;
```

### 5. Workspace Context

Manages entity lifecycle and exposes observable state for elements.

```typescript
import { UMB_MY_ENTITY_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_MY_ENTITY_ENTITY_TYPE, UMB_MY_ENTITY_WORKSPACE_ALIAS } from '../../entity.js';
import type { UmbMyEntityDetailModel } from '../types.js';
import { UmbEntityNamedDetailWorkspaceContextBase } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMyEntityWorkspaceContext
	extends UmbEntityNamedDetailWorkspaceContextBase<UmbMyEntityDetailModel>
{
	// Derived observables — only emit when the specific property changes
	readonly myProperty = this._data.createObservablePartOfCurrent((data) => data?.myProperty);

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_MY_ENTITY_WORKSPACE_ALIAS,
			entityType: UMB_MY_ENTITY_ENTITY_TYPE,
			detailRepositoryAlias: UMB_MY_ENTITY_DETAIL_REPOSITORY_ALIAS,
		});
	}

	// Setters mutate the current data — triggers observable updates
	setMyProperty(value: string) {
		this._data.updateCurrent({ myProperty: value });
	}
}

export { UmbMyEntityWorkspaceContext as api };
```

### 6. Element

Consumes context, observes data, renders UI, dispatches user actions back to context.

```typescript
import { UMB_MY_ENTITY_WORKSPACE_CONTEXT } from '../my-entity-workspace.context-token.js';
import type { UmbMyEntityDetailModel } from '../../types.js';
import { customElement, html, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-my-entity-workspace-view')
export class UmbMyEntityWorkspaceViewElement extends UmbLitElement {
	@state()
	private _data?: UmbMyEntityDetailModel;

	#workspaceContext?: typeof UMB_MY_ENTITY_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_MY_ENTITY_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;

			this.observe(context.data, (data) => {
				this._data = data;
			});
		});
	}

	#onChange(event: Event) {
		const value = (event.target as HTMLInputElement).value;
		this.#workspaceContext?.setMyProperty(value);
	}

	override render() {
		if (!this._data) return nothing;

		return html`
			<uui-box>
				<umb-property-layout label=${this.localize.term('myEntity_myProperty')}>
					<uui-input
						slot="editor"
						.value=${this._data.myProperty}
						@input=${this.#onChange}></uui-input>
				</umb-property-layout>
			</uui-box>
		`;
	}
}
```

### 7. Manifests

```typescript
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: 'Umb.Repository.MyEntity.Detail',
		name: 'My Entity Detail Repository',
		api: () => import('./my-entity-detail.repository.js'),
	},
	{
		type: 'store',
		alias: 'Umb.Store.MyEntity.Detail',
		name: 'My Entity Detail Store',
		api: () => import('./my-entity-detail.store.js'),
	},
];
```

Manifests are lazy-loaded — the `api: () => import(...)` pattern ensures code is only loaded when needed.

---

## Key Rules

1. **Never call generated API services directly from elements or contexts** — always go through data source → repository
2. **Always use `tryExecute()` for API calls** — it handles errors and notifications
3. **Data sources handle model mapping** — they translate between backend API types and frontend domain models
4. **All return values use `{ data, error }` tuples** — never throw from async data operations
5. **Export `default` from repositories, stores, and data sources** — this enables lazy-loading via `api: () => import(...)`
6. **Do not introduce new store types** — stores are being phased out. Manage state in the context layer using observable state classes directly
