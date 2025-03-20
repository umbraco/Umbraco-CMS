import type { UmbTreeExpansionModel } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * Manages the expansion state of a tree
 * @exports
 * @class UmbTreeExpansionManager
 * @augments {UmbControllerBase}
 */
export class UmbTreeExpansionManager extends UmbControllerBase {
	#expansion = new UmbArrayState<UmbEntityModel>([], (x) => x.unique);
	expansion = this.#expansion.asObservable();

	/**
	 * Sets the expansion state
	 * @param {UmbTreeExpansionModel | undefined} expansion The expansion state
	 * @memberof UmbTreeExpansionManager
	 * @returns {void}
	 */
	setExpansion(expansion: UmbTreeExpansionModel): void {
		this.#expansion.setValue(expansion);
	}

	/**
	 * Gets the expansion state
	 * @memberof UmbTreeExpansionManager
	 * @returns {UmbTreeExpansionModel} The expansion state
	 */
	getExpansion(): UmbTreeExpansionModel {
		return this.#expansion.getValue();
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
		this.#expansion.appendOne(entity);
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
		this.#expansion.filter((x) => x.entityType !== entity.entityType || x.unique !== entity.unique);
	}

	/**
	 * Closes all child tree items
	 * @memberof UmbTreeExpansionManager
	 * @returns {Promise<void>}
	 */
	public async collapseAll(): Promise<void> {
		this.#expansion.setValue([]);
	}
}
