import { UmbMenuVariantTreeStructureWorkspaceContextBase } from './menu-variant-tree-structure-workspace-context-base.js';
import {
	UmbTestMenuVariantStructureControllerHostElement,
	UmbTestSubmittableTreeEntityWorkspaceContext,
	UmbTestVariantTreeRepository,
	createTestVariantAncestorItem,
	createTestVariantTreeRepositoryManifest,
} from './menu-variant-tree-structure-workspace-context.test-utils.js';
import { UMB_PARENT_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { aTimeout, expect } from '@open-wc/testing';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_SUBMITTABLE_TREE_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

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

		host = new UmbTestMenuVariantStructureControllerHostElement();
		document.body.appendChild(host);

		actionEventContext = new UmbActionEventContext(host);
		workspaceContext = new UmbTestSubmittableTreeEntityWorkspaceContext(host);
		new UmbContextProviderController(host, UMB_SUBMITTABLE_TREE_ENTITY_WORKSPACE_CONTEXT, workspaceContext as never);

		context = new TestMenuVariantTreeStructureWorkspaceContext(host);

		workspaceContext.setEntityType('test-entity-type');
		workspaceContext.setUnique('test-unique');
		await aTimeout(150);
	});

	afterEach(() => {
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
