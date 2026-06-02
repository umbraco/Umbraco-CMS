---
name: general-create-workspace
description: Create a new workspace for an entity in the Umbraco backoffice. Supports two workspace types — default (simple, manifest-only, for root/listing pages) and routable (entity detail editing with create/edit flows). Use when the user says "create a workspace", "add a workspace for X", or "scaffold a workspace". For routable workspaces, the entity must already have a package, entity type, repository, and data source.
allowed-tools: Read, Write, Edit, Grep, Glob
---

# Create Workspace

Create a workspace in the Umbraco backoffice. Two workspace types are available:

- **Default workspace** — manifest-only, no custom code. For root pages, listing pages, settings.
- **Routable workspace** — custom context class with create/edit URL routing. For entity detail editing with full CRUD lifecycle.

## What you need from the user

1. **Workspace type** — `default` (root/listing) or `routable` (entity detail editing)
2. **Entity name** — singular, kebab-case (e.g., `webhook`, `data-type`, `language`)
3. **Package path** — which package directory (e.g., `src/packages/webhook/webhook/`)

**Additional for routable workspaces:**
4. **Has parent entity** — whether creation requires a parent (tree entities like data-type) or not (flat entities like webhook)
5. **Entity properties** — what observable properties the workspace should expose beyond `name` and `unique`

---

## Option A: Default Workspace

The simplest workspace. Just a manifest — no custom context class or element. The `default` kind provides `UmbDefaultWorkspaceContext` and `<umb-default-workspace>` automatically.

### Prerequisites

- Entity type constant in `entity.ts` (e.g., `UMB_WEBHOOK_ROOT_ENTITY_TYPE`)
- Workspace alias constant (e.g., `UMB_WEBHOOK_ROOT_WORKSPACE_ALIAS`)

### Files to create

Just a `manifests.ts` (can be in an existing manifests file or a new `workspace/` directory):

```typescript
import { UMB_{ENTITY}_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_{ENTITY}_ROOT_WORKSPACE_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'default',
		alias: UMB_{ENTITY}_ROOT_WORKSPACE_ALIAS,
		name: '{EntityName} Root Workspace',
		meta: {
			entityType: UMB_{ENTITY}_ROOT_ENTITY_TYPE,
			headline: '#treeHeaders_{entityPlural}',
		},
	},
	// Optional: add a collection view to list child entities
	{
		type: 'workspaceView',
		kind: 'collection',
		alias: 'Umb.WorkspaceView.{EntityName}Root.Collection',
		name: '{EntityName} Root Collection Workspace View',
		meta: {
			label: 'Collection',
			pathname: 'collection',
			icon: 'icon-layers',
			collectionAlias: UMB_{ENTITY}_COLLECTION_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_{ENTITY}_ROOT_WORKSPACE_ALIAS,
			},
		],
	},
];
```

### Default workspace checklist

- [ ] Workspace alias constant defined
- [ ] Entity type constant defined
- [ ] Manifest has `kind: 'default'`, `meta.entityType`, and `meta.headline`
- [ ] Workspace views use `UMB_WORKSPACE_CONDITION_ALIAS` condition
- [ ] Manifests wired into parent module's `manifests.ts`

---

## Option B: Routable Workspace (Entity Detail)

For entity detail editing with create/edit URL routing, validation, save flow, and data management.

### Prerequisites

Before creating a routable workspace, the entity must already have:

- Entity type constants in `entity.ts` (constant + root entity type + workspace alias)
- Detail model type (e.g., `UmbWebhookDetailModel`) in `types.ts`
- Detail repository + data source + store (see [Data Flow](../../../docs/data-flow.md))
- Repository manifest registered

Verify these exist before proceeding. If the package doesn't exist yet, use the `general-create-package` skill first.

### Files to create

```
{package-path}/workspace/
├── constants.ts                              # Re-export context token
├── {entity}-workspace.context-token.ts       # Typed context token
├── {entity}-workspace.context.ts             # Workspace context class
├── {entity}-workspace-editor.element.ts      # Editor shell element
├── manifests.ts                              # workspace + workspaceView + workspaceAction
└── views/
    └── {entity}-details-workspace-view.element.ts  # Main detail view
```

