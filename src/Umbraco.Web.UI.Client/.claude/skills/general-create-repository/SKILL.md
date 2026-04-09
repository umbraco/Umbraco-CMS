---
name: general-create-repository
description: Create or extend a repository in the Umbraco backoffice. Covers detail (CRUD), item (batch lookup), collection (paginated list), and action-specific (publish, duplicate, move, etc.) repositories. Use when the user says "create a repository", "add a data source", or when a feature needs to fetch or post data. Each repository type has its own template — pick the right one based on the operation.
allowed-tools: Read, Write, Edit, Grep, Glob
---

# Create Repository

Create or extend a repository and its data source for an entity feature in the Umbraco backoffice.

## Foundational documentation

Read these before creating a repository — they define the conventions this skill builds on:

- **[Repositories](../../../docs/repositories.md)** — Repository categories, file structure, naming, extension registration, when to use which type
- **[Data Flow](../../../docs/data-flow.md)** — Full data flow chain, tryExecute, model mapping, store direction
- **[Package Development](../../../docs/package-development.md)** — Folder conventions, vertical slices, public API rules

## What you need from the user

1. **Entity name** — singular, kebab-case (e.g., `webhook`, `document`, `data-type`)
2. **Repository type** — `detail`, `item`, `collection`, or `action-specific`
3. **Package path** — which package directory (e.g., `src/packages/webhook/webhook/`)

**Additional for action-specific:**
4. **Action name** — what the operation does (e.g., `duplicate`, `move-to`, `publishing`, `culture-and-hostnames`)
5. **Methods** — what operations the repository exposes

## Choosing the right type

