import { UmbMenuTreeStructureWorkspaceContextBase } from './menu-tree-structure-workspace-context-base.js';
import {
	UmbTestMenuStructureControllerHostElement,
	UmbTestSubmittableTreeEntityWorkspaceContext,
	UmbTestTreeRepository,
	createTestAncestorItem,
	createTestTreeRepositoryManifest,
} from './menu-tree-structure-workspace-context.test-utils.js';
import { UMB_ANCESTORS_ENTITY_CONTEXT, UMB_PARENT_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { aTimeout, expect } from '@open-wc/testing';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_SUBMITTABLE_TREE_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

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

		host = new UmbTestMenuStructureControllerHostElement();
		document.body.appendChild(host);

		actionEventContext = new UmbActionEventContext(host);
		workspaceContext = new UmbTestSubmittableTreeEntityWorkspaceContext(host);
		new UmbContextProviderController(host, UMB_SUBMITTABLE_TREE_ENTITY_WORKSPACE_CONTEXT, workspaceContext as never);

		context = new TestMenuTreeStructureWorkspaceContext(host);

		workspaceContext.setEntityType('test-entity-type');
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
