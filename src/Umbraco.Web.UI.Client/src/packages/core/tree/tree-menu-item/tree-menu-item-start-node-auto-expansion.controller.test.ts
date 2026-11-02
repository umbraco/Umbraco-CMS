import { UmbTreeMenuItemStartNodeAutoExpansionController } from './tree-menu-item-start-node-auto-expansion.controller.js';
import { aTimeout, expect, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbControllerHostElementMixin, type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_MENU_ITEM_CONTEXT, type UmbMenuItemExpansionEntryModel } from '@umbraco-cms/backoffice/menu';

const ENTITY_TYPE = 'test-entity-type';
const MENU_ITEM_ALIAS = 'Umb.Test.MenuItem';
const TREE_REPOSITORY_ALIAS = 'Umb.Test.TreeMenuItemStartNodeAutoExpansion.TreeRepository';
const OTHER_TREE_REPOSITORY_ALIAS = `${TREE_REPOSITORY_ALIAS}.Other`;
const SLOW_TREE_REPOSITORY_ALIAS = `${TREE_REPOSITORY_ALIAS}.Slow`;

const entity = (unique: string): UmbEntityModel => ({ unique, entityType: ENTITY_TYPE });

@customElement('umb-test-tree-menu-item-start-node-auto-expansion-host')
class UmbTestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

class UmbTestTreeRepository {
	static instances: Array<UmbTestTreeRepository> = [];
	static ancestors: Record<string, Array<UmbEntityModel>> = {};
	static failingUniques: Array<string> = [];
	static ancestorsGate?: Promise<void>;

	destroyed = false;

	static reset() {
		UmbTestTreeRepository.instances = [];
		UmbTestTreeRepository.ancestors = {};
		UmbTestTreeRepository.failingUniques = [];
		UmbTestTreeRepository.ancestorsGate = undefined;
	}

	constructor() {
		UmbTestTreeRepository.instances.push(this);
	}

	async requestTreeItemAncestors(args: { treeItem: { unique: string; entityType: string } }) {
		await UmbTestTreeRepository.ancestorsGate;
		if (UmbTestTreeRepository.failingUniques.includes(args.treeItem.unique)) {
			throw new Error(`Failed to request ancestors of ${args.treeItem.unique}`);
		}
		return { data: UmbTestTreeRepository.ancestors[args.treeItem.unique] ?? [] };
	}

	destroy() {
		this.destroyed = true;
	}
}

class UmbTestMenuItemContext extends UmbContextBase {
	manifest = { alias: MENU_ITEM_ALIAS };
	expandedEntries: Array<UmbMenuItemExpansionEntryModel> = [];
	expansion = {
		expandItems: (entries: Array<UmbMenuItemExpansionEntryModel>) => {
			this.expandedEntries.push(...entries);
		},
	};

	constructor(host: UmbControllerHost) {
		super(host, UMB_MENU_ITEM_CONTEXT as never);
	}
}

