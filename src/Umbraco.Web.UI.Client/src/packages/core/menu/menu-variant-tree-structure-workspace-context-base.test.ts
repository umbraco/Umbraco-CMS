import { UmbMenuVariantTreeStructureWorkspaceContextBase } from './menu-variant-tree-structure-workspace-context-base.js';
import {
	UmbTestMenuVariantStructureControllerHostElement,
	UmbTestSectionContext,
	UmbTestSectionSidebarMenuContext,
	UmbTestSubmittableTreeEntityWorkspaceContext,
	UmbTestVariantTreeRepository,
	createTestVariantAncestorItem,
	createTestVariantTreeRepositoryManifest,
} from './menu-variant-tree-structure-workspace-context.test-utils.js';
import { UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT } from './section-sidebar-menu/index.js';
import type { UmbVariantStructureItemModel } from './types.js';
import { UMB_ANCESTORS_ENTITY_CONTEXT, UMB_PARENT_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { aTimeout, expect } from '@open-wc/testing';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import {
	UMB_SUBMITTABLE_TREE_ENTITY_WORKSPACE_CONTEXT,
	UMB_WORKSPACE_EDIT_PATH_PATTERN,
	UMB_WORKSPACE_PATH_PATTERN,
} from '@umbraco-cms/backoffice/workspace';

const TEST_TREE_REPOSITORY_ALIAS = 'Umb.Test.MenuVariantTreeStructureWorkspaceContextBase.TreeRepository';

class TestMenuVariantTreeStructureWorkspaceContext extends UmbMenuVariantTreeStructureWorkspaceContextBase {
	constructor(host: UmbControllerHost) {
		super(host, { treeRepositoryAlias: TEST_TREE_REPOSITORY_ALIAS });
	}
}

describe('UmbMenuVariantTreeStructureWorkspaceContextBase', () => {
	let host: UmbTestMenuVariantStructureControllerHostElement;
	let actionEventContext: UmbActionEventContext;
	let workspaceContext: UmbTestSubmittableTreeEntityWorkspaceContext;
	let context: TestMenuVariantTreeStructureWorkspaceContext;

	before(() => {
		umbExtensionsRegistry.register(createTestVariantTreeRepositoryManifest(TEST_TREE_REPOSITORY_ALIAS));
	});

	after(() => {
		umbExtensionsRegistry.unregister(TEST_TREE_REPOSITORY_ALIAS);
	});

	beforeEach(async () => {
		UmbTestVariantTreeRepository.reset();
		UmbTestSectionSidebarMenuContext.reset();

		host = new UmbTestMenuVariantStructureControllerHostElement();
		document.body.appendChild(host);

		actionEventContext = new UmbActionEventContext(host);
		workspaceContext = new UmbTestSubmittableTreeEntityWorkspaceContext(host);
		new UmbContextProviderController(host, UMB_SUBMITTABLE_TREE_ENTITY_WORKSPACE_CONTEXT, workspaceContext as never);
		new UmbContextProviderController(
			host,
			UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT,
			new UmbTestSectionSidebarMenuContext(host) as never,
		);
		new UmbTestSectionContext(host);

		context = new TestMenuVariantTreeStructureWorkspaceContext(host);
		context.manifest = {
			type: 'workspaceContext',
			kind: 'menuStructure',
			alias: 'Umb.Test.MenuVariantStructureWorkspaceContext',
			name: 'Test Menu Variant Structure Workspace Context',
			meta: { menuItemAlias: 'test-menu-item' },
		};

		workspaceContext.setEntityType('test-entity-type');
		workspaceContext.setIsNew(false);
		workspaceContext.setUnique('test-unique');
		await aTimeout(150);
	});

	afterEach(() => {
		// Cancels any pending debounced fetch so it doesn't leak into a later test.
		context.destroy();
		document.body.removeChild(host);
	});

	function dispatchReloadStructure(overrides?: { unique?: string | null; entityType?: string }) {
		actionEventContext.dispatchEvent(
			new UmbRequestReloadStructureForEntityEvent({
				unique: 'test-unique',
				entityType: 'test-entity-type',
				...overrides,
			}),
		);
	}

	it('requests ancestors for the open entity on load', async () => {
		expect(UmbTestVariantTreeRepository.requestTreeItemAncestorsCalls).to.deep.equal([
			{ unique: 'test-unique', entityType: 'test-entity-type' },
		]);
	});

	it('sets UMB_PARENT_ENTITY_CONTEXT from the resolved ancestors', async () => {
		UmbTestVariantTreeRepository.ancestors = [
			createTestVariantAncestorItem({ unique: 'parent-unique', entityType: 'test-entity-type' }),
		];

		dispatchReloadStructure();
		await aTimeout(150);

		const parentContext = await context.getContext(UMB_PARENT_ENTITY_CONTEXT);
		expect(parentContext?.getParent()).to.deep.equal({ unique: 'parent-unique', entityType: 'test-entity-type' });
	});

	it('propagates name and isFolder onto each ancestor in the structure', async () => {
		UmbTestVariantTreeRepository.ancestors = [
			createTestVariantAncestorItem({ unique: 'folder-unique', entityType: 'test-folder-entity-type' }, 'My Folder', true),
			createTestVariantAncestorItem({ unique: 'parent-unique', entityType: 'test-entity-type' }, 'My Item'),
		];

		dispatchReloadStructure();
		await aTimeout(150);

		const structure = await firstValueFrom(context.structure);
		expect(structure.find((item) => item.unique === 'folder-unique')).to.deep.include({
			name: 'My Folder',
			isFolder: true,
		});
		expect(structure.find((item) => item.unique === 'parent-unique')).to.deep.include({
			name: 'My Item',
			isFolder: false,
		});
	});

	describe('navigating to a different entity', () => {
		it('clears the parent and ancestor state immediately, before the new fetch resolves (avoids a stale breadcrumb)', async () => {
			UmbTestVariantTreeRepository.ancestors = [
				createTestVariantAncestorItem({ unique: 'parent-unique', entityType: 'test-entity-type' }),
			];

			dispatchReloadStructure();
			await aTimeout(150);

			const parentContext = await context.getContext(UMB_PARENT_ENTITY_CONTEXT);
			const ancestorContext = await context.getContext(UMB_ANCESTORS_ENTITY_CONTEXT);
			expect(parentContext?.getParent()).to.deep.equal({ unique: 'parent-unique', entityType: 'test-entity-type' });
			expect(ancestorContext?.getAncestors()).to.deep.equal([
				{ unique: 'parent-unique', entityType: 'test-entity-type' },
			]);

			const callCountBeforeNavigate = UmbTestVariantTreeRepository.requestTreeItemAncestorsCalls.length;

			// Navigate to a different entity. The debounce means the fetch for it hasn't even started yet.
			workspaceContext.setUnique('other-unique');

			expect(UmbTestVariantTreeRepository.requestTreeItemAncestorsCalls).to.have.lengthOf(callCountBeforeNavigate);
			expect(parentContext?.getParent()).to.equal(undefined);
			expect(ancestorContext?.getAncestors()).to.deep.equal([]);
		});

		it('does not expand the outgoing entity when isNew re-settles before `unique` updates to the new entity', async () => {
			// Give the current entity an ancestor chain ending with itself, matching the real tree repository's shape.
			UmbTestVariantTreeRepository.ancestors = [
				createTestVariantAncestorItem({ unique: 'parent-unique', entityType: 'test-entity-type' }),
				createTestVariantAncestorItem({ unique: 'test-unique', entityType: 'test-entity-type' }),
			];
			dispatchReloadStructure();
			await aTimeout(150);

			UmbTestSectionSidebarMenuContext.reset();

			// Navigate to a sibling entity, replaying the exact `load()` ordering the real workspace context uses:
			// isNew resets and re-settles to false (with getUnique() transiently undefined) before unique updates.
			workspaceContext.loadDifferentEntity('other-unique');

			const wasOutgoingEntityExpanded = UmbTestSectionSidebarMenuContext.expandItemsCalls.some((call) =>
				(call as Array<{ unique: string }>).some((entry) => entry.unique === 'test-unique'),
			);
			expect(wasOutgoingEntityExpanded).to.equal(false);
		});
	});

	describe('reload on UmbRequestReloadStructureForEntityEvent', () => {
		it('re-requests ancestors when the event matches the open entity (fixes a stale parent after a move)', async () => {
			// The item was moved to a new parent elsewhere (e.g. via "Move to"), without this workspace reloading.
			UmbTestVariantTreeRepository.ancestors = [
				createTestVariantAncestorItem({ unique: 'new-parent-unique', entityType: 'test-entity-type' }),
			];

			dispatchReloadStructure();
			await aTimeout(150);

			expect(UmbTestVariantTreeRepository.requestTreeItemAncestorsCalls).to.have.lengthOf(2);

			const parentContext = await context.getContext(UMB_PARENT_ENTITY_CONTEXT);
			expect(parentContext?.getParent()).to.deep.equal({
				unique: 'new-parent-unique',
				entityType: 'test-entity-type',
			});
		});

		it('re-requests ancestors when the event matches a currently displayed ancestor (e.g. an ancestor was renamed elsewhere)', async () => {
			UmbTestVariantTreeRepository.ancestors = [
				createTestVariantAncestorItem({ unique: 'parent-unique', entityType: 'test-entity-type' }),
			];

			dispatchReloadStructure();
			await aTimeout(150);

			expect(UmbTestVariantTreeRepository.requestTreeItemAncestorsCalls).to.have.lengthOf(2);

			// The event is for the ancestor, not the open entity itself.
			dispatchReloadStructure({ unique: 'parent-unique' });
			await aTimeout(150);

			expect(UmbTestVariantTreeRepository.requestTreeItemAncestorsCalls).to.have.lengthOf(3);
		});

		it('does not re-request ancestors when the event is for an entity that is neither the open entity nor a currently displayed ancestor', async () => {
			UmbTestVariantTreeRepository.ancestors = [
				createTestVariantAncestorItem({ unique: 'parent-unique', entityType: 'test-entity-type' }),
			];

			dispatchReloadStructure();
			await aTimeout(150);

			expect(UmbTestVariantTreeRepository.requestTreeItemAncestorsCalls).to.have.lengthOf(2);

			dispatchReloadStructure({ unique: 'unrelated-unique' });
			await aTimeout(150);

			expect(UmbTestVariantTreeRepository.requestTreeItemAncestorsCalls).to.have.lengthOf(2);
		});

		it('does not re-request ancestors when the event is for a different unique', async () => {
			dispatchReloadStructure({ unique: 'some-other-unique' });
			await aTimeout(150);

			expect(UmbTestVariantTreeRepository.requestTreeItemAncestorsCalls).to.have.lengthOf(1);
		});

		it('does not re-request ancestors when the event is for a different entity type', async () => {
			dispatchReloadStructure({ entityType: 'some-other-entity-type' });
			await aTimeout(150);

			expect(UmbTestVariantTreeRepository.requestTreeItemAncestorsCalls).to.have.lengthOf(1);
		});
	});

	describe('expanding the sidebar menu', () => {
		it('expands the resolved parent when opening an existing item', async () => {
			expect(UmbTestSectionSidebarMenuContext.expandItemsCalls).to.have.lengthOf(1);
		});
	});

	describe('getItemHref', () => {
		const TEST_WORKSPACE_ALIAS = 'Umb.Test.MenuVariantTreeStructureWorkspaceContextBase.Workspace';

		function structureItem(overrides: Partial<UmbVariantStructureItemModel>): UmbVariantStructureItemModel {
			return {
				unique: 'item-unique',
				entityType: 'test-entity-type',
				variants: [{ name: 'Item', culture: null, segment: null }],
				...overrides,
			};
		}

		function registerWorkspaceFor(entityType: string) {
			umbExtensionsRegistry.register({
				type: 'workspace',
				alias: TEST_WORKSPACE_ALIAS,
				name: 'Test Workspace',
				meta: { entityType },
			});
		}

		afterEach(() => {
			umbExtensionsRegistry.unregister(TEST_WORKSPACE_ALIAS);
		});

		it('returns undefined for an item whose entity type has no registered workspace', () => {
			expect(context.getItemHref(structureItem({}))).to.equal(undefined);
		});

		it('returns an edit-path link for an item whose entity type has a registered workspace', () => {
			registerWorkspaceFor('test-entity-type');

			expect(context.getItemHref(structureItem({}))).to.equal(
				UMB_WORKSPACE_EDIT_PATH_PATTERN.generateAbsolute({
					sectionName: UmbTestSectionContext.PATHNAME,
					entityType: 'test-entity-type',
					unique: 'item-unique',
				}),
			);
		});

		it('returns undefined for a root item (no unique) whose entity type has no registered workspace', () => {
			expect(context.getItemHref(structureItem({ unique: null, entityType: 'test-root-entity-type' }))).to.equal(
				undefined,
			);
		});

		it('returns a root-path link (no unique segment) for a root item whose entity type has a registered workspace', () => {
			registerWorkspaceFor('test-root-entity-type');

			expect(context.getItemHref(structureItem({ unique: null, entityType: 'test-root-entity-type' }))).to.equal(
				UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
					sectionName: UmbTestSectionContext.PATHNAME,
					entityType: 'test-root-entity-type',
				}),
			);
		});
	});

	describe('destroy', () => {
		it('stops reacting to reload-structure events once destroyed', async () => {
			context.destroy();

			UmbTestVariantTreeRepository.ancestors = [
				createTestVariantAncestorItem({ unique: 'new-parent-unique', entityType: 'test-entity-type' }),
			];
			dispatchReloadStructure();
			await aTimeout(150);

			expect(UmbTestVariantTreeRepository.requestTreeItemAncestorsCalls).to.have.lengthOf(1);
		});
	});
});