## Step 1: Create context token

File: `{entity}-workspace.context-token.ts`

```typescript
import type { Umb{EntityName}WorkspaceContext } from './{entity}-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_{ENTITY}_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSubmittableWorkspaceContext,
	Umb{EntityName}WorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is Umb{EntityName}WorkspaceContext => context.getEntityType?.() === '{entity-type}',
);
```

## Step 2: Create constants.ts

File: `constants.ts`

```typescript
export { UMB_{ENTITY}_WORKSPACE_CONTEXT } from './{entity}-workspace.context-token.js';
```

## Step 3: Create workspace context

File: `{entity}-workspace.context.ts`

### Variant A: Simple (no parent hierarchy, like webhook)

```typescript
import type { Umb{EntityName}DetailRepository } from '../repository/index.js';
import { UMB_{ENTITY}_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_{ENTITY}_ENTITY_TYPE, UMB_{ENTITY}_ROOT_ENTITY_TYPE, UMB_{ENTITY}_WORKSPACE_ALIAS } from '../../entity.js';
import type { Umb{EntityName}DetailModel } from '../types.js';
import { Umb{EntityName}WorkspaceEditorElement } from './{entity}-workspace-editor.element.js';
import {
	UmbEntityNamedDetailWorkspaceContextBase,
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceIsNewRedirectControllerAlias,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export class Umb{EntityName}WorkspaceContext
	extends UmbEntityNamedDetailWorkspaceContextBase<Umb{EntityName}DetailModel, Umb{EntityName}DetailRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	// Observable properties — add one per entity property
	// readonly myProp = this._data.createObservablePartOfCurrent((data) => data?.myProp);

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_{ENTITY}_WORKSPACE_ALIAS,
			entityType: UMB_{ENTITY}_ENTITY_TYPE,
			detailRepositoryAlias: UMB_{ENTITY}_DETAIL_REPOSITORY_ALIAS,
		});

		this.routes.setRoutes([
			{
				path: 'create',
				component: Umb{EntityName}WorkspaceEditorElement,
				setup: async () => {
					await this.createScaffold({ parent: { entityType: UMB_{ENTITY}_ROOT_ENTITY_TYPE, unique: null } });

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:unique',
				component: Umb{EntityName}WorkspaceEditorElement,
				setup: (_component, info) => {
					this.removeUmbControllerByAlias(UmbWorkspaceIsNewRedirectControllerAlias);
					this.load(info.match.params.unique);
				},
			},
		]);
	}

	// Getter/setter methods — add one pair per entity property
	// getMyProp(): string | undefined {
	// 	return this._data.getCurrent()?.myProp;
	// }
	// setMyProp(value: string) {
	// 	this._data.updateCurrent({ myProp: value });
	// }
}

export { Umb{EntityName}WorkspaceContext as api };
```

### Variant B: Parent-aware (tree entities, like data-type)

Replace the `create` route with:

```typescript
{
	path: 'create/parent/:entityType/:parentUnique',
	component: Umb{EntityName}WorkspaceEditorElement,
	setup: async (_component, info) => {
		const parentEntityType = info.match.params.entityType;
		const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
		await this.createScaffold({ parent: { entityType: parentEntityType, unique: parentUnique } });

		new UmbWorkspaceIsNewRedirectController(
			this,
			this,
			this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
		);
	},
},
```

## Step 4: Create editor element

File: `{entity}-workspace-editor.element.ts`

Uses `<umb-workspace-header-name-editable>` — a built-in element that auto-consumes the workspace context for name binding, validation, write guard permissions, and auto-focus. No manual name wiring needed.

```typescript
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

// Back path: find or create a constant like UMB_{ENTITY}_SECTION_PATH in the entity's section-path constants.
// Grep for 'SECTION_PATH' or 'WORKSPACE_PATH' in the entity's package to find the correct import.
const BACK_PATH = 'FIXME: replace with section path constant';

@customElement('umb-{entity}-workspace-editor')
export class Umb{EntityName}WorkspaceEditorElement extends UmbLitElement {
	override render() {
		return html`
			<umb-entity-detail-workspace-editor back-path=${BACK_PATH}>
				<umb-workspace-header-name-editable slot="header"></umb-workspace-header-name-editable>
			</umb-entity-detail-workspace-editor>
		`;
	}

	static override styles = [
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];
}

export { Umb{EntityName}WorkspaceEditorElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-{entity}-workspace-editor': Umb{EntityName}WorkspaceEditorElement;
	}
}
```

