import { UmbTreeItemPickerLocationManager } from './tree-item-picker-location.manager.js';
import type { UmbTreeItemModel } from '../types.js';
import { aTimeout, expect, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbInteractionMemoryManager } from '@umbraco-cms/backoffice/interaction-memory';

const TREE_ALIAS = 'Umb.Test.LocationManager.Tree';
const REPOSITORY_ALIAS = 'Umb.Test.LocationManager.TreeRepository';
const UNRESOLVABLE_TREE_ALIAS = 'Umb.Test.LocationManager.TreeWithoutRepository';
const LOCATION_MEMORY_UNIQUE = 'UmbTreeItemPickerLocation';

/** Long enough for a restore to complete against the immediate test repository. */
const RESTORE_WAIT = 50;

const ROOT = { unique: null, entityType: 'test-root', name: 'Root' };

function item(unique: string, name: string): UmbTreeItemModel {
	return {
		unique,
		entityType: 'test-item',
		name,
		hasChildren: true,
		isFolder: false,
		parent: { unique: null, entityType: 'test-root' },
	};
}

const A = item('a', 'A');
const B = item('b', 'B');
const C = item('c', 'C');
const ELSEWHERE = item('elsewhere', 'Elsewhere');

/** Ancestors per node, the response ending with the requested node itself. */
const ANCESTORS: Record<string, Array<UmbTreeItemModel>> = {
	a: [A],
	b: [A, B],
	c: [A, B, C],
	elsewhere: [ELSEWHERE],
};

class UmbTestTreeRepository {
	async requestTreeRoot() {
		return { data: ROOT };
	}

	async requestTreeItemAncestors({ treeItem }: { treeItem: { unique: string } }) {
		return { data: ANCESTORS[treeItem.unique] ?? [] };
	}

	destroy() {}
}

