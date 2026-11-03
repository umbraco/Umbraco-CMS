import { UmbTreeItemChildrenManager } from './tree-item-children.manager.js';
import { UMB_TREE_CONTEXT } from '../tree.context.token.js';
import { UmbTreeItemsNotSupportedError } from '../data/tree-items-not-supported.error.js';
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
	public itemsCalls: Array<Array<string>> = [];
	public items: Array<UmbTreeItemModel> = [];
	public total?: number;
	/** Items resolvable by unique, used by `requestTreeItems`. */
	public itemsByUnique: Array<UmbTreeItemModel> = [];

	/** Mirrors UmbTreeRepositoryBase, where the method always exists but reports a data source without support. */
	public supportsItems = true;

	async requestTreeItems(args: { uniques: Array<string> }) {
		this.itemsCalls.push(args.uniques);

		if (!this.supportsItems) {
			return { data: undefined, error: new UmbTreeItemsNotSupportedError() };
		}

		const data = args.uniques
			.map((unique) => this.itemsByUnique.find((item) => item.unique === unique))
			.filter((item): item is UmbTreeItemModel => item !== undefined);
		return { data };
	}

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

	describe('start nodes', () => {
		const startNodeA = { unique: 'start-node-a', entityType: 'test-entity-type' };
		const startNodeB = { unique: 'start-node-b', entityType: 'test-entity-type' };
		const startNodeC = { unique: 'start-node-c', entityType: 'test-entity-type' };

		const asItem = (node: { unique: string; entityType: string }): UmbTreeItemModel => ({
			unique: node.unique,
			entityType: node.entityType,
			name: node.unique,
			hasChildren: true,
			isFolder: false,
			parent: { unique: null, entityType: 'test-root-entity-type' },
		});

		// The states emit synchronously on subscribe, so this reads the current children.
		const currentChildren = () => {
			let children: Array<UmbTreeItemModel> = [];
			manager.children.subscribe((items) => (children = items ?? [])).unsubscribe();
			return children;
		};

		beforeEach(() => {
			repository.itemsByUnique = [startNodeA, startNodeB, startNodeC].map(asItem);
			manager.setTreeItem(treeRoot);
		});

		it('loads the start nodes themselves as children in a single request', async () => {
			manager.setStartNodes([startNodeA, startNodeB]);

			await manager.loadChildren();

			expect(repository.itemsCalls).to.have.lengthOf(1);
			expect(repository.itemsCalls[0]).to.deep.equal([startNodeA.unique, startNodeB.unique]);
			expect(repository.itemsOfCalls).to.have.lengthOf(0);
			expect(repository.rootCalls).to.equal(0);
		});

		it('keeps the requested order and drops start nodes that cannot be resolved', async () => {
			repository.itemsByUnique = [asItem(startNodeB), asItem(startNodeA)];
			manager.setStartNodes([startNodeA, startNodeB, startNodeC]);

			await manager.loadChildren();

			expect(currentChildren().map((child) => child.unique)).to.deep.equal([startNodeA.unique, startNodeB.unique]);
		});

		it('loads the children of a single start node instead of the start node itself', async () => {
			manager.setStartNodes([startNodeA]);

			await manager.loadChildren();

			expect(repository.itemsCalls).to.have.lengthOf(0);
			expect(repository.itemsOfCalls).to.have.lengthOf(1);
			expect(repository.itemsOfCalls[0].parentUnique).to.equal(startNodeA.unique);
		});

		it('falls back to the first start node when the repository reports it cannot resolve items by unique', async () => {
			// UmbTreeRepositoryBase always defines requestTreeItems and reports the lack of support through the
			// returned error, so the fallback has to be driven by that rather than by a missing method.
			repository.supportsItems = false;
			manager.setStartNodes([startNodeA, startNodeB]);

			await manager.loadChildren();

			expect(repository.itemsCalls).to.have.lengthOf(1);
			expect(repository.itemsOfCalls).to.have.lengthOf(1);
			expect(repository.itemsOfCalls[0].parentUnique).to.equal(startNodeA.unique);
		});

		it('stays on the fallback for later loads once the repository has reported no support', async () => {
			repository.supportsItems = false;
			manager.setStartNodes([startNodeA, startNodeB]);

			await manager.loadChildren();
			await manager.loadChildren();

			// Only the first load asks for items again; afterwards it goes straight to the children of the first node.
			expect(repository.itemsCalls).to.have.lengthOf(1);
			expect(repository.itemsOfCalls).to.have.lengthOf(2);
		});

		it('falls back when the repository has no support for resolving items by unique at all', async () => {
			(repository as { requestTreeItems?: unknown }).requestTreeItems = undefined;
			manager.setStartNodes([startNodeA, startNodeB]);

			await manager.loadChildren();

			expect(repository.itemsOfCalls).to.have.lengthOf(1);
			expect(repository.itemsOfCalls[0].parentUnique).to.equal(startNodeA.unique);
		});

		it('paginates the start nodes and appends the next slice', async () => {
			manager.setTakeSize(2);
			manager.setStartNodes([startNodeA, startNodeB, startNodeC]);

			await manager.loadChildren();

			expect(repository.itemsCalls[0]).to.deep.equal([startNodeA.unique, startNodeB.unique]);
			expect(manager.offsetPagination.getTotalItems()).to.equal(3);

			await manager.loadNextChildren();

			expect(repository.itemsCalls[1]).to.deep.equal([startNodeC.unique]);
			expect(currentChildren()).to.have.lengthOf(3);
		});

		it('does not request anything above the start nodes', async () => {
			manager.setStartNodes([startNodeA, startNodeB]);
			await manager.loadChildren();

			await manager.loadPrevChildren();

			expect(repository.itemsCalls).to.have.lengthOf(1);
			expect(repository.itemsOfCalls).to.have.lengthOf(0);
		});

		it('reloads the start nodes when one of them changes structure', async () => {
			manager.setStartNodes([startNodeA, startNodeB]);
			await manager.loadChildren();

			actionEventContext.dispatchEvent(
				new UmbRequestReloadStructureForEntityEvent({
					entityType: startNodeA.entityType,
					unique: startNodeA.unique,
				}),
			);

			await aTimeout(0);

			expect(repository.itemsCalls).to.have.lengthOf(2);
		});

		it('returns to the tree root items when the start nodes are removed', async () => {
			manager.setStartNodes([startNodeA, startNodeB]);
			await manager.loadChildren();

			manager.setStartNodes(undefined);
			manager.setTreeItem(undefined);
			await manager.loadChildren();

			expect(repository.rootCalls).to.equal(1);
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