describe('UmbMenuVariantTreeStructureWorkspaceContextBase (creating a new item)', () => {
	let host: UmbTestMenuVariantStructureControllerHostElement;
	let workspaceContext: UmbTestSubmittableTreeEntityWorkspaceContext;
	let context: TestMenuVariantTreeStructureWorkspaceContext;

	before(() => {
		umbExtensionsRegistry.register(createTestVariantTreeRepositoryManifest(TEST_TREE_REPOSITORY_ALIAS));
	});

	after(() => {
		umbExtensionsRegistry.unregister(TEST_TREE_REPOSITORY_ALIAS);
	});

	beforeEach(async () => {
		UmbTestVariantTreeRepository.reset();
		UmbTestSectionSidebarMenuContext.reset();

		host = new UmbTestMenuVariantStructureControllerHostElement();
		document.body.appendChild(host);

		workspaceContext = new UmbTestSubmittableTreeEntityWorkspaceContext(host);
		new UmbContextProviderController(host, UMB_SUBMITTABLE_TREE_ENTITY_WORKSPACE_CONTEXT, workspaceContext as never);
		new UmbContextProviderController(
			host,
			UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT,
			new UmbTestSectionSidebarMenuContext(host) as never,
		);

		context = new TestMenuVariantTreeStructureWorkspaceContext(host);
		context.manifest = {
			type: 'workspaceContext',
			kind: 'menuStructure',
			alias: 'Umb.Test.MenuVariantStructureWorkspaceContext.Create',
			name: 'Test Menu Variant Structure Workspace Context (create)',
			meta: { menuItemAlias: 'test-menu-item' },
		};

		// Simulate opening a "Create X under Y" workspace before any save: isNew is already true,
		// and the item already has a client-generated unique.
		workspaceContext.setEntityType('test-entity-type');
		workspaceContext.setIsNew(true);
		workspaceContext.setCreateUnderParent({ unique: 'parent-unique', entityType: 'test-entity-type' });
		workspaceContext.setUnique('new-item-unique');
		await aTimeout(150);
	});

	afterEach(() => {
		context.destroy();
		document.body.removeChild(host);
	});

	it('does not expand the parent while the workspace is still in create mode', async () => {
		expect(UmbTestSectionSidebarMenuContext.expandItemsCalls).to.have.lengthOf(0);
	});

	it('expands the parent once the item has been saved', async () => {
		workspaceContext.setIsNew(false);
		await aTimeout(150);

		expect(UmbTestSectionSidebarMenuContext.expandItemsCalls).to.have.lengthOf(1);
	});
});

