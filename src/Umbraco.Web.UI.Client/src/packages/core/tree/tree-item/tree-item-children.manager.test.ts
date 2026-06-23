import { UmbTreeItemChildrenManager } from './tree-item-children.manager.js';
import { UMB_TREE_CONTEXT } from '../tree.context.token.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '../types.js';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

type RequestCall = { parentUnique: string | null };

class UmbTestTreeRepository {
	public itemsOfCalls: Array<RequestCall> = [];
	public rootCalls = 0;

	async requestTreeItemsOf(args: any) {
		this.itemsOfCalls.push({ parentUnique: args.parent.unique });
		return { data: { items: [], total: 0, totalBefore: 0, totalAfter: 0 } };
	}

	async requestTreeRootItems() {
		this.rootCalls++;
		return { data: { items: [], total: 0, totalBefore: 0, totalAfter: 0 } };
	}
}

class UmbTestTreeContext extends UmbContextBase {
	#repository: UmbTestTreeRepository;

	constructor(host: UmbControllerHost, repository: UmbTestTreeRepository) {
		super(host, UMB_TREE_CONTEXT as unknown as UmbContextToken<UmbTestTreeContext>);
		this.#repository = repository;
	}

	getRepository() {
		return this.#repository;
	}
}

@customElement('umb-test-tree-children-manager-host')
class UmbTestTreeChildrenManagerHostElement extends UmbElementMixin(HTMLElement) {}

const treeRoot: UmbTreeRootModel = {
	unique: null,
	entityType: 'test-root-entity-type',
	name: 'Root',
	hasChildren: true,
	isFolder: false,
};

const startNode = { unique: 'start-node-id', entityType: 'test-entity-type' };

describe('UmbTreeItemChildrenManager', () => {
	let host: UmbTestTreeChildrenManagerHostElement;
	let repository: UmbTestTreeRepository;
	let actionEventContext: UmbActionEventContext;
	let manager: UmbTreeItemChildrenManager<UmbTreeItemModel, UmbTreeRootModel>;

	beforeEach(async () => {
		host = new UmbTestTreeChildrenManagerHostElement();
		document.body.appendChild(host);

		repository = new UmbTestTreeRepository();
		new UmbTestTreeContext(host, repository);
		actionEventContext = new UmbActionEventContext(host);

		manager = new UmbTreeItemChildrenManager<UmbTreeItemModel, UmbTreeRootModel>(host);

		// Allow consumeContext (tree + action event context) to resolve and the
		// reload event listeners to be wired up before dispatching.
		await aTimeout(0);
	});

	afterEach(() => {
		document.body.removeChild(host);
	});

	describe('reload children events', () => {
		it('reloads the start node children when drilled into a start node', async () => {
			// The tree root is the tracked tree item, but children are loaded for the start node.
			manager.setTreeItem(treeRoot);
			manager.setStartNode(startNode);

			actionEventContext.dispatchEvent(
				new UmbRequestReloadChildrenOfEntityEvent({
					entityType: startNode.entityType,
					unique: startNode.unique,
				}),
			);

			await aTimeout(0);

			expect(repository.itemsOfCalls.length).to.equal(1);
			expect(repository.itemsOfCalls[0].parentUnique).to.equal(startNode.unique);
		});

		it('ignores reload events targeting an unrelated entity', async () => {
			manager.setTreeItem(treeRoot);
			manager.setStartNode(startNode);

			actionEventContext.dispatchEvent(
				new UmbRequestReloadChildrenOfEntityEvent({
					entityType: 'some-other-type',
					unique: 'some-other-unique',
				}),
			);

			await aTimeout(0);

			expect(repository.itemsOfCalls.length).to.equal(0);
			expect(repository.rootCalls).to.equal(0);
		});

		it('reloads the tree item children when no start node is set', async () => {
			const treeItem: UmbTreeItemModel = {
				unique: 'parent-folder-id',
				entityType: 'test-entity-type',
				name: 'Parent Folder',
				hasChildren: true,
				isFolder: true,
				parent: { unique: null, entityType: 'test-root-entity-type' },
			};
			manager.setTreeItem(treeItem);

			actionEventContext.dispatchEvent(
				new UmbRequestReloadChildrenOfEntityEvent({
					entityType: treeItem.entityType,
					unique: treeItem.unique,
				}),
			);

			await aTimeout(0);

			expect(repository.itemsOfCalls.length).to.equal(1);
			expect(repository.itemsOfCalls[0].parentUnique).to.equal(treeItem.unique);
		});
	});
});
