import type { UmbTreeExpansionModel } from './types.js';
import type { UmbTreeRepository } from '../data/tree-repository.interface.js';
import type { UmbTreeStartNode } from '../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbEntityExpansionManager, linkEntityExpansionEntries } from '@umbraco-cms/backoffice/utils';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbEntityExpansionEntryModel } from '@umbraco-cms/backoffice/utils';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

/**
 * Manages the expansion state of a tree
 * @exports
 * @class UmbTreeExpansionManager
 * @augments {UmbControllerBase}
 */
export class UmbTreeExpansionManager extends UmbControllerBase {
	#manager = new UmbEntityExpansionManager(this);
	expansion = this.#manager.expansion;

	/**
	 * Observe the expansion entry for a specific entity
	 * @param {UmbEntityModel} entity - The entity to observe
	 * @returns {Observable<UmbEntityExpansionEntryModel | undefined>} - An observable of the expansion entry
	 * @memberof UmbTreeExpansionManager
	 */
	entry(entity: UmbEntityModel): Observable<UmbEntityExpansionEntryModel | undefined> {
		return this.#manager.entry(entity);
	}

	/**
	 * Checks if an entity is expanded
	 * @param {UmbEntityModel} entity The entity to check
	 * @param {string} entity.entityType The entity type
	 * @param {string} entity.unique The unique key
	 * @returns {Observable<boolean>} True if the entity is expanded
	 * @memberof UmbTreeExpansionManager
	 */
	isExpanded(entity: UmbEntityModel): Observable<boolean> {
		return this.#manager.isExpanded(entity);
	}

	/**
	 * Sets the expansion state
	 * @param {UmbTreeExpansionModel | undefined} expansion The expansion state
	 * @memberof UmbTreeExpansionManager
	 * @returns {void}
	 */
	setExpansion(expansion: UmbTreeExpansionModel): void {
		this.#manager.setExpansion(expansion);
	}

	/**
	 * Gets the expansion state
	 * @memberof UmbTreeExpansionManager
	 * @returns {UmbTreeExpansionModel} The expansion state
	 */
	getExpansion(): UmbTreeExpansionModel {
		return this.#manager.getExpansion();
	}

	/**
	 * Opens a child tree item
	 * @param {UmbEntityModel} entity The entity to open
	 * @param {string} entity.entityType The entity type
	 * @param {string} entity.unique The unique key
	 * @memberof UmbTreeExpansionManager
	 * @returns {Promise<void>}
	 */
	public async expandItem(entity: UmbEntityExpansionEntryModel): Promise<void> {
		this.#manager.expandItem(entity);
	}

	/**
	 * Closes a child tree item
	 * @param {UmbEntityModel} entity The entity to close
	 * @param {string} entity.entityType The entity type
	 * @param {string} entity.unique The unique key
	 * @memberof UmbTreeExpansionManager
	 * @returns {Promise<void>}
	 */
	public async collapseItem(entity: UmbEntityModel): Promise<void> {
		this.#manager.collapseItem(entity);
	}

	/**
	 * Closes all child tree items
	 * @memberof UmbTreeExpansionManager
	 * @returns {Promise<void>}
	 */
	public async collapseAll(): Promise<void> {
		this.#manager.collapseAll();
	}

	/**
	 * Gets a tree item from the expansion state
	 * @param {UmbEntityModel} entity The entity to get
	 * @param {string} entity.entityType The entity type
	 * @param {string} entity.unique The unique key
	 * @returns {*}  {(Promise<UmbEntityExpansionEntryModel | undefined>)}
	 * @memberof UmbEntityExpansionManager
	 */
	public async getItem(entity: UmbEntityModel): Promise<UmbEntityExpansionEntryModel | undefined> {
		return this.#manager.getItem(entity);
	}

	/**
	 * Expands the tree to the given entity by fetching its ancestors from the repository,
	 * building a linked chain, and replacing the current expansion state.
	 * If a startNode is provided the chain is clipped to begin at that node.
	 * On repository error the expansion state is left unchanged.
	 * @param {UmbEntityModel} entity The target entity to expand to
	 * @param {{ repository: UmbTreeRepository; startNode?: UmbTreeStartNode }} options
	 * @memberof UmbTreeExpansionManager
	 */
	public async expandTo(
		entity: { unique: string; entityType: string },
		options: { repository: UmbTreeRepository; startNode?: UmbTreeStartNode },
	): Promise<void> {
		const { data, error } = await options.repository.requestTreeItemAncestors({ treeItem: entity });
		if (error || !data) return;

		let chain: Array<UmbEntityModel> = [...data, entity];

		if (options.startNode) {
			const startIndex = chain.findIndex(
				(item) => item.unique === options.startNode!.unique && item.entityType === options.startNode!.entityType,
			);
			if (startIndex !== -1) {
				chain = chain.slice(startIndex);
			}
		}

		this.setExpansion(linkEntityExpansionEntries(chain));
	}
}