describe('UmbMenuVariantTreeStructureWorkspaceContextBase (isNew resolves after the structure has already loaded)', () => {
	let host: UmbTestMenuVariantStructureControllerHostElement;
	let workspaceContext: UmbTestSubmittableTreeEntityWorkspaceContext;
	let context: TestMenuVariantTreeStructureWorkspaceContext;

	before(() => {
		umbExtensionsRegistry.register(createTestVariantTreeRepositoryManifest(TEST_TREE_REPOSITORY_ALIAS));
	});

	after(() => {
		umbExtensionsRegistry.unregister(TEST_TREE_REPOSITORY_ALIAS);
	});

	beforeEach(async () => {
		UmbTestVariantTreeRepository.reset();
		UmbTestSectionSidebarMenuContext.reset();

		host = new UmbTestMenuVariantStructureControllerHostElement();
		document.body.appendChild(host);

		workspaceContext = new UmbTestSubmittableTreeEntityWorkspaceContext(host);
		new UmbContextProviderController(host, UMB_SUBMITTABLE_TREE_ENTITY_WORKSPACE_CONTEXT, workspaceContext as never);
		new UmbContextProviderController(
			host,
			UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT,
			new UmbTestSectionSidebarMenuContext(host) as never,
		);

		context = new TestMenuVariantTreeStructureWorkspaceContext(host);
		context.manifest = {
			type: 'workspaceContext',
			kind: 'menuStructure',
			alias: 'Umb.Test.MenuVariantStructureWorkspaceContext.Race',
			name: 'Test Menu Variant Structure Workspace Context (isNew resolves late)',
			meta: { menuItemAlias: 'test-menu-item' },
		};

		// A workspace context that publishes `unique` before `isNew` has resolved (e.g. a slow detail request) -
		// the structure fetch runs and completes while `isNew` is still undefined.
		workspaceContext.setEntityType('test-entity-type');
		workspaceContext.setUnique('test-unique');
		await aTimeout(150);
	});

	afterEach(() => {
		context.destroy();
		document.body.removeChild(host);
	});

	it('fetches the structure but does not expand while isNew is still unresolved', async () => {
		expect(UmbTestVariantTreeRepository.requestTreeItemAncestorsCalls).to.have.lengthOf(1);
		expect(UmbTestSectionSidebarMenuContext.expandItemsCalls).to.have.lengthOf(0);
	});

	it('expands once isNew resolves to false, without re-fetching the structure', async () => {
		const requestCountBeforeIsNewResolves = UmbTestVariantTreeRepository.requestTreeItemAncestorsCalls.length;

		workspaceContext.setIsNew(false);
		await aTimeout(150);

		expect(UmbTestSectionSidebarMenuContext.expandItemsCalls).to.have.lengthOf(1);
		expect(UmbTestVariantTreeRepository.requestTreeItemAncestorsCalls).to.have.lengthOf(requestCountBeforeIsNewResolves);
	});
});
