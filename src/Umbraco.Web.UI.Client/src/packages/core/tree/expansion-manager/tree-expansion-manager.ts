import type { UmbTreeExpansionModel } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import { UmbEntityExpansionManager } from '@umbraco-cms/backoffice/utils';

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
	public async expandItem(entity: UmbEntityModel): Promise<void> {
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
}
