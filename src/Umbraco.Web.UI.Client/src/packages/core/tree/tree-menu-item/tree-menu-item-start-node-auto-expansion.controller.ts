import type { UmbTreeRepository } from '../data/tree-repository.interface.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_MENU_ITEM_CONTEXT } from '@umbraco-cms/backoffice/menu';
import { linkEntityExpansionEntries } from '@umbraco-cms/backoffice/utils';

/**
 * Expands a tree menu item down to a set of start nodes, so the start nodes are visible in the menu.
 * @class UmbTreeMenuItemStartNodeAutoExpansionController
 * @augments {UmbControllerBase}
 */
export class UmbTreeMenuItemStartNodeAutoExpansionController extends UmbControllerBase {
	#menuItemContext?: typeof UMB_MENU_ITEM_CONTEXT.TYPE;
	#treeRepositoryAlias?: string;
	#treeRepository?: UmbTreeRepository;
	#treeRepositoryPromise?: Promise<UmbTreeRepository | undefined>;
	#treeRepositoryRequest = 0;
	#startNodes?: Array<UmbEntityModel>;
	#expansionRun = 0;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_MENU_ITEM_CONTEXT, (context) => {
			this.#menuItemContext = context;
			this.#expand();
		});
	}

	/**
	 * Sets the alias of the tree repository used to look up the ancestors of the start nodes.
	 * @param {string | undefined} alias - The tree repository alias
	 */
	setTreeRepositoryAlias(alias: string | undefined): void {
		if (alias === this.#treeRepositoryAlias) return;
		this.#treeRepositoryAlias = alias;
		this.#destroyTreeRepository();
		this.#treeRepositoryPromise = alias ? this.#createTreeRepository(alias) : undefined;
		this.#expand();
	}

	/**
	 * Gets the alias of the tree repository used to look up the ancestors of the start nodes.
	 * @returns {string | undefined} The tree repository alias
	 */
	getTreeRepositoryAlias(): string | undefined {
		return this.#treeRepositoryAlias;
	}

	/**
	 * Sets the start nodes to expand the menu item down to.
	 * @param {Array<UmbEntityModel> | undefined} startNodes - The start nodes, an empty array for none, or undefined when not yet known
	 */
	setStartNodes(startNodes: Array<UmbEntityModel> | undefined): void {
		this.#startNodes = startNodes;
		this.#expand();
	}

	/**
	 * Gets the start nodes the menu item is expanded down to.
	 * @returns {Array<UmbEntityModel> | undefined} The start nodes, an empty array for none, or undefined when not yet known
	 */
	getStartNodes(): Array<UmbEntityModel> | undefined {
		return this.#startNodes;
	}

	async #expand() {
		const run = ++this.#expansionRun;

		const menuItemAlias = this.#menuItemContext?.manifest?.alias;
		const startNodes = this.#startNodes;
		if (!menuItemAlias || !this.#treeRepositoryPromise || !startNodes?.length) return;

		const treeRepository = await this.#treeRepositoryPromise;
		if (!treeRepository || run !== this.#expansionRun) return;

		for (const startNode of startNodes) {
			if (startNode.unique === null) continue;

			try {
				const { data: ancestors } = await treeRepository.requestTreeItemAncestors({
					treeItem: { unique: startNode.unique, entityType: startNode.entityType },
				});
				// A newer alias or set of start nodes supersedes this run.
				if (run !== this.#expansionRun) return;
				if (!ancestors || ancestors.length === 0) continue;

				const entries = linkEntityExpansionEntries(ancestors).map((entry) => ({ ...entry, menuItemAlias }));
				this.#menuItemContext?.expansion.expandItems(entries);
			} catch {
				// A start node that can't be resolved is skipped so the remaining start nodes can still expand.
			}
		}
	}

	async #createTreeRepository(alias: string): Promise<UmbTreeRepository | undefined> {
		const request = this.#treeRepositoryRequest;
		let treeRepository: UmbTreeRepository;
		try {
			treeRepository = await createExtensionApiByAlias<UmbTreeRepository>(this, alias);
		} catch {
			return undefined;
		}

		// The alias changed, or the controller was destroyed, while the repository was being created.
		if (request !== this.#treeRepositoryRequest) {
			treeRepository.destroy();
			return undefined;
		}

		this.#treeRepository = treeRepository;
		return treeRepository;
	}

	#destroyTreeRepository() {
		this.#treeRepositoryRequest++;
		this.#treeRepository?.destroy();
		this.#treeRepository = undefined;
		this.#treeRepositoryPromise = undefined;
	}

	override destroy(): void {
		this.#expansionRun++;
		this.#destroyTreeRepository();
		this.#menuItemContext = undefined;
		this.#startNodes = undefined;
		super.destroy();
	}
}