See [Repositories — When to Create Which](../../../docs/repositories.md#when-to-create-which-repository-type) for the decision matrix.

---

## Option A: Detail Repository

The most common repository type. Handles full CRUD lifecycle with store caching.

### Prerequisites

- Entity type constant in `entity.ts`
- Detail model type in `types.ts` (must extend `UmbEntityModel` — needs `entityType` and `unique` fields)
- Generated API client available for the entity

### Files to create

```
{package-path}/repository/detail/
├── {entity}-detail.repository.ts
├── {entity}-detail.server.data-source.ts
├── {entity}-detail.store.ts
├── {entity}-detail.store.context-token.ts
├── constants.ts
└── manifests.ts
```

### Step 1: Create store context token

File: `{entity}-detail.store.context-token.ts`

```typescript
import type { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { Umb{EntityName}DetailModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_{ENTITY}_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbDetailStoreBase<Umb{EntityName}DetailModel>>(
	'Umb{EntityName}DetailStore',
);
```

### Step 2: Create store

File: `{entity}-detail.store.ts`

```typescript
import type { Umb{EntityName}DetailModel } from '../../types.js';
import { UMB_{ENTITY}_DETAIL_STORE_CONTEXT } from './{entity}-detail.store.context-token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class Umb{EntityName}DetailStore extends UmbDetailStoreBase<Umb{EntityName}DetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_{ENTITY}_DETAIL_STORE_CONTEXT.toString());
	}
}

export { Umb{EntityName}DetailStore as api };
```

### Step 3: Create server data source

File: `{entity}-detail.server.data-source.ts`

The data source maps between server API types and domain models. See [Data Flow](../../../docs/data-flow.md) for context on how this fits in the chain.

```typescript
import type { Umb{EntityName}DetailModel } from '../../types.js';
import { UMB_{ENTITY}_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import { {EntityName}Service } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class Umb{EntityName}DetailServerDataSource implements UmbDetailDataSource<Umb{EntityName}DetailModel> {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async createScaffold(preset: Partial<Umb{EntityName}DetailModel> = {}) {
		const data: Umb{EntityName}DetailModel = {
			entityType: UMB_{ENTITY}_ENTITY_TYPE,
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
			{EntityName}Service.get{EntityName}ById({ path: { id: unique } }),
		);

		if (error || !data) {
			return { error };
		}

		const model: Umb{EntityName}DetailModel = {
			entityType: UMB_{ENTITY}_ENTITY_TYPE,
			unique: data.id,
			name: data.name ?? '',
			// ... map domain-specific properties from server response
		};

		return { data: model };
	}

	async create(model: Umb{EntityName}DetailModel, parentUnique: string | null) {
		if (!model) throw new Error('Model is missing');

		const body = {
			id: model.unique,
			name: model.name,
			// ... map domain-specific properties to server request
		};

		const { data, error } = await tryExecute(
			this.#host,
			{EntityName}Service.post{EntityName}({ body }),
		);

		if (data) {
			return this.read(data as never);
		}
		return { error };
	}

	async update(model: Umb{EntityName}DetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		const body = {
			name: model.name,
			// ... map domain-specific properties to server request
		};

		const { error } = await tryExecute(
			this.#host,
			{EntityName}Service.put{EntityName}ById({ path: { id: model.unique }, body }),
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
			{EntityName}Service.delete{EntityName}ById({ path: { id: unique } }),
		);
	}
}
```

**Important:** Find the actual generated API service name and method names by checking `@umbraco-cms/backoffice/external/backend-api`. The names above are illustrative — the real generated service may differ.

### Step 4: Create repository

File: `{entity}-detail.repository.ts`

```typescript
import type { Umb{EntityName}DetailModel } from '../../types.js';
import { Umb{EntityName}DetailServerDataSource } from './{entity}-detail.server.data-source.js';
import { UMB_{ENTITY}_DETAIL_STORE_CONTEXT } from './{entity}-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class Umb{EntityName}DetailRepository extends UmbDetailRepositoryBase<Umb{EntityName}DetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, Umb{EntityName}DetailServerDataSource, UMB_{ENTITY}_DETAIL_STORE_CONTEXT);
	}
}

export { Umb{EntityName}DetailRepository as api };
```

### Step 5: Create constants

File: `constants.ts`

```typescript
export const UMB_{ENTITY}_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.{EntityName}.Detail';
export const UMB_{ENTITY}_DETAIL_STORE_ALIAS = 'Umb.Store.{EntityName}.Detail';
```

### Step 6: Create manifests

File: `manifests.ts`

```typescript
import { UMB_{ENTITY}_DETAIL_REPOSITORY_ALIAS, UMB_{ENTITY}_DETAIL_STORE_ALIAS } from './constants.js';
import { Umb{EntityName}DetailStore } from './{entity}-detail.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_{ENTITY}_DETAIL_REPOSITORY_ALIAS,
		name: '{EntityName} Detail Repository',
		api: () => import('./{entity}-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_{ENTITY}_DETAIL_STORE_ALIAS,
		name: '{EntityName} Detail Store',
		api: Umb{EntityName}DetailStore,
	},
];
```

### Step 7: Wire manifests into parent

Import and spread in the parent module's `manifests.ts`:

```typescript
import { manifests as detailRepositoryManifests } from './repository/detail/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...detailRepositoryManifests,
	// ... other manifests
];
```

### Detail repository checklist

- [ ] Detail model type exists in `types.ts` with `entityType` and `unique` fields
- [ ] Entity type constant exists in `entity.ts`
- [ ] Store context token created
- [ ] Store class extends `UmbDetailStoreBase<T>`
- [ ] Data source implements `UmbDetailDataSource<T>` with all 5 methods
- [ ] Data source maps server types ↔ domain model in both directions
- [ ] Data source uses `tryExecute()` for all API calls
- [ ] Repository extends `UmbDetailRepositoryBase<T>`, passing data source class + store context
- [ ] Repository and store exported as `api` (for lazy-loading)
- [ ] Manifest aliases defined as constants
- [ ] Manifests registered with `type: 'repository'` and `type: 'store'`
- [ ] Manifests wired into parent module's `manifests.ts`

---

## Option B: Item Repository

For batch-fetching lightweight display info (name, icon, entity type) by unique IDs. Used by pickers, reference lists, and breadcrumbs.

### Prerequisites

- Item model type in `types.ts` (needs `unique` field)
- Generated API client with a batch/list endpoint

### Files to create

```
{package-path}/item/repository/
├── {entity}-item.repository.ts
├── {entity}-item.server.data-source.ts
├── {entity}-item.store.ts
├── {entity}-item.store.context-token.ts
├── constants.ts
└── manifests.ts
```

### Step 1: Create item store context token

File: `{entity}-item.store.context-token.ts`

```typescript
import type { UmbItemStore } from '@umbraco-cms/backoffice/store';
import type { Umb{EntityName}ItemModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_{ENTITY}_ITEM_STORE_CONTEXT = new UmbContextToken<UmbItemStore<Umb{EntityName}ItemModel>>(
	'Umb{EntityName}ItemStore',
);
```

### Step 2: Create item store

File: `{entity}-item.store.ts`

```typescript
import type { Umb{EntityName}ItemModel } from '../../types.js';
import { UMB_{ENTITY}_ITEM_STORE_CONTEXT } from './{entity}-item.store.context-token.js';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class Umb{EntityName}ItemStore extends UmbItemStoreBase<Umb{EntityName}ItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_{ENTITY}_ITEM_STORE_CONTEXT.toString());
	}
}

export { Umb{EntityName}ItemStore as api };
```

### Step 3: Create item server data source

File: `{entity}-item.server.data-source.ts`

```typescript
import type { Umb{EntityName}ItemModel } from '../../types.js';
import { UMB_{ENTITY}_ENTITY_TYPE } from '../../entity.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { {EntityName}ItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { {EntityName}Service } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class Umb{EntityName}ItemServerDataSource extends UmbItemServerDataSourceBase<
	{EntityName}ItemResponseModel,
	Umb{EntityName}ItemModel
> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (uniques) => {EntityName}Service.getItem{EntityName}({ query: { id: uniques } }),
			mapper: (item) => ({
				entityType: UMB_{ENTITY}_ENTITY_TYPE,
				unique: item.id,
				name: item.name ?? '',
				// ... map domain-specific item properties
			}),
		});
	}
}
```

### Step 4: Create item repository

File: `{entity}-item.repository.ts`

```typescript
import type { Umb{EntityName}ItemModel } from '../../types.js';
import { Umb{EntityName}ItemServerDataSource } from './{entity}-item.server.data-source.js';
import { UMB_{ENTITY}_ITEM_STORE_CONTEXT } from './{entity}-item.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class Umb{EntityName}ItemRepository extends UmbItemRepositoryBase<Umb{EntityName}ItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, Umb{EntityName}ItemServerDataSource, UMB_{ENTITY}_ITEM_STORE_CONTEXT);
	}
}

export { Umb{EntityName}ItemRepository as api };
```

### Step 5: Create constants and manifests

Follow the same pattern as detail (Step 5–6), using `Item` instead of `Detail` in aliases.

### Item repository checklist

- [ ] Item model type exists in `types.ts` with `unique` field
- [ ] Store context token, store class, data source, and repository created
- [ ] Data source extends `UmbItemServerDataSourceBase` with `getItems` and `mapper`
- [ ] Repository extends `UmbItemRepositoryBase<T>`
- [ ] Manifests registered and wired into parent

---

## Option C: Collection Repository

For paginated/filtered listings. No base class — implement the `UmbCollectionRepository` interface on `UmbRepositoryBase`.

### Files to create

```
{package-path}/collection/repository/
├── {entity}-collection.repository.ts
├── {entity}-collection.server.data-source.ts
└── manifests.ts
```

### Step 1: Create collection server data source

File: `{entity}-collection.server.data-source.ts`

```typescript
import type { Umb{EntityName}CollectionFilterModel } from '../types.js';
import { {EntityName}Service } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class Umb{EntityName}CollectionServerDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async getCollection(filter: Umb{EntityName}CollectionFilterModel) {
		const { data, error } = await tryExecute(
			this.#host,
			{EntityName}Service.get{EntityName}({ query: { skip: filter.skip, take: filter.take } }),
		);

		if (error || !data) {
			return { error };
		}

		return {
			data: {
				items: data.items.map((item) => ({
					// ... map to collection item model
				})),
				total: data.total,
			},
		};
	}
}
```

### Step 2: Create collection repository

File: `{entity}-collection.repository.ts`

```typescript
import { Umb{EntityName}CollectionServerDataSource } from './{entity}-collection.server.data-source.js';
import type { Umb{EntityName}CollectionFilterModel } from '../types.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';

export class Umb{EntityName}CollectionRepository
	extends UmbRepositoryBase
	implements UmbCollectionRepository
{
	#collectionSource = new Umb{EntityName}CollectionServerDataSource(this);

	async requestCollection(filter: Umb{EntityName}CollectionFilterModel) {
		return this.#collectionSource.getCollection(filter);
	}
}

export { Umb{EntityName}CollectionRepository as api };
```

### Collection repository checklist

- [ ] Collection filter model type defined
- [ ] Data source maps server response to collection item models
- [ ] Repository implements `UmbCollectionRepository` interface
- [ ] Manifests registered and wired into parent

---

## Option D: Action-Specific Repository

For domain operations that don't fit CRUD — publish, duplicate, move, sort, recycle bin, etc.

### Prerequisites

- Entity type constant
- Generated API client with the relevant endpoint

### Files to create

```
{package-path}/{action-location}/repository/
├── {entity}-{action}.repository.ts
├── {entity}-{action}.server.data-source.ts
├── types.ts                              # Optional — args/response types
└── manifests.ts
```

**Location rules:**
- Entity actions: `entity-actions/{action-name}/repository/`
- Bulk actions: `entity-bulk-actions/{action-name}/repository/`
- Domain sub-features: `{feature-name}/repository/` (e.g., `publishing/repository/`)

### Step 1: Create types (if needed)

File: `types.ts`

```typescript
export interface Umb{Action}{EntityName}RequestArgs {
	unique: string;
	// ... action-specific arguments
}
```

### Step 2: Create server data source

File: `{entity}-{action}.server.data-source.ts`

```typescript
import type { Umb{Action}{EntityName}RequestArgs } from './types.js';
import { {EntityName}Service } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class Umb{Action}{EntityName}ServerDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async {actionMethod}(args: Umb{Action}{EntityName}RequestArgs) {
		if (!args.unique) throw new Error('Unique is missing');

		return tryExecute(
			this.#host,
			{EntityName}Service.{apiMethod}({
				path: { id: args.unique },
				body: {
					// ... map args to server request
				},
			}),
		);
	}
}
```

### Step 3: Create repository

File: `{entity}-{action}.repository.ts`

```typescript
import { Umb{Action}{EntityName}ServerDataSource } from './{entity}-{action}.server.data-source.js';
import type { Umb{Action}{EntityName}RequestArgs } from './types.js';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class Umb{Action}{EntityName}Repository extends UmbRepositoryBase {
	#{action}Source = new Umb{Action}{EntityName}ServerDataSource(this);

	async request{Action}(args: Umb{Action}{EntityName}RequestArgs) {
		const { data, error } = await this.#{action}Source.{actionMethod}(args);

		if (!error) {
			const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
			const notification = { data: { message: `{Action} completed` } };
			notificationContext.peek('positive', notification);
		}

		return { data, error };
	}
}

export { Umb{Action}{EntityName}Repository as api };
```

### Step 4: Create manifests

File: `manifests.ts`

```typescript
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: 'Umb.Repository.{EntityName}.{Action}',
		name: '{EntityName} {Action} Repository',
		api: () => import('./{entity}-{action}.repository.js'),
	},
];
```

### Action-specific repository checklist

- [ ] Args type defined (if the action has parameters beyond unique)
- [ ] Data source handles API call with `tryExecute()`
- [ ] Repository extends `UmbRepositoryBase` or `UmbControllerBase`
- [ ] Repository methods validate required arguments
- [ ] Success notification shown where appropriate
- [ ] Repository exported as `api` for lazy-loading
- [ ] Manifest registered and wired into parent

---

## Reference examples

See [Repositories — Reference Examples](../../../docs/repositories.md#reference-examples) for real implementations to study.