@customElement('test-location-manager-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbTreeItemPickerLocationManager', () => {
	let host: UmbTestControllerHostElement;
	let memory: UmbInteractionMemoryManager;
	let manager: UmbTreeItemPickerLocationManager;

	before(() => {
		umbExtensionsRegistry.registerMany([
			{ type: 'repository', alias: REPOSITORY_ALIAS, name: 'Test Tree Repository', api: UmbTestTreeRepository },
			{ type: 'tree', alias: TREE_ALIAS, name: 'Test Tree', meta: { repositoryAlias: REPOSITORY_ALIAS } },
			{
				type: 'tree',
				alias: UNRESOLVABLE_TREE_ALIAS,
				name: 'Test Tree Without Repository',
				meta: { repositoryAlias: 'Umb.Test.LocationManager.Missing' },
			},
		] as Array<never>);
	});

	after(() => {
		umbExtensionsRegistry.unregister(TREE_ALIAS);
		umbExtensionsRegistry.unregister(UNRESOLVABLE_TREE_ALIAS);
		umbExtensionsRegistry.unregister(REPOSITORY_ALIAS);
	});

	beforeEach(() => {
		host = new UmbTestControllerHostElement();
		document.body.appendChild(host);
		memory = new UmbInteractionMemoryManager(host);
		manager = new UmbTreeItemPickerLocationManager(host, { interactionMemoryManager: memory });
	});

	afterEach(() => {
		host.remove();
	});

	const trail = () => manager.getBreadcrumb().map((crumb) => crumb.name);

	async function start(treeAlias = TREE_ALIAS) {
		manager.setTreeAlias(treeAlias);
		await waitUntil(() => manager.getBreadcrumb().length > 0, 'the initial trail was never loaded');
	}

	describe('starting from the tree root', () => {
		beforeEach(() => start());

		it('puts the root in the trail', () => {
			expect(trail()).to.eql(['Root']);
		});

		// The root is a place like any other, which is what lets an undefined location mean only "not established yet".
		it('reports the tree root as the location', () => {
			expect(manager.getCurrentLocation()).to.eql(ROOT);
		});
	});

	describe('browsing to a node', () => {
		beforeEach(async () => {
			await start();
			await manager.navigateTo({ unique: 'c', entityType: 'test-item' });
		});

		it('builds the trail below the root', () => {
			expect(trail()).to.eql(['Root', 'A', 'B', 'C']);
		});

		// The location is the tree's own model of the node, so anything a host derives from it — an icon, a content type,
		// a collection — describes the node being browsed and cannot describe another.
		it('reports the tree item of the node', () => {
			expect(manager.getCurrentLocation()).to.eql(C);
		});

		it('remembers where it is', () => {
			expect(memory.getMemory(LOCATION_MEMORY_UNIQUE)?.value?.entity).to.eql({
				unique: 'c',
				entityType: 'test-item',
			});
		});
	});

	describe('browsing back through the trail', () => {
		beforeEach(async () => {
			await start();
			await manager.navigateTo({ unique: 'c', entityType: 'test-item' });
		});

		it('drops everything below the step it returns to', async () => {
			await manager.navigateTo({ unique: 'a', entityType: 'test-item' });

			expect(trail()).to.eql(['Root', 'A']);
		});

		// A step taken from the trail has to carry the same item a freshly resolved one would, or a host deriving anything
		// from it describes the wrong node.
		it('reports the tree item of the step it returns to', async () => {
			await manager.navigateTo({ unique: 'b', entityType: 'test-item' });

			expect(manager.getCurrentLocation()).to.eql(B);
		});

		it('is back at the root when it returns to it', async () => {
			await manager.navigateTo();

			expect(trail()).to.eql(['Root']);
			expect(manager.getCurrentLocation()).to.eql(ROOT);
		});

		it('forgets where it was when it returns to the root', async () => {
			await manager.navigateTo();

			expect(memory.getMemory(LOCATION_MEMORY_UNIQUE)).to.be.undefined;
		});

		it('rebuilds the trail for a node in another branch', async () => {
			await manager.navigateTo({ unique: 'elsewhere', entityType: 'test-item' });

			expect(trail()).to.eql(['Root', 'Elsewhere']);
			expect(manager.getCurrentLocation()).to.eql(ELSEWHERE);
		});
	});

	// A location the tree cannot describe is worse than no location at all: a host would render a level it knows nothing
	// about. So the location is dropped and the reason reported instead.
	describe('browsing to a node the tree does not have', () => {
		beforeEach(async () => {
			await start();
			await manager.navigateTo({ unique: 'c', entityType: 'test-item' });
			await manager.navigateTo({ unique: 'gone', entityType: 'test-item' });
		});

		// Null rather than undefined: something is wrong, which a host must not read as still loading.
		it('reports the location as null', () => {
			expect(manager.getCurrentLocation()).to.be.null;
		});

		// The trail is what tells the user where they came from, so it is the one thing worth keeping.
		it('keeps the trail it came from', () => {
			expect(trail()).to.eql(['Root', 'A', 'B', 'C']);
		});

		it('forgets the node so the next session does not open on it', () => {
			expect(memory.getMemory(LOCATION_MEMORY_UNIQUE)).to.be.undefined;
		});

		it('recovers once somewhere else is browsed', async () => {
			await manager.navigateTo({ unique: 'a', entityType: 'test-item' });

			expect(manager.getCurrentLocation()).to.eql(A);
		});
	});

	describe('with a start node', () => {
		beforeEach(async () => {
			manager.setStartNode({ unique: 'b', entityType: 'test-item' });
			await start();
		});

		it('cuts the trail off at the start node', () => {
			expect(trail()).to.eql(['B']);
		});

		it('keeps the ceiling when browsing deeper', async () => {
			await manager.navigateTo({ unique: 'c', entityType: 'test-item' });

			expect(trail()).to.eql(['B', 'C']);
		});

		it('returns to the start node rather than nowhere when browsing to no node', async () => {
			await manager.navigateTo({ unique: 'c', entityType: 'test-item' });

			await manager.navigateTo();

			expect(trail()).to.eql(['B']);
			expect(manager.getCurrentLocation()).to.include({ unique: 'b', entityType: 'test-item' });
		});
	});

	describe('restoring the remembered location', () => {
		it('returns to where it was left', async () => {
			memory.setMemory({
				unique: LOCATION_MEMORY_UNIQUE,
				value: { entity: { unique: 'b', entityType: 'test-item' } },
			});

			await start();
			await waitUntil(() => manager.getCurrentLocation()?.unique === 'b', 'the location was never restored');

			expect(trail()).to.eql(['Root', 'A', 'B']);
		});

		// The positive control for the test below: it proves a restore does complete within the wait both use, so a
		// refusal cannot be mistaken for a restore that simply had not happened yet.
		it('restores a location inside the start node', async () => {
			memory.setMemory({
				unique: LOCATION_MEMORY_UNIQUE,
				value: { entity: { unique: 'c', entityType: 'test-item' } },
			});
			manager.setStartNode({ unique: 'b', entityType: 'test-item' });

			await start();
			await aTimeout(RESTORE_WAIT);

			expect(manager.getCurrentLocation()?.unique).to.equal('c');
			expect(trail()).to.eql(['B', 'C']);
		});

		// Opening a picker is not a request for the remembered node, so its disappearance is not reported as one.
		it('opens at the start rather than reporting a dead end when the remembered node is gone', async () => {
			memory.setMemory({
				unique: LOCATION_MEMORY_UNIQUE,
				value: { entity: { unique: 'gone', entityType: 'test-item' } },
			});

			await start();
			await aTimeout(RESTORE_WAIT);

			expect(manager.getCurrentLocation()).to.eql(ROOT);
			expect(memory.getMemory(LOCATION_MEMORY_UNIQUE)).to.be.undefined;
		});

		// A picker restricted to a subtree must not be restored to somewhere outside it.
		it('refuses a remembered location outside the start node', async () => {
			memory.setMemory({
				unique: LOCATION_MEMORY_UNIQUE,
				value: { entity: { unique: 'elsewhere', entityType: 'test-item' } },
			});
			manager.setStartNode({ unique: 'b', entityType: 'test-item' });

			await start();
			await aTimeout(RESTORE_WAIT);

			expect(manager.getCurrentLocation()?.unique).to.equal('b');
			expect(trail()).to.eql(['B']);
		});
	});

	describe('when the tree has no repository', () => {
		it('reports the node as not found rather than guessing at it', async () => {
			manager.setTreeAlias(UNRESOLVABLE_TREE_ALIAS);

			await manager.navigateTo({ unique: 'a', entityType: 'test-item' });

			expect(manager.getCurrentLocation()).to.be.null;
		});
	});
});
