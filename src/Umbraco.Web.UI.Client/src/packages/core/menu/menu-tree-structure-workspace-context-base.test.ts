import { UmbMenuTreeStructureWorkspaceContextBase } from './menu-tree-structure-workspace-context-base.js';
import {
	UmbTestMenuStructureControllerHostElement,
	UmbTestSectionSidebarMenuContext,
	UmbTestSubmittableTreeEntityWorkspaceContext,
	UmbTestTreeRepository,
	createTestAncestorItem,
	createTestTreeRepositoryManifest,
} from './menu-tree-structure-workspace-context.test-utils.js';
import { UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT } from './section-sidebar-menu/index.js';
import { UMB_ANCESTORS_ENTITY_CONTEXT, UMB_PARENT_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { aTimeout, expect } from '@open-wc/testing';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_SUBMITTABLE_TREE_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';

const TEST_TREE_REPOSITORY_ALIAS = 'Umb.Test.MenuTreeStructureWorkspaceContextBase.TreeRepository';

class TestMenuTreeStructureWorkspaceContext extends UmbMenuTreeStructureWorkspaceContextBase {
	constructor(host: UmbControllerHost) {
		super(host, { treeRepositoryAlias: TEST_TREE_REPOSITORY_ALIAS });
	}
}

describe('UmbMenuTreeStructureWorkspaceContextBase', () => {
	let host: UmbTestMenuStructureControllerHostElement;
	let actionEventContext: UmbActionEventContext;
	let workspaceContext: UmbTestSubmittableTreeEntityWorkspaceContext;
	let context: TestMenuTreeStructureWorkspaceContext;

	before(() => {
		umbExtensionsRegistry.register(createTestTreeRepositoryManifest(TEST_TREE_REPOSITORY_ALIAS));
	});

	after(() => {
		umbExtensionsRegistry.unregister(TEST_TREE_REPOSITORY_ALIAS);
	});

	beforeEach(async () => {
		UmbTestTreeRepository.reset();
		UmbTestSectionSidebarMenuContext.reset();

		host = new UmbTestMenuStructureControllerHostElement();
		document.body.appendChild(host);

		actionEventContext = new UmbActionEventContext(host);
		workspaceContext = new UmbTestSubmittableTreeEntityWorkspaceContext(host);
		new UmbContextProviderController(host, UMB_SUBMITTABLE_TREE_ENTITY_WORKSPACE_CONTEXT, workspaceContext as never);
		new UmbContextProviderController(
			host,
			UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT,
			new UmbTestSectionSidebarMenuContext(host) as never,
		);

		context = new TestMenuTreeStructureWorkspaceContext(host);
		context.manifest = {
			type: 'workspaceContext',
			kind: 'menuStructure',
			alias: 'Umb.Test.MenuStructureWorkspaceContext',
			name: 'Test Menu Structure Workspace Context',
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
		expect(UmbTestTreeRepository.requestTreeItemAncestorsCalls).to.deep.equal([
			{ unique: 'test-unique', entityType: 'test-entity-type' },
		]);
	});

	it('sets UMB_PARENT_ENTITY_CONTEXT from the resolved ancestors', async () => {
		UmbTestTreeRepository.ancestors = [
			createTestAncestorItem({ unique: 'parent-unique', entityType: 'test-entity-type' }),
		];

		dispatchReloadStructure();
		await aTimeout(150);

		const parentContext = await context.getContext(UMB_PARENT_ENTITY_CONTEXT);
		expect(parentContext?.getParent()).to.deep.equal({ unique: 'parent-unique', entityType: 'test-entity-type' });
	});

	describe('navigating to a different entity', () => {
		it('clears the parent and ancestor state immediately, before the new fetch resolves (avoids a stale breadcrumb)', async () => {
			UmbTestTreeRepository.ancestors = [
				createTestAncestorItem({ unique: 'parent-unique', entityType: 'test-entity-type' }),
			];

			dispatchReloadStructure();
			await aTimeout(150);

			const parentContext = await context.getContext(UMB_PARENT_ENTITY_CONTEXT);
			const ancestorContext = await context.getContext(UMB_ANCESTORS_ENTITY_CONTEXT);
			expect(parentContext?.getParent()).to.deep.equal({ unique: 'parent-unique', entityType: 'test-entity-type' });
			expect(ancestorContext?.getAncestors()).to.deep.equal([
				{ unique: 'parent-unique', entityType: 'test-entity-type' },
			]);

			const callCountBeforeNavigate = UmbTestTreeRepository.requestTreeItemAncestorsCalls.length;

			// Navigate to a different entity. The debounce means the fetch for it hasn't even started yet.
			workspaceContext.setUnique('other-unique');

			expect(UmbTestTreeRepository.requestTreeItemAncestorsCalls).to.have.lengthOf(callCountBeforeNavigate);
			expect(parentContext?.getParent()).to.equal(undefined);
			expect(ancestorContext?.getAncestors()).to.deep.equal([]);
		});
	});

	describe('reload on UmbRequestReloadStructureForEntityEvent', () => {
		it('re-requests ancestors when the event matches the open entity (fixes a stale parent after a move)', async () => {
			// The item was moved to a new parent elsewhere (e.g. via "Move to"), without this workspace reloading.
			UmbTestTreeRepository.ancestors = [
				createTestAncestorItem({ unique: 'new-parent-unique', entityType: 'test-entity-type' }),
			];

			dispatchReloadStructure();
			await aTimeout(150);

			expect(UmbTestTreeRepository.requestTreeItemAncestorsCalls).to.have.lengthOf(2);

			const parentContext = await context.getContext(UMB_PARENT_ENTITY_CONTEXT);
			expect(parentContext?.getParent()).to.deep.equal({
				unique: 'new-parent-unique',
				entityType: 'test-entity-type',
			});
		});

		it('re-requests ancestors when the event matches a currently displayed ancestor (e.g. an ancestor was renamed elsewhere)', async () => {
			UmbTestTreeRepository.ancestors = [
				createTestAncestorItem({ unique: 'parent-unique', entityType: 'test-entity-type' }),
			];

			dispatchReloadStructure();
			await aTimeout(150);

			expect(UmbTestTreeRepository.requestTreeItemAncestorsCalls).to.have.lengthOf(2);

			// The event is for the ancestor, not the open entity itself.
			dispatchReloadStructure({ unique: 'parent-unique' });
			await aTimeout(150);

			expect(UmbTestTreeRepository.requestTreeItemAncestorsCalls).to.have.lengthOf(3);
		});

		it('does not re-request ancestors when the event is for an entity that is neither the open entity nor a currently displayed ancestor', async () => {
			UmbTestTreeRepository.ancestors = [
				createTestAncestorItem({ unique: 'parent-unique', entityType: 'test-entity-type' }),
			];

			dispatchReloadStructure();
			await aTimeout(150);

			expect(UmbTestTreeRepository.requestTreeItemAncestorsCalls).to.have.lengthOf(2);

			dispatchReloadStructure({ unique: 'unrelated-unique' });
			await aTimeout(150);

			expect(UmbTestTreeRepository.requestTreeItemAncestorsCalls).to.have.lengthOf(2);
		});

		it('does not re-request ancestors when the event is for a different unique', async () => {
			dispatchReloadStructure({ unique: 'some-other-unique' });
			await aTimeout(150);

			expect(UmbTestTreeRepository.requestTreeItemAncestorsCalls).to.have.lengthOf(1);
		});

		it('does not re-request ancestors when the event is for a different entity type', async () => {
			dispatchReloadStructure({ entityType: 'some-other-entity-type' });
			await aTimeout(150);

			expect(UmbTestTreeRepository.requestTreeItemAncestorsCalls).to.have.lengthOf(1);
		});
	});

	describe('expanding the sidebar menu', () => {
		it('expands the resolved parent when opening an existing item', async () => {
			expect(UmbTestSectionSidebarMenuContext.expandItemsCalls).to.have.lengthOf(1);
		});
	});

	describe('destroy', () => {
		it('stops reacting to reload-structure events once destroyed', async () => {
			context.destroy();

			UmbTestTreeRepository.ancestors = [
				createTestAncestorItem({ unique: 'new-parent-unique', entityType: 'test-entity-type' }),
			];
			dispatchReloadStructure();
			await aTimeout(150);

			expect(UmbTestTreeRepository.requestTreeItemAncestorsCalls).to.have.lengthOf(1);
		});
	});
});

describe('UmbMenuTreeStructureWorkspaceContextBase (creating a new item)', () => {
	let host: UmbTestMenuStructureControllerHostElement;
	let workspaceContext: UmbTestSubmittableTreeEntityWorkspaceContext;
	let context: TestMenuTreeStructureWorkspaceContext;

	before(() => {
		umbExtensionsRegistry.register(createTestTreeRepositoryManifest(TEST_TREE_REPOSITORY_ALIAS));
	});

	after(() => {
		umbExtensionsRegistry.unregister(TEST_TREE_REPOSITORY_ALIAS);
	});

	beforeEach(async () => {
		UmbTestTreeRepository.reset();
		UmbTestSectionSidebarMenuContext.reset();

		host = new UmbTestMenuStructureControllerHostElement();
		document.body.appendChild(host);

		workspaceContext = new UmbTestSubmittableTreeEntityWorkspaceContext(host);
		new UmbContextProviderController(host, UMB_SUBMITTABLE_TREE_ENTITY_WORKSPACE_CONTEXT, workspaceContext as never);
		new UmbContextProviderController(
			host,
			UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT,
			new UmbTestSectionSidebarMenuContext(host) as never,
		);

		context = new TestMenuTreeStructureWorkspaceContext(host);
		context.manifest = {
			type: 'workspaceContext',
			kind: 'menuStructure',
			alias: 'Umb.Test.MenuStructureWorkspaceContext.Create',
			name: 'Test Menu Structure Workspace Context (create)',
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

describe('UmbMenuTreeStructureWorkspaceContextBase (creating a new item directly under the root)', () => {
	let host: UmbTestMenuStructureControllerHostElement;
	let workspaceContext: UmbTestSubmittableTreeEntityWorkspaceContext;
	let context: TestMenuTreeStructureWorkspaceContext;

	before(() => {
		umbExtensionsRegistry.register(createTestTreeRepositoryManifest(TEST_TREE_REPOSITORY_ALIAS));
	});

	after(() => {
		umbExtensionsRegistry.unregister(TEST_TREE_REPOSITORY_ALIAS);
	});

	beforeEach(async () => {
		UmbTestTreeRepository.reset();
		UmbTestSectionSidebarMenuContext.reset();

		host = new UmbTestMenuStructureControllerHostElement();
		document.body.appendChild(host);

		workspaceContext = new UmbTestSubmittableTreeEntityWorkspaceContext(host);
		new UmbContextProviderController(host, UMB_SUBMITTABLE_TREE_ENTITY_WORKSPACE_CONTEXT, workspaceContext as never);
		new UmbContextProviderController(
			host,
			UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT,
			new UmbTestSectionSidebarMenuContext(host) as never,
		);

		context = new TestMenuTreeStructureWorkspaceContext(host);
		context.manifest = {
			type: 'workspaceContext',
			kind: 'menuStructure',
			alias: 'Umb.Test.MenuStructureWorkspaceContext.CreateUnderRoot',
			name: 'Test Menu Structure Workspace Context (create under root)',
			meta: { menuItemAlias: 'test-menu-item' },
		};

		// Creating directly under the tree root (e.g. a Dictionary item created at the root): the parent IS the
		// root, so no ancestors call happens while still new (the root is already the full structure on its own).
		workspaceContext.setEntityType('test-entity-type');
		workspaceContext.setIsNew(true);
		workspaceContext.setCreateUnderParent({ unique: null, entityType: 'test-root-entity-type' });
		workspaceContext.setUnique('new-item-unique');
		await aTimeout(150);
	});

	afterEach(() => {
		context.destroy();
		document.body.removeChild(host);
	});

	it('re-fetches the structure once the item has been saved, so the root stays in the breadcrumb', async () => {
		// The real ancestors endpoint, once the item exists, returns the item itself as the trailing entry.
		UmbTestTreeRepository.ancestors = [createTestAncestorItem({ unique: 'new-item-unique', entityType: 'test-entity-type' })];

		workspaceContext.setIsNew(false);
		await aTimeout(150);

		expect(UmbTestTreeRepository.requestTreeItemAncestorsCalls).to.deep.equal([
			{ unique: 'new-item-unique', entityType: 'test-entity-type' },
		]);

		const structure = await firstValueFrom(context.structure);
		expect(structure.map((item) => item.unique)).to.deep.equal([null, 'new-item-unique']);
	});
});

describe('UmbMenuTreeStructureWorkspaceContextBase (isNew resolves after the structure has already loaded)', () => {
	let host: UmbTestMenuStructureControllerHostElement;
	let workspaceContext: UmbTestSubmittableTreeEntityWorkspaceContext;
	let context: TestMenuTreeStructureWorkspaceContext;

	before(() => {
		umbExtensionsRegistry.register(createTestTreeRepositoryManifest(TEST_TREE_REPOSITORY_ALIAS));
	});

	after(() => {
		umbExtensionsRegistry.unregister(TEST_TREE_REPOSITORY_ALIAS);
	});

	beforeEach(async () => {
		UmbTestTreeRepository.reset();
		UmbTestSectionSidebarMenuContext.reset();

		host = new UmbTestMenuStructureControllerHostElement();
		document.body.appendChild(host);

		workspaceContext = new UmbTestSubmittableTreeEntityWorkspaceContext(host);
		new UmbContextProviderController(host, UMB_SUBMITTABLE_TREE_ENTITY_WORKSPACE_CONTEXT, workspaceContext as never);
		new UmbContextProviderController(
			host,
			UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT,
			new UmbTestSectionSidebarMenuContext(host) as never,
		);

		context = new TestMenuTreeStructureWorkspaceContext(host);
		context.manifest = {
			type: 'workspaceContext',
			kind: 'menuStructure',
			alias: 'Umb.Test.MenuStructureWorkspaceContext.Race',
			name: 'Test Menu Structure Workspace Context (isNew resolves late)',
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
		expect(UmbTestTreeRepository.requestTreeItemAncestorsCalls).to.have.lengthOf(1);
		expect(UmbTestSectionSidebarMenuContext.expandItemsCalls).to.have.lengthOf(0);
	});

	it('expands once isNew resolves to false, without re-fetching the structure', async () => {
		const requestCountBeforeIsNewResolves = UmbTestTreeRepository.requestTreeItemAncestorsCalls.length;

		workspaceContext.setIsNew(false);
		await aTimeout(150);

		expect(UmbTestSectionSidebarMenuContext.expandItemsCalls).to.have.lengthOf(1);
		expect(UmbTestTreeRepository.requestTreeItemAncestorsCalls).to.have.lengthOf(requestCountBeforeIsNewResolves);
	});
});
