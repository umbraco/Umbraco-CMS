import { UmbTreeItemChildrenManager } from './tree-item-children.manager.js';
import { UMB_TREE_CONTEXT } from '../tree.context.token.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '../types.js';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

type RequestCall = { parentUnique: string | null; skip?: number; take?: number };

class UmbTestTreeRepository {
	public itemsOfCalls: Array<RequestCall> = [];
	public rootCalls = 0;
	public items: Array<UmbTreeItemModel> = [];
	public total?: number;

	async requestTreeItemsOf(args: any) {
		this.itemsOfCalls.push({ parentUnique: args.parent.unique, skip: args.skip, take: args.take });
		return {
			data: { items: this.items, total: this.total ?? this.items.length, totalBefore: 0, totalAfter: 0 },
		};
	}

	async requestTreeRootItems() {
		this.rootCalls++;
		return {
			data: { items: this.items, total: this.total ?? this.items.length, totalBefore: 0, totalAfter: 0 },
		};
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

	describe('reload structure events', () => {
		const childItem: UmbTreeItemModel = {
			unique: 'child-id',
			entityType: 'test-entity-type',
			name: 'Child',
			hasChildren: false,
			isFolder: false,
			parent: { unique: startNode.unique, entityType: startNode.entityType },
		};

		it('reloads children when a displayed child changes (e.g. is deleted) in a drilled start node', async () => {
			repository.items = [childItem];
			manager.setTreeItem(treeRoot);
			manager.setStartNode(startNode);

			await manager.loadChildren();
			expect(repository.itemsOfCalls.length).to.equal(1);

			actionEventContext.dispatchEvent(
				new UmbRequestReloadStructureForEntityEvent({
					entityType: childItem.entityType,
					unique: childItem.unique,
				}),
			);

			await aTimeout(0);

			expect(repository.itemsOfCalls.length).to.equal(2);
			expect(repository.itemsOfCalls[1].parentUnique).to.equal(startNode.unique);
		});

		it('ignores structure changes for an entity that is not a displayed child', async () => {
			repository.items = [childItem];
			manager.setTreeItem(treeRoot);
			manager.setStartNode(startNode);

			await manager.loadChildren();
			expect(repository.itemsOfCalls.length).to.equal(1);

			actionEventContext.dispatchEvent(
				new UmbRequestReloadStructureForEntityEvent({
					entityType: 'some-other-type',
					unique: 'some-other-unique',
				}),
			);

			await aTimeout(0);

			expect(repository.itemsOfCalls.length).to.equal(1);
		});
	});

	describe('load next children', () => {
		const parentFolder: UmbTreeItemModel = {
			unique: 'parent-folder-id',
			entityType: 'test-entity-type',
			name: 'Parent Folder',
			hasChildren: true,
			isFolder: true,
			parent: { unique: null, entityType: 'test-root-entity-type' },
		};

		const firstPage: Array<UmbTreeItemModel> = Array.from({ length: 3 }, (_, i) => ({
			unique: `child-${i}`,
			entityType: 'test-entity-type',
			name: `Child ${i}`,
			hasChildren: false,
			isFolder: false,
			parent: { unique: parentFolder.unique, entityType: parentFolder.entityType },
		}));

		it('requests the next slice using the loaded item count as skip on the first "load more"', async () => {
			// More items exist on the server than the first page, so "load more" is meaningful.
			repository.items = firstPage;
			repository.total = 10;
			manager.setTakeSize(firstPage.length);
			manager.setTreeItem(parentFolder);

			await manager.loadChildren();
			expect(repository.itemsOfCalls.length).to.equal(1);
			expect(repository.itemsOfCalls[0].skip).to.equal(0);

			await manager.loadNextChildren();

			// The page counter is only advanced after the request completes, so deriving skip from it
			// would re-request page 1 (skip 0). It must instead reflect the already-loaded children.
			expect(repository.itemsOfCalls.length).to.equal(2);
			expect(repository.itemsOfCalls[1].skip).to.equal(firstPage.length);
		});
	});
});