describe('UmbTreeMenuItemStartNodeAutoExpansionController', () => {
	let host: UmbTestHostElement;
	let menuItemContext: UmbTestMenuItemContext;
	let controller: UmbTreeMenuItemStartNodeAutoExpansionController;

	before(() => {
		for (const alias of [TREE_REPOSITORY_ALIAS, OTHER_TREE_REPOSITORY_ALIAS]) {
			umbExtensionsRegistry.register({
				type: 'repository',
				alias,
				name: 'Test Tree Repository',
				api: UmbTestTreeRepository,
			});
		}
	});

	after(() => {
		umbExtensionsRegistry.unregister(TREE_REPOSITORY_ALIAS);
		umbExtensionsRegistry.unregister(OTHER_TREE_REPOSITORY_ALIAS);
	});

	beforeEach(() => {
		UmbTestTreeRepository.reset();
		UmbTestTreeRepository.ancestors = {
			b: [entity('a'), entity('b')],
			c: [entity('c')],
		};

		host = new UmbTestHostElement();
		document.body.appendChild(host);
		menuItemContext = new UmbTestMenuItemContext(host);
		controller = new UmbTreeMenuItemStartNodeAutoExpansionController(host);
	});

	afterEach(() => {
		controller.destroy();
		document.body.removeChild(host);
		umbExtensionsRegistry.unregister(SLOW_TREE_REPOSITORY_ALIAS);
	});

	describe('start nodes', () => {
		it('are undefined until they have been set', () => {
			expect(controller.getStartNodes()).to.be.undefined;
		});

		it('can be reset to undefined', () => {
			controller.setStartNodes([entity('c')]);
			controller.setStartNodes(undefined);

			expect(controller.getStartNodes()).to.be.undefined;
		});
	});

	describe('expansion', () => {
		it('expands the menu item down to each start node through its ancestors', async () => {
			controller.setTreeRepositoryAlias(TREE_REPOSITORY_ALIAS);
			controller.setStartNodes([entity('b'), entity('c')]);

			await waitUntil(() => menuItemContext.expandedEntries.length === 3);
			expect(menuItemContext.expandedEntries).to.deep.equal([
				{ ...entity('a'), target: entity('b'), menuItemAlias: MENU_ITEM_ALIAS },
				{ ...entity('b'), menuItemAlias: MENU_ITEM_ALIAS },
				{ ...entity('c'), menuItemAlias: MENU_ITEM_ALIAS },
			]);
		});

		it('expands regardless of whether the start nodes or the repository alias is set first', async () => {
			controller.setStartNodes([entity('c')]);
			await aTimeout(0);
			expect(menuItemContext.expandedEntries).to.have.lengthOf(0);

			controller.setTreeRepositoryAlias(TREE_REPOSITORY_ALIAS);

			await waitUntil(() => menuItemContext.expandedEntries.length === 1);
		});

		it('does not expand when there are no start nodes', async () => {
			controller.setTreeRepositoryAlias(TREE_REPOSITORY_ALIAS);
			controller.setStartNodes([]);
			await aTimeout(0);

			expect(menuItemContext.expandedEntries).to.have.lengthOf(0);
		});

		it('keeps expanding the remaining start nodes when one cannot be resolved', async () => {
			UmbTestTreeRepository.failingUniques = ['b'];
			controller.setTreeRepositoryAlias(TREE_REPOSITORY_ALIAS);
			controller.setStartNodes([entity('b'), entity('c')]);

			await waitUntil(() => menuItemContext.expandedEntries.length === 1);
			expect(menuItemContext.expandedEntries).to.deep.equal([{ ...entity('c'), menuItemAlias: MENU_ITEM_ALIAS }]);
		});

		it('does not apply start nodes that were replaced while their ancestors were being requested', async () => {
			let releaseAncestors!: () => void;
			UmbTestTreeRepository.ancestorsGate = new Promise<void>((resolve) => (releaseAncestors = resolve));
			controller.setTreeRepositoryAlias(TREE_REPOSITORY_ALIAS);
			controller.setStartNodes([entity('b')]);
			await aTimeout(0);

			controller.setStartNodes([entity('c')]);
			releaseAncestors();

			await waitUntil(() => menuItemContext.expandedEntries.length === 1);
			await aTimeout(0);
			expect(menuItemContext.expandedEntries).to.deep.equal([{ ...entity('c'), menuItemAlias: MENU_ITEM_ALIAS }]);
		});

		it('does not throw when the repository alias is not registered', async () => {
			let rejected = false;
			const onRejection = () => (rejected = true);
			window.addEventListener('unhandledrejection', onRejection);

			try {
				controller.setTreeRepositoryAlias('Umb.Test.NotRegistered');
				controller.setStartNodes([entity('c')]);
				await aTimeout(0);
			} finally {
				window.removeEventListener('unhandledrejection', onRejection);
			}

			expect(rejected).to.be.false;
			expect(menuItemContext.expandedEntries).to.have.lengthOf(0);
		});
	});

	describe('tree repository lifecycle', () => {
		it('creates one tree repository for a repository alias', async () => {
			controller.setTreeRepositoryAlias(TREE_REPOSITORY_ALIAS);
			controller.setStartNodes([entity('b')]);
			controller.setStartNodes([entity('c')]);

			await waitUntil(() => menuItemContext.expandedEntries.length > 0);
			expect(UmbTestTreeRepository.instances).to.have.lengthOf(1);
		});

		it('destroys the tree repository when the repository alias changes', async () => {
			controller.setTreeRepositoryAlias(TREE_REPOSITORY_ALIAS);
			await waitUntil(() => UmbTestTreeRepository.instances.length === 1);
			await aTimeout(0);

			controller.setTreeRepositoryAlias(OTHER_TREE_REPOSITORY_ALIAS);

			expect(UmbTestTreeRepository.instances[0].destroyed).to.be.true;
		});

		it('destroys the tree repository when the controller is destroyed', async () => {
			controller.setTreeRepositoryAlias(TREE_REPOSITORY_ALIAS);
			await waitUntil(() => UmbTestTreeRepository.instances.length === 1);
			await aTimeout(0);

			controller.destroy();

			expect(UmbTestTreeRepository.instances[0].destroyed).to.be.true;
		});

		it('destroys a tree repository that finishes being created after the repository alias changed', async () => {
			let releaseLoader!: () => void;
			const loaderReleased = new Promise<void>((resolve) => (releaseLoader = resolve));
			umbExtensionsRegistry.register({
				type: 'repository',
				alias: SLOW_TREE_REPOSITORY_ALIAS,
				name: 'Slow Test Tree Repository',
				api: async () => {
					await loaderReleased;
					return { default: UmbTestTreeRepository };
				},
			});

			controller.setTreeRepositoryAlias(SLOW_TREE_REPOSITORY_ALIAS);
			controller.setTreeRepositoryAlias(TREE_REPOSITORY_ALIAS);
			await waitUntil(() => UmbTestTreeRepository.instances.length === 1);

			releaseLoader();

			await waitUntil(() => UmbTestTreeRepository.instances.length === 2);
			await aTimeout(0);
			const [current, stale] = UmbTestTreeRepository.instances;
			expect(stale.destroyed).to.be.true;
			expect(current.destroyed).to.be.false;
		});

		it('destroys a tree repository that finishes being created after the controller was destroyed', async () => {
			let releaseLoader!: () => void;
			const loaderReleased = new Promise<void>((resolve) => (releaseLoader = resolve));
			umbExtensionsRegistry.register({
				type: 'repository',
				alias: SLOW_TREE_REPOSITORY_ALIAS,
				name: 'Slow Test Tree Repository',
				api: async () => {
					await loaderReleased;
					return { default: UmbTestTreeRepository };
				},
			});

			controller.setTreeRepositoryAlias(SLOW_TREE_REPOSITORY_ALIAS);
			controller.destroy();
			releaseLoader();

			await waitUntil(() => UmbTestTreeRepository.instances.length === 1);
			await aTimeout(0);
			expect(UmbTestTreeRepository.instances[0].destroyed).to.be.true;
		});
	});
});