## Step 5: Create detail view element

File: `views/{entity}-details-workspace-view.element.ts`

```typescript
import { UMB_{ENTITY}_WORKSPACE_CONTEXT } from '../{entity}-workspace.context-token.js';
import type { Umb{EntityName}DetailModel } from '../../types.js';
import { css, customElement, html, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-{entity}-details-workspace-view')
export class Umb{EntityName}DetailsWorkspaceViewElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@state()
	private _data?: Umb{EntityName}DetailModel;

	#workspaceContext?: typeof UMB_{ENTITY}_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_{ENTITY}_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this.observe(this.#workspaceContext?.data, (data) => {
				this._data = data;
			});
		});
	}

	override render() {
		if (!this._data) return nothing;

		return html`
			<uui-box>
				<!-- Add umb-property-layout elements for each property -->
				<!-- Example:
				<umb-property-layout label="My Property">
					<uui-input
						slot="editor"
						.value=${this._data.myProp ?? ''}
						@input=${(e: InputEvent) => this.#workspaceContext?.setMyProp((e.target as HTMLInputElement).value)}>
					</uui-input>
				</umb-property-layout>
				-->
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-space-6);
			}

			uui-input {
				width: 100%;
			}

			umb-property-layout:first-child {
				padding-top: 0;
			}
			umb-property-layout:last-child {
				padding-bottom: 0;
			}
		`,
	];
}

export { Umb{EntityName}DetailsWorkspaceViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-{entity}-details-workspace-view': Umb{EntityName}DetailsWorkspaceViewElement;
	}
}
```

## Step 6: Create manifests

File: `manifests.ts`

```typescript
import { UMB_{ENTITY}_ENTITY_TYPE, UMB_{ENTITY}_WORKSPACE_ALIAS } from '../../entity.js';
import { UMB_WORKSPACE_CONDITION_ALIAS, UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_{ENTITY}_WORKSPACE_ALIAS,
		name: '{EntityName} Workspace',
		api: () => import('./{entity}-workspace.context.js'),
		meta: {
			entityType: UMB_{ENTITY}_ENTITY_TYPE,
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.{EntityName}.Details',
		name: '{EntityName} Workspace Details View',
		element: () => import('./views/{entity}-details-workspace-view.element.js'),
		weight: 90,
		meta: {
			label: '#general_details',
			pathname: 'details',
			icon: 'edit',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_{ENTITY}_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.{EntityName}.Save',
		name: 'Save {EntityName} Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			label: '#buttons_save',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_{ENTITY}_WORKSPACE_ALIAS,
			},
		],
	},
];
```

## Step 7: Wire manifests into parent module

Import and spread the workspace manifests in the parent module's `manifests.ts`:

```typescript
import { manifests as workspaceManifests } from './workspace/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...workspaceManifests,
	// ... other manifests
];
```

## Routable workspace checklist

- [ ] Context token file with correct discriminator (`getEntityType()` check)
- [ ] `constants.ts` re-exports context token
- [ ] Workspace context extends `UmbEntityNamedDetailWorkspaceContextBase`
- [ ] Workspace context exported as `api` (last line)
- [ ] Routes set up with `create` and `edit/:unique` paths
- [ ] `UmbWorkspaceIsNewRedirectController` used in create route
- [ ] Editor element uses `<umb-entity-detail-workspace-editor>` with `<umb-workspace-header-name-editable slot="header">`
- [ ] Editor element exports as `element` and declared on `HTMLElementTagNameMap`
- [ ] View element implements `UmbWorkspaceViewElement`
- [ ] View element exports as `element` and declared on `HTMLElementTagNameMap`
- [ ] Manifests include `workspace`, `workspaceView`, and `workspaceAction`
- [ ] All view/action manifests use `UMB_WORKSPACE_CONDITION_ALIAS` condition
- [ ] Workspace manifests wired into parent module's `manifests.ts`
- [ ] Back path constant set correctly in editor element
